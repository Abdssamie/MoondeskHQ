using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for managing sensors
/// </summary>
public interface ISensorRepository
{
    Task<IEnumerable<Sensor>> GetAllAsync();
    
    Task<IEnumerable<Sensor>> GetAllWithAssetsAsync();
    
    Task<Sensor?> GetByIdAsync(long id);
    
    Task<IEnumerable<Sensor>> GetByAssetIdAsync(long assetId);
    
    Task<Sensor> AddAsync(Sensor sensor);
    
    Task UpdateAsync(Sensor sensor);
    
    Task DeleteAsync(long id);
    
    Task<IEnumerable<Sensor>> GetActiveSensorsAsync();
    
    Task<IEnumerable<Sensor>> GetSensorsByTypeAsync(SensorType type);
}
