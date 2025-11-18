using AquaPP.Models.Parameters;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AquaPP.Models.Templates;

public partial class MineralTemplate : BaseTemplate
{
    [ObservableProperty] private Parameter _calcium = new("Calcium", 0, 1000, 0, 100, "mg/L");
    [ObservableProperty] private Parameter _alkalinity = new("Alkalinity", 0, 1000, 0, 100, "mg/L");
    [ObservableProperty] private Parameter _magnesium = new("Magnesium", 0, 1000, 0, 100, "mg/L");
    [ObservableProperty] private Parameter _sulfate = new("Sulfate", 0, 1000, 0, 100, "mg/L");
    [ObservableProperty] private Parameter _nitrate = new("Nitrate", 0, 1000, 0, 100, "mg/L");
    [ObservableProperty] private Parameter _phosphate = new("Phosphate", 0, 1000, 0, 100, "mg/L");
    [ObservableProperty] private Parameter _iron = new("Iron", 0, 1000, 0, 100, "mg/L");
}
