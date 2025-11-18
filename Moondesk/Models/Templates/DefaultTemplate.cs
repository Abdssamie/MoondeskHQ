using AquaPP.Models.Parameters;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AquaPP.Models.Templates;

public partial class DefaultTemplate : BaseTemplate
{
    [ObservableProperty] private Parameter _pH = new("pH", 0, 14, 6.5, 8.5);
    [ObservableProperty] private Parameter _conductivity = new("Conductivity", 0, 10000, 0, 1000, "µS/cm");
    [ObservableProperty] private Parameter _turbidity = new("Turbidity", 0, 100, 0, 10, "NTU");
    [ObservableProperty] private Parameter _chlorineResidual = new("Chlorine Residual", 0, 5, 0.5, 2, "mg/L");
}