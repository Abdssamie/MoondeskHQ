namespace AquaPP.Core.Models.IoT;

/// <summary>
/// Represents a single time-series reading from a sensor
/// </summary>
public class Reading
{
    public long Id { get; set; }
    
    public int SensorId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
    
    public double Value { get; set; }
    
    public ReadingQuality Quality { get; set; } = ReadingQuality.Good;
    
    public string? Notes { get; set; }
    
    // Navigation property
    public Sensor Sensor { get; set; } = null!;
}

public enum ReadingQuality
{
    Good = 0,
    Uncertain = 1,
    Bad = 2,
    Simulated = 3
}
