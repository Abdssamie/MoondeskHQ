using System;
using System.ComponentModel.DataAnnotations;

namespace AquaPP.Core.Models.IoT;

/// <summary>
/// Represents an alert triggered when sensor readings exceed thresholds
/// </summary>
public class Alert
{
    public int Id { get; set; }
    
    public int SensorId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
    
    public AlertSeverity Severity { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public double TriggerValue { get; set; }
    
    public double? ThresholdValue { get; set; }
    
    public bool Acknowledged { get; set; }
    
    public DateTimeOffset? AcknowledgedAt { get; set; }
    
    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }
    
    [MaxLength(100)]
    public string? Notes { get; set; }
    
    // Navigation property
    public Sensor Sensor { get; set; } = null!;
}

public enum AlertSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2,
    Emergency = 3
}
