using System;
using System.Collections.Generic;

namespace AquaPP.Services.Units;

public interface IUnitConversionService
{   
    public double Convert(double value, string fromUnit, string toUnit, Parameter parameter);
}
public class UnitConversionService : IUnitConversionService
{
    // Define base units for storage
    public const string BaseConductivityUnit = "µS/cm";
    public const string BaseTurbidityUnit = "NTU";
    public const string BaseChlorineUnit = "mg/L";

    // Conversion factors
    private readonly Dictionary<string, double> _conductivityConversions = new()
    {
        { "µS/cm", 1 },
        { "mS/cm", 0.001 }
    };

    private readonly Dictionary<string, double> _turbidityConversions = new()
    {
        { "NTU", 1 },
        { "FNU", 1 }
    };
    
    private readonly Dictionary<string, double> _chlorineConversions = new()
    {
        { "mg/L", 1 },
        { "ppm", 1 }
    };

    public double Convert(double value, string fromUnit, string toUnit, Parameter parameter)
    {
        var conversions = GetConversions(parameter);
        
        if (conversions.TryGetValue(fromUnit, out var fromFactor) && conversions.TryGetValue(toUnit, out var toFactor))
        {
            // Convert to base unit first, then to the target unit
            var valueInBase = value / fromFactor;
            return valueInBase * toFactor;
        }
        throw new ArgumentException($"Unsupported unit conversion from {fromUnit} to {toUnit} for {parameter}");
    }

    private Dictionary<string, double> GetConversions(Parameter parameter)
    {
        return parameter switch
        {
            Parameter.Conductivity => _conductivityConversions,
            Parameter.Turbidity => _turbidityConversions,
            Parameter.Chlorine => _chlorineConversions,
            _ => throw new ArgumentException($"Unsupported parameter: {parameter}")
        };
    }
}

public enum Parameter
{
    Conductivity,
    Turbidity,
    Chlorine
}
