using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace AquaPP.Models;

public partial class WaterQualityReading : ObservableValidator
{
    [ObservableProperty] private Guid _id;
    
    [ObservableProperty] private string _sampleId = string.Empty;
    
    [ObservableProperty] private DateTime _timestamp = DateTime.Now;

    [ObservableProperty]
    [Required(ErrorMessage = "Location is required")]
    [StringLength(100, ErrorMessage = "Location must be less than 100 characters")] 
    private string _location = string.Empty;

    // Water Quality Parameters as doubles
    [ObservableProperty] 
    private double _pH;

    [ObservableProperty] 
    private double _conductivity;

    [ObservableProperty] 
    private double _turbidity;

    [ObservableProperty] 
    private double _chlorineResidual;

    [ObservableProperty] private string _notes = string.Empty;
    
    public static string GenerateSampleId()
    {
        var random = new Random();
        return random.Next(100000000, 999999999).ToString();
    }
}
