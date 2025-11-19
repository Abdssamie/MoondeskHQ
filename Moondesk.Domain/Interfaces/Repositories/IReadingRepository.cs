using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces.Repositories;

public interface IReadingRepository
{
    Task<Reading> GetReadingAsync(long id);
    
    Task<IEnumerable<Reading>> GetReadingsAsync();
    
    Task<IEnumerable<Reading>> GetReadingsByQualityAsync(ReadingQuality quality);
    
    Task<Reading> UpdateReadingAsync(long id, Reading reading);

    Task DeleteReadingAsync(long id);
    
    Task<IEnumerable<Reading>> GetReadingsBySensorAsync(long sensorId);
    
    Task<IEnumerable<Reading>> GetReadingsByProtocolAsync(Protocol protocol); 
}