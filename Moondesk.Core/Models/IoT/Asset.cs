using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AquaPP.Core.Models.IoT;

/// <summary>
/// Represents an industrial asset (machine, equipment, facility) being monitored
/// </summary>
public class Asset
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty; // e.g., "Pump", "Tank", "Valve", "Compressor"
    
    public string Location { get; set; } = string.Empty;
    
    public AssetStatus Status { get; set; } = AssetStatus.Unknown;
    
    public DateTimeOffset? LastSeen { get; set; }
    
    [MaxLength(100)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Manufacturer { get; set; }
    
    [MaxLength(100)]
    public string? ModelNumber { get; set; }
    
    public DateTimeOffset? InstallationDate { get; set; }
    
    // Navigation property
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}

public enum AssetStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Warning = 3,
    Critical = 4,
    Maintenance = 5
}
