using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<SensorRepository> _logger;

    public SensorRepository(MoondeskDbContext context, ILogger<SensorRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Sensor>> GetAllAsync()
    {
        try
        {
            return await _context.Sensors
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all sensors");
            throw;
        }
    }

    public async Task<IEnumerable<Sensor>> GetAllWithAssetsAsync()
    {
        try
        {
            return await _context.Sensors
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensors with assets");
            throw;
        }
    }

    public async Task<Sensor?> GetByIdAsync(long id)
    {
        try
        {
            return await _context.Sensors
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensor with ID: {SensorId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Sensor>> GetByAssetIdAsync(long assetId)
    {
        try
        {
            return await _context.Sensors
                .AsNoTracking()
                .Where(s => s.AssetId == assetId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensors for asset: {AssetId}", assetId);
            throw;
        }
    }

    public async Task<Sensor> AddAsync(Sensor sensor)
    {
        if (sensor == null)
            throw new ArgumentNullException(nameof(sensor));

        try
        {
            _logger.LogInformation("Creating sensor: {SensorName} for asset {AssetId}", sensor.Name, sensor.AssetId);

            // Convert to extended model for database storage
            var sensorExtended = new Sensor
            {
                AssetId = sensor.AssetId,
                Name = sensor.Name,
                Type = sensor.Type,
                Unit = sensor.Unit,
                ThresholdLow = sensor.ThresholdLow,
                ThresholdHigh = sensor.ThresholdHigh,
                MinValue = sensor.MinValue,
                MaxValue = sensor.MaxValue,
                SamplingIntervalMs = sensor.SamplingIntervalMs,
                IsActive = sensor.IsActive,
                Protocol = sensor.Protocol,
                Description = sensor.Description,
                Metadata = sensor.Metadata,
                OrganizationId = "temp" // This should be set by the service layer
            };

            _context.Sensors.Add(sensorExtended);
            await _context.SaveChangesAsync();

            sensor.Id = sensorExtended.Id;
            return sensor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sensor: {SensorName}", sensor?.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Sensor sensor)
    {
        if (sensor == null)
            throw new ArgumentNullException(nameof(sensor));

        try
        {
            var existing = await _context.Sensors.FindAsync(sensor.Id);
            if (existing == null)
                throw new ArgumentException($"Sensor with ID {sensor.Id} not found");

            // Update properties
            existing.Name = sensor.Name;
            existing.Type = sensor.Type;
            existing.Unit = sensor.Unit;
            existing.ThresholdLow = sensor.ThresholdLow;
            existing.ThresholdHigh = sensor.ThresholdHigh;
            existing.MinValue = sensor.MinValue;
            existing.MaxValue = sensor.MaxValue;
            existing.SamplingIntervalMs = sensor.SamplingIntervalMs;
            existing.IsActive = sensor.IsActive;
            existing.Protocol = sensor.Protocol;
            existing.Description = sensor.Description;
            existing.Metadata = sensor.Metadata;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sensor: {SensorId}", sensor?.Id);
            throw;
        }
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null)
                throw new ArgumentException($"Sensor with ID {id} not found");

            _logger.LogWarning("Deleting sensor: {SensorId}", id);

            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sensor: {SensorId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Sensor>> GetActiveSensorsAsync()
    {
        try
        {
            return await _context.Sensors
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sensors");
            throw;
        }
    }

    public async Task<IEnumerable<Sensor>> GetSensorsByTypeAsync(SensorType type)
    {
        try
        {
            return await _context.Sensors
                .AsNoTracking()
                .Where(s => s.Type == type)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sensors by type: {SensorType}", type);
            throw;
        }
    }
}
