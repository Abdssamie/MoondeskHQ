using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces.Repositories;

public interface IReadingRepository
{
    Task<Reading> GetReadingAsync(long sensorId, DateTimeOffset timestamp);
    
    Task<IEnumerable<Reading>> GetReadingsAsync();
    
    Task<IEnumerable<Reading>> GetReadingsByQualityAsync(ReadingQuality quality);
    
    Task<Reading> UpdateReadingAsync(long sensorId, DateTimeOffset timestamp, Reading reading);

    Task DeleteReadingAsync(long sensorId, DateTimeOffset timestamp);
    
    Task<IEnumerable<Reading>> GetReadingsBySensorAsync(long sensorId);
    
    Task<IEnumerable<Reading>> GetReadingsByProtocolAsync(Protocol protocol);
    
    Task<IEnumerable<Reading>> GetRecentReadingsAsync(string organizationId, long sensorId, int limit = 100);
    
    Task BulkInsertReadingsAsync(IEnumerable<Reading> readings);
}