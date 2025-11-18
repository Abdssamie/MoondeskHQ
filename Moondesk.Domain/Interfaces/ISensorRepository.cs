using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces;

/// <summary>
/// Repository for managing sensors
/// </summary>
public interface ISensorRepository
{
    Task<IEnumerable<Sensor>> GetAllAsync();
    
    Task<IEnumerable<Sensor>> GetAllWithAssetsAsync();
    
    Task<Sensor?> GetByIdAsync(int id);
    
    Task<IEnumerable<Sensor>> GetByAssetIdAsync(int assetId);
    
    Task<Sensor> AddAsync(Sensor sensor);
    
    Task UpdateAsync(Sensor sensor);
    
    Task DeleteAsync(int id);
    
    Task<IEnumerable<Sensor>> GetActiveSensorsAsync();
    
    Task<IEnumerable<Sensor>> GetSensorsByTypeAsync(SensorType type);
}
