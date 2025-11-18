using System;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AquaPP.Models.Templates;

public partial class BaseTemplate : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Location is required")]
    [StringLength(100, ErrorMessage = "Location must be less than 100 characters")]
    private string _location = string.Empty;

    [ObservableProperty] private string _sampleId = GenerateSampleId();

    [ObservableProperty]
    [Required(ErrorMessage = "Sample date is required")]
    [DataType(DataType.Date, ErrorMessage = "Sample date must be a valid date")]
    private DateTime _sampleDate;

    public static string GenerateSampleId()
    {
        var random = new Random();
        return random.Next(100000000, 999999999).ToString();
    }
}