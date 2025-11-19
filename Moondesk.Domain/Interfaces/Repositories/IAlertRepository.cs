using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces.Repositories;

public interface IAlertRepository
{
    Task<Alert> GetAlertAsync(long id);
    
    Task<IEnumerable<Alert>> GetAlertsAsync();

    Task<IEnumerable<Alert>> GetAlertsByAlertSeverityAsync(AlertSeverity severity);

    Task<Alert> CreateAlertAsync(Alert alert);

    Task<Alert> UpdateAlertAsync(long id, Alert reading);

    Task DeleteAlertAsync(long id);
    
    Task<IEnumerable<Alert>> GetAlertsBySensorAsync(long sensorId);
    
    Task<IEnumerable<Alert>> GetAlertsByProtocolAsync(Protocol protocol); 
}