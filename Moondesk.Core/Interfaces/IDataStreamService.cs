using AquaPP.Core.Models.IoT;

namespace AquaPP.Core.Interfaces;

/// <summary>
/// Service for streaming real-time sensor data
/// </summary>
public interface IDataStreamService
{
    /// <summary>
    /// Stream real-time readings from a specific sensor
    /// </summary>
    IObservable<Reading> StreamReadings(int sensorId);
    
    /// <summary>
    /// Stream readings from all sensors of a specific asset
    /// </summary>
    IObservable<Reading> StreamAssetReadings(int assetId);
    
    /// <summary>
    /// Get historical readings for a sensor within a time range
    /// </summary>
    Task<IEnumerable<Reading>> GetHistoricalReadingsAsync(
        int sensorId, 
        DateTimeOffset from, 
        DateTimeOffset to);
    
    /// <summary>
    /// Start streaming data for all active sensors
    /// </summary>
    Task StartStreamingAsync();
    
    /// <summary>
    /// Stop all data streams
    /// </summary>
    Task StopStreamingAsync();
    
    /// <summary>
    /// Check if the service is currently streaming
    /// </summary>
    bool IsStreaming { get; }
}
