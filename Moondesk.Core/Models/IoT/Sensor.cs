using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AquaPP.Core.Models.IoT;

/// <summary>
/// Represents a sensor attached to an asset
/// </summary>
public class Sensor
{
    public int Id { get; set; }
    
    public int AssetId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public SensorType Type { get; set; }
    
    public string Unit { get; set; } = string.Empty; // e.g., "°C", "PSI", "Hz", "L/min"
    
    public double? ThresholdLow { get; set; }
    
    public double? ThresholdHigh { get; set; }
    
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public int SamplingIntervalMs { get; set; } = 1000; // Default 1 second
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(100)]
    public string? Description { get; set; }
    
    // Navigation properties
    public Asset Asset { get; set; } = null!;
    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}

public enum SensorType
{
    Temperature = 1,
    Pressure = 2,
    Vibration = 3,
    FlowRate = 4,
    Level = 5,
    Humidity = 6,
    Power = 7,
    Speed = 8,
    pH = 9,
    Conductivity = 10,
    Custom = 99
}
