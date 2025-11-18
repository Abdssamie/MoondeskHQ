using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces;

/// <summary>
/// Repository for managing assets
/// </summary>
public interface IAssetRepository
{
    Task<IEnumerable<Asset>> GetAllAsync();
    
    Task<Asset?> GetByIdAsync(int id);
    
    Task<Asset?> GetByIdWithSensorsAsync(int id);
    
    Task<Asset> AddAsync(Asset asset);
    
    Task UpdateAsync(Asset asset);
    
    Task DeleteAsync(int id);
    
    Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
    
    Task<IEnumerable<Asset>> GetAssetsByTypeAsync(string type);
}
