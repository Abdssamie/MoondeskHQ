using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for managing assets
/// </summary>
public interface IAssetRepository
{
    Task<IEnumerable<Asset>> GetAllAsync();
    
    Task<Asset?> GetByIdAsync(long id);
    
    Task<Asset?> GetByIdWithSensorsAsync(long id);
    
    Task<Asset> AddAsync(Asset asset);
    
    Task UpdateAsync(Asset asset);
    
    Task DeleteAsync(long id);
    
    Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
    
    Task<IEnumerable<Asset>> GetAssetsByTypeAsync(string type);
}
