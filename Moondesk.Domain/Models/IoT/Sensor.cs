using System.ComponentModel.DataAnnotations;
using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models.IoT;

/// <summary>
/// Represents a sensor attached to an asset
/// </summary>
public class Sensor
{
    public long Id { get; set; }
    
    public long AssetId { get; set; }
    
    public required string OrganizationId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public SensorType Type { get; set; }
    
    public Parameter Parameter { get; set; } = Parameter.None;
    
    public string Unit { get; set; } = string.Empty; // e.g., "°C", "PSI", "Hz", "L/min"
    
    public double? ThresholdLow { get; set; }
    
    public double? ThresholdHigh { get; set; }
    
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public int SamplingIntervalMs { get; set; } = 1000; // Default 1 second
    
    public bool IsActive { get; set; } = true;
    
    public Protocol Protocol { get; set; }
    
    [MaxLength(100)]
    public string? Description { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; } = new();
    
    // Navigation properties
    public Asset Asset { get; set; } = null!;
    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<Command> Commands { get; set; } = new List<Command>();
}