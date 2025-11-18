using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AquaPP.Models.Parameters;

public partial class Parameter : ObservableValidator
{
    public Parameter(
        string name,
        double physicalMin,
        double physicalMax,
        double acceptedMin,
        double acceptedMax,
        string unit = "",
        string? description = null)
    {
        Name = name;
        PhysicalMin = physicalMin;
        PhysicalMax = physicalMax;
        AcceptedMin = acceptedMin;
        AcceptedMax = acceptedMax;
        Unit = unit;
        Description = description ?? name;
    }

    [ObservableProperty]
    private double _value;

    public string Name { get; }
    public string Unit { get; }
    public string Description { get; }

    public double PhysicalMin { get; }
    public double PhysicalMax { get; }
    public double AcceptedMin { get; }
    public double AcceptedMax { get; }

    /// <summary>
    /// True if value is within physically possible limits.
    /// </summary>
    public bool IsPhysicallyValid => Value >= PhysicalMin && Value <= PhysicalMax;

    /// <summary>
    /// True if value is within accepted range (safe/desired).
    /// </summary>
    public bool IsWithinAcceptedRange => Value >= AcceptedMin && Value <= AcceptedMax;

    public string? StatusMessage
    {
        get
        {
            if (!IsPhysicallyValid)
                return $"ERROR: {Name} out of physical range ({PhysicalMin}-{PhysicalMax} {Unit})";

            if (!IsWithinAcceptedRange)
                return $"WARNING: {Name} outside accepted range ({AcceptedMin}-{AcceptedMax} {Unit})";

            return null; // OK
        }
    }

    [RelayCommand]
    public void ValidateParameter()
    {
        ValidateAllProperties();
        // Could trigger UI updates or logging based on StatusMessage
    }
}