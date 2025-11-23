using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Exceptions;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;

namespace Moondesk.DataAccess.Repositories;

/// <summary>
/// Repository for managing Organization entities with multi-tenant security and validation.
/// </summary>
public class OrganizationRepository : IOrganizationRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<OrganizationRepository> _logger;

    public OrganizationRepository(MoondeskDbContext context, ILogger<OrganizationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Organization?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(id));

        try
        {
            _logger.LogDebug("Retrieving organization with ID: {OrganizationId}", id);
            
            return await _context.Organizations
                .AsNoTracking()
                .Include(o => o.Memberships)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization with ID: {OrganizationId}", id);
            throw;
        }
    }

    public async Task<Organization?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name cannot be null or empty", nameof(name));

        try
        {
            return await _context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Name.ToLower() == name.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization with name: {OrganizationName}", name);
            throw;
        }
    }

    public async Task<IEnumerable<Organization>> GetByOwnerIdAsync(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be null or empty", nameof(ownerId));

        try
        {
            return await _context.Organizations
                .AsNoTracking()
                .Where(o => o.OwnerId == ownerId)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations for owner: {OwnerId}", ownerId);
            throw;
        }
    }

    public async Task<IEnumerable<Organization>> GetAllAsync()
    {
        try
        {
            return await _context.Organizations
                .AsNoTracking()
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all organizations");
            throw;
        }
    }

    public async Task<Organization> CreateAsync(Organization organization)
    {
        if (organization == null)
            throw new ArgumentNullException(nameof(organization));

        try
        {
            organization.ValidateName();

            if (await ExistsAsync(organization.Id))
                throw new DomainException($"Organization with ID {organization.Id} already exists");

            if (await GetByNameAsync(organization.Name) != null)
                throw new DomainException($"Organization with name '{organization.Name}' already exists");

            _logger.LogInformation("Creating organization: {OrganizationId} ({Name})", 
                organization.Id, organization.Name);

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization: {OrganizationId}", organization.Id);
            throw;
        }
    }

    public async Task<Organization> UpdateAsync(Organization organization)
    {
        if (organization == null)
            throw new ArgumentNullException(nameof(organization));

        try
        {
            organization.ValidateName();

            var existing = await _context.Organizations.FindAsync(organization.Id);
            if (existing == null)
                return organization;

            _context.Entry(existing).CurrentValues.SetValues(organization);
            await _context.SaveChangesAsync();

            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization: {OrganizationId}", organization.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(id));

        try
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
                return;

            _logger.LogWarning("Deleting organization: {OrganizationId}", id);

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization: {OrganizationId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        try
        {
            return await _context.Organizations
                .AsNoTracking()
                .AnyAsync(o => o.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if organization exists: {OrganizationId}", id);
            throw;
        }
    }
}
