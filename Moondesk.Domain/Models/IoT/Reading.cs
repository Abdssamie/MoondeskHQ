using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models.IoT;

/// <summary>
/// Represents a single time-series reading from a sensor
/// </summary>
public class Reading
{
    public long SensorId { get; set; }
    
    public required string OrganizationId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
    
    public double Value { get; set; }
    
    public Parameter Parameter { get; set; } = Parameter.None;
    
    public Protocol Protocol { get; set; }
    
    public ReadingQuality Quality { get; set; } = ReadingQuality.Good;
    
    public string? Notes { get; set; }

    public Dictionary<string, string>? Metadata { get; set; } = new();
    
    // Navigation property
    public Sensor Sensor { get; set; } = null!;
}