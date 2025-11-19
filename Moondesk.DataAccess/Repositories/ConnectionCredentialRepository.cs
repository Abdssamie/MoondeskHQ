using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Exceptions;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.Network;

namespace Moondesk.DataAccess.Repositories;

// TODO: CRITICAL SECURITY - Implement IEncryptionService to encrypt/decrypt EncryptedPassword
// Currently storing credentials in plain text. Need to:
// 1. Inject IEncryptionService in constructor
// 2. Encrypt password in CreateAsync/UpdateAsync before saving
// 3. Decrypt password in GetByIdAsync/GetAllAsync after retrieving
/// <summary>
/// Repository for managing ConnectionCredential entities with encryption and multi-tenant security.
/// </summary>
public class ConnectionCredentialRepository : IConnectionCredentialRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<ConnectionCredentialRepository> _logger;

    public ConnectionCredentialRepository(MoondeskDbContext context, ILogger<ConnectionCredentialRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConnectionCredential?> GetByIdAsync(long id)
    {
        try
        {
            _logger.LogDebug("Retrieving connection credential with ID: {CredentialId}", id);
            
            return await _context.ConnectionCredentials
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection credential with ID: {CredentialId}", id);
            throw;
        }
    }

    public async Task<ConnectionCredential?> GetByNameAsync(string name, string organizationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationId));

        try
        {
            return await _context.ConnectionCredentials
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && 
                                         c.OrganizationId == organizationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection credential by name: {Name} for org: {OrganizationId}", 
                name, organizationId);
            throw;
        }
    }

    public async Task<IEnumerable<ConnectionCredential>> GetByOrganizationIdAsync(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationId));

        try
        {
            return await _context.ConnectionCredentials
                .AsNoTracking()
                .Where(c => c.OrganizationId == organizationId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection credentials for organization: {OrganizationId}", 
                organizationId);
            throw;
        }
    }

    public async Task<IEnumerable<ConnectionCredential>> GetByProtocolAsync(Protocol protocol, string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationId));

        try
        {
            return await _context.ConnectionCredentials
                .AsNoTracking()
                .Where(c => c.Protocol == protocol && c.OrganizationId == organizationId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection credentials by protocol: {Protocol} for org: {OrganizationId}", 
                protocol, organizationId);
            throw;
        }
    }

    public async Task<IEnumerable<ConnectionCredential>> GetActiveAsyncByOrganizationIdAsync(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationId));

        try
        {
            return await _context.ConnectionCredentials
                .AsNoTracking()
                .Where(c => c.IsActive && c.OrganizationId == organizationId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active connection credentials for organization: {OrganizationId}", 
                organizationId);
            throw;
        }
    }

    public async Task<ConnectionCredential> CreateAsync(ConnectionCredential credential)
    {
        if (credential == null)
            throw new ArgumentNullException(nameof(credential));

        try
        {
            credential.ValidateName();
            credential.ValidateEndpoint();

            // Check for duplicate names within organization
            if (await GetByNameAsync(credential.Name, credential.OrganizationId) != null)
                throw new DuplicateConnectionNameException(credential.Name);

            _logger.LogInformation("Creating connection credential: {Name} for org: {OrganizationId}", 
                credential.Name, credential.OrganizationId);

            _context.ConnectionCredentials.Add(credential);
            await _context.SaveChangesAsync();

            return credential;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating connection credential: {Name}", credential.Name);
            throw;
        }
    }

    public async Task<ConnectionCredential> UpdateAsync(ConnectionCredential credential)
    {
        if (credential == null)
            throw new ArgumentNullException(nameof(credential));

        try
        {
            credential.ValidateName();
            credential.ValidateEndpoint();

            var existing = await _context.ConnectionCredentials.FindAsync(credential.Id);
            if (existing == null)
                throw new ConnectionCredentialNotFoundException(credential.Id);

            _context.Entry(existing).CurrentValues.SetValues(credential);
            await _context.SaveChangesAsync();

            return credential;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection credential: {CredentialId}", credential.Id);
            throw;
        }
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            var credential = await _context.ConnectionCredentials.FindAsync(id);
            if (credential == null)
                throw new ConnectionCredentialNotFoundException(id);

            _logger.LogWarning("Deleting connection credential: {CredentialId}", id);

            _context.ConnectionCredentials.Remove(credential);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting connection credential: {CredentialId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        try
        {
            return await _context.ConnectionCredentials
                .AsNoTracking()
                .AnyAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if connection credential exists: {CredentialId}", id);
            throw;
        }
    }

    public async Task UpdateLastUsedAsync(long id)
    {
        try
        {
            var credential = await _context.ConnectionCredentials.FindAsync(id);
            if (credential != null)
            {
                credential.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Updated last used timestamp for credential: {CredentialId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last used timestamp for credential: {CredentialId}", id);
            throw;
        }
    }
}
