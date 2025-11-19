using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Repositories;

/// <summary>
/// High-performance repository for IoT sensor readings with TimescaleDB optimizations.
/// Implements bulk operations, time-series queries, and aggregation support.
/// </summary>
public class ReadingRepository : IReadingRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<ReadingRepository> _logger;

    public ReadingRepository(MoondeskDbContext context, ILogger<ReadingRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Reading> GetReadingAsync(long id)
    {
        try
        {
            var reading = await _context.Readings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            
            return reading ?? throw new ArgumentException($"Reading with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reading with ID: {ReadingId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Reading>> GetReadingsAsync()
    {
        try
        {
            return await _context.Readings
                .AsNoTracking()
                .OrderByDescending(r => r.Timestamp)
                .Take(1000) // Limit for performance
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving readings");
            throw;
        }
    }

    public async Task<IEnumerable<Reading>> GetReadingsByQualityAsync(ReadingQuality quality)
    {
        try
        {
            return await _context.Readings
                .AsNoTracking()
                .Where(r => r.Quality == quality)
                .OrderByDescending(r => r.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving readings by quality: {Quality}", quality);
            throw;
        }
    }

    public async Task<Reading> UpdateReadingAsync(long id, Reading reading)
    {
        try
        {
            var existing = await _context.Readings.FindAsync(id);
            if (existing == null)
                throw new ArgumentException($"Reading with ID {id} not found");

            _context.Entry(existing).CurrentValues.SetValues(reading);
            await _context.SaveChangesAsync();
            
            return reading;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reading: {ReadingId}", id);
            throw;
        }
    }

    public async Task DeleteReadingAsync(long id)
    {
        try
        {
            var reading = await _context.Readings.FindAsync(id);
            if (reading == null)
                throw new ArgumentException($"Reading with ID {id} not found");

            _context.Readings.Remove(reading);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reading: {ReadingId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Reading>> GetReadingsBySensorAsync(long sensorId)
    {
        try
        {
            return await _context.Readings
                .AsNoTracking()
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving readings for sensor: {SensorId}", sensorId);
            throw;
        }
    }

    public async Task<IEnumerable<Reading>> GetReadingsByProtocolAsync(Protocol protocol)
    {
        try
        {
            return await _context.Readings
                .AsNoTracking()
                .Where(r => r.Protocol == protocol)
                .OrderByDescending(r => r.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving readings by protocol: {Protocol}", protocol);
            throw;
        }
    }

    /// <summary>
    /// Retrieves recent readings for a sensor with organization scoping.
    /// Optimized for real-time dashboards and monitoring.
    /// </summary>
    public async Task<IEnumerable<Reading>> GetRecentReadingsAsync(
        string organizationId, long sensorId, int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentException("Organization ID cannot be null or empty", nameof(organizationId));
        if (limit <= 0 || limit > 10000)
            throw new ArgumentException("Limit must be between 1 and 10000", nameof(limit));

        try
        {
            _logger.LogDebug("Retrieving {Limit} recent readings for sensor {SensorId} in org {OrganizationId}", 
                limit, sensorId, organizationId);

            return await _context.Readings
                .AsNoTracking()
                .Where(r => r.OrganizationId == organizationId && r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent readings for sensor {SensorId}", sensorId);
            throw;
        }
    }

    /// <summary>
    /// Performs high-performance bulk insert of readings using EF Core batch operations.
    /// Optimized for IoT data ingestion with minimal overhead.
    /// </summary>
    public async Task BulkInsertReadingsAsync(IEnumerable<Reading> readings)
    {
        if (readings == null)
            throw new ArgumentNullException(nameof(readings));

        var readingsList = readings.ToList();
        if (!readingsList.Any())
            return;

        try
        {
            _logger.LogInformation("Bulk inserting {Count} readings", readingsList.Count);

            // Validate organization IDs are present
            var invalidReadings = readingsList.Where(r => string.IsNullOrWhiteSpace(r.OrganizationId)).ToList();
            if (invalidReadings.Any())
                throw new ArgumentException("All readings must have an organization ID");

            await _context.Readings.AddRangeAsync(readingsList);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully inserted {Count} readings", readingsList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk inserting {Count} readings", readingsList.Count);
            throw;
        }
    }
}
