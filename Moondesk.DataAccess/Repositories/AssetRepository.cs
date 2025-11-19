using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<AssetRepository> _logger;

    public AssetRepository(MoondeskDbContext context, ILogger<AssetRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        try
        {
            return await _context.Assets
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all assets");
            throw;
        }
    }

    public async Task<Asset?> GetByIdAsync(long id)
    {
        try
        {
            return await _context.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with ID: {AssetId}", id);
            throw;
        }
    }

    public async Task<Asset?> GetByIdWithSensorsAsync(long id)
    {
        try
        {
            return await _context.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with sensors for ID: {AssetId}", id);
            throw;
        }
    }

    public async Task<Asset> AddAsync(Asset asset)
    {
        if (asset == null)
            throw new ArgumentNullException(nameof(asset));

        try
        {
            _logger.LogInformation("Creating asset: {AssetName}", asset.Name);

            // Convert to extended model for database storage
            var assetExtended = new Asset
            {
                Name = asset.Name,
                Type = asset.Type,
                Location = asset.Location,
                Status = asset.Status,
                LastSeen = asset.LastSeen,
                Description = asset.Description,
                Manufacturer = asset.Manufacturer,
                ModelNumber = asset.ModelNumber,
                InstallationDate = asset.InstallationDate,
                Metadata = asset.Metadata,
                OrganizationId = "temp" // This should be set by the service layer
            };

            _context.Assets.Add(assetExtended);
            await _context.SaveChangesAsync();

            asset.Id = assetExtended.Id;
            return asset;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset: {AssetName}", asset?.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Asset asset)
    {
        if (asset == null)
            throw new ArgumentNullException(nameof(asset));

        try
        {
            var existing = await _context.Assets.FindAsync(asset.Id);
            if (existing == null)
                throw new ArgumentException($"Asset with ID {asset.Id} not found");

            // Update properties
            existing.Name = asset.Name;
            existing.Type = asset.Type;
            existing.Location = asset.Location;
            existing.Status = asset.Status;
            existing.LastSeen = asset.LastSeen;
            existing.Description = asset.Description;
            existing.Manufacturer = asset.Manufacturer;
            existing.ModelNumber = asset.ModelNumber;
            existing.InstallationDate = asset.InstallationDate;
            existing.Metadata = asset.Metadata;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset: {AssetId}", asset?.Id);
            throw;
        }
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
                throw new ArgumentException($"Asset with ID {id} not found");

            _logger.LogWarning("Deleting asset: {AssetId}", id);

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset: {AssetId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
    {
        try
        {
            return await _context.Assets
                .AsNoTracking()
                .Where(a => a.Status == status)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets by status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByTypeAsync(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Asset type cannot be null or empty", nameof(type));

        try
        {
            return await _context.Assets
                .AsNoTracking()
                .Where(a => a.Type.ToLower() == type.ToLower())
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets by type: {Type}", type);
            throw;
        }
    }
}
