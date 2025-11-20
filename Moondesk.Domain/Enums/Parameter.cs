namespace Moondesk.Domain.Enums;

/// <summary>
/// Water quality parameters for specialized monitoring and compliance
/// </summary>
public enum Parameter
{
    None = 0,
    
    // Chemical Parameters
    pH = 1,
    FreeChlorine = 2,
    TotalChlorine = 3,
    Fluoride = 4,
    DissolvedOxygen = 5,
    
    // Physical Parameters
    Turbidity = 10,
    Temperature = 11,
    Conductivity = 12,
    TotalDissolvedSolids = 13,
    
    // Hydraulic Parameters
    FlowRate = 20,
    Pressure = 21,
    Level = 22
}
