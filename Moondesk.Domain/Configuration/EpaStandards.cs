using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Configuration;

public static class EpaStandards
{
    public static readonly Dictionary<Parameter, EpaThreshold> Thresholds = new()
    {
        { Parameter.pH, new EpaThreshold(6.5, 8.5, "pH units") },
        { Parameter.Turbidity, new EpaThreshold(0, 0.3, "NTU") },
        { Parameter.FreeChlorine, new EpaThreshold(0.2, 4.0, "mg/L") },
        { Parameter.TotalChlorine, new EpaThreshold(0.2, 4.0, "mg/L") },
        { Parameter.Fluoride, new EpaThreshold(0.7, 4.0, "mg/L") },
        { Parameter.TotalDissolvedSolids, new EpaThreshold(0, 500, "mg/L") }
    };
}

public record EpaThreshold(double Min, double Max, string Unit);
