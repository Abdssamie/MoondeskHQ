using AquaPP.Models.Parameters;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AquaPP.Models.Templates;

public partial class MicrobiologyTemplate : BaseTemplate
{
    [ObservableProperty] private Parameter _totalColiforms = new("Total Coliforms", 0, 1000, 0, 100, "MPN/100mL");
    [ObservableProperty] private Parameter _ecoli = new("E. coli", 0, 1000, 0, 10, "MPN/100mL");
    [ObservableProperty] private Parameter _enterococci = new("Enterococci", 0, 1000, 0, 10, "MPN/100mL");
    [ObservableProperty] private Parameter _heterotrophicPlateCount = new("Heterotrophic Plate Count", 0, 1000, 0, 100, "CFU/mL");
}
