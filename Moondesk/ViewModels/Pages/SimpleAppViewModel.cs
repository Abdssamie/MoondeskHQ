using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnitsNet;
using AquaPP.Services.WaterTreatment.PrimaryTreatment;
using Azure;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AquaPP.ViewModels.Pages;

public partial class SimpleAppViewModel : PageBase
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(CalculateSettlingVelocityCommand))]
    private bool _isBusy;

    // Properties bound to your input TextBoxes (example)
    [ObservableProperty] private double _particleDiameterValue;
    [ObservableProperty] private double _particleDensityValue = 2650;

    // Property bound to your outp_settlingVelocityResultut TextBlock
    [ObservableProperty] private string _settlingVelocityResult = "N/A";

    public SimpleAppViewModel() : base("Demo", "fa-solid fa-calculator", 3)
    {
        
    }
    
    // Use [RelayCommand] to automatically generate the command property.
    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private async Task CalculateSettlingVelocityAsync()
    {
        IsBusy = true; // Use the generated IsBusy property
        Debug.WriteLine("Command execution started. IsBusy set to true.");

        try
        {
            Debug.WriteLine("Fetching input values and calculating...");

            var result = await Task.Run(() => PrimarySedimentation.CalculateTerminalSettlingVelocity(
                Length.FromMicrometers(ParticleDiameterValue),
                Density.FromKilogramsPerCubicMeter(ParticleDensityValue),
                Density.FromKilogramsPerCubicMeter(997),
                Acceleration.FromMetersPerSecondSquared(9.81),
                DynamicViscosity.FromPascalSeconds(8.90e-4),
                Temperature.FromDegreesCelsius(25),
                Ratio.FromDecimalFractions(1),
                0.001
            ));
            
            SettlingVelocityResult = result.ToString("F4");
            Debug.WriteLine($"Settling velocity calculated: {result}");
            Debug.WriteLine($"Result updated in UI: {SettlingVelocityResult}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during calculation: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            Debug.WriteLine("Command execution completed. IsBusy set to false.");
        }
    }

    // A method to control when the command can be executed.
    private bool CanCalculate() => !IsBusy;

}

