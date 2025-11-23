using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<AlertRepository> _logger;

    public AlertRepository(MoondeskDbContext context, ILogger<AlertRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Alert?> GetAlertAsync(long id)
    {
        try
        {
            var alert = await _context.Alerts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Alert>> GetAlertsAsync()
    {
        try
        {
            return await _context.Alerts
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts");
            throw;
        }
    }

    public async Task<Alert> CreateAlertAsync(Alert alert)
    {
        try
        {
            // check if id already exists
            var existing = await _context.Alerts.FindAsync(alert.Id);
            if (existing != null)
            {
                throw new ArgumentException($"Alert with id {alert.Id} already exists in the database");
            }

            await _context.Alerts.AddAsync(alert);

            await _context.SaveChangesAsync();

            return alert;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating alert: {AlertId}", alert.Id);
            throw;
        }
    }

    public async Task<IEnumerable<Alert>> GetAlertsByAlertSeverityAsync(AlertSeverity severity)
    {
        try
        {
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.Severity == severity)
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts by severity: {Severity}", severity);
            throw;
        }
    }

    public async Task<Alert> UpdateAlertAsync(long id, Alert alert)
    {
        try
        {
            var existing = await _context.Alerts.FindAsync(id);
            if (existing == null)
                throw new ArgumentException($"Alert with ID {id} not found");

            _context.Entry(existing).CurrentValues.SetValues(alert);
            await _context.SaveChangesAsync();
            
            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alert: {AlertId}", id);
            throw;
        }
    }

    public async Task DeleteAlertAsync(long id)
    {
        try
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null)
                throw new ArgumentException($"Alert with ID {id} not found");

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting alert: {AlertId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Alert>> GetAlertsBySensorAsync(long sensorId)
    {
        try
        {
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.SensorId == sensorId)
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts for sensor: {SensorId}", sensorId);
            throw;
        }
    }

    public async Task<IEnumerable<Alert>> GetAlertsByProtocolAsync(Protocol protocol)
    {
        try
        {
            return await _context.Alerts
                .AsNoTracking()
                .Where(a => a.Protocol == protocol)
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts by protocol: {Protocol}", protocol);
            throw;
        }
    }
}
