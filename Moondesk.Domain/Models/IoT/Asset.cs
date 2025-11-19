using System.ComponentModel.DataAnnotations;
using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models.IoT;

/// <summary>
/// Represents an industrial asset (machine, equipment, facility) being monitored
/// </summary>
public class Asset
{
    public long Id { get; set; }
    
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
    
    public Dictionary<string, string>? Metadata { get; set; } = new();
    
    // Navigation property
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}