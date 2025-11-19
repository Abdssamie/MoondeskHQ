using System.ComponentModel.DataAnnotations;
using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models.IoT;

/// <summary>
/// Represents an alert triggered when sensor readings exceed thresholds
/// </summary>
public class Alert
{
    public long Id { get; set; }
    
    public long SensorId { get; set; }
    
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
    
    public Protocol Protocol { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; } = new();
    
    // Navigation property
    public Sensor Sensor { get; set; } = null!;
}