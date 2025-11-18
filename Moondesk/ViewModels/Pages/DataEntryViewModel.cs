using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AquaPP.Models;
using AquaPP.Services.Csv; 
using AquaPP.Services.File;
using AquaPP.Services.Units;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using ILogger = Serilog.ILogger;

namespace AquaPP.ViewModels.Pages;

public partial class DataEntryViewModel : PageBase
{
    private readonly ILogger _logger;
    private readonly ISukiToastManager _toastService;
    private readonly WaterQualityCsvService _csvService;
    private readonly IFilePickerService _filePickerService;
    private readonly IUnitConversionService _unitConversionService;
    
    private string _currentConductivityUnit;
    private string _currentTurbidityUnit;
    private string _currentChlorineUnit;

    [ObservableProperty] private string _selectedConductivityUnit = UnitConversionService.BaseConductivityUnit;

    [ObservableProperty] private string _selectedTurbidityUnit = UnitConversionService.BaseTurbidityUnit;

    [ObservableProperty] private string _selectedChlorineUnit = UnitConversionService.BaseChlorineUnit;

    public List<string> ConductivityUnits { get; } = ["µS/cm", "mS/cm"];
    public List<string> TurbidityUnits { get; } = ["NTU", "FNU"];
    public List<string> ChlorineUnits { get; } = ["mg/L", "ppm"];

    [ObservableProperty] private DateTime _currentDate = DateTime.Now;

    [ObservableProperty] private bool _showNoDataMessage;


    private ObservableCollection<WaterQualityReading> _selectedReadings = [];

    public ObservableCollection<WaterQualityReading> SelectedReadings
    {
        get => _selectedReadings;
        set => SetProperty(ref _selectedReadings, value);
    }

    public ICommand SelectedDateChangedCommand { get; set; }
    public ICommand AddBlankReadingRowCommand { get; }
    public ICommand UpdateConductivityUnitCommand { get; }
    public ICommand UpdateTurbidityUnitCommand { get; }
    public ICommand UpdateChlorineUnitCommand { get; }

    public event Action? RequestScrollToBottom;


    private readonly ObservableCollection<WaterQualityReading> _readings = [];

    public ObservableCollection<WaterQualityReading> Readings
    {
        get => _readings;
        set
        {
            if (_readings == value) return;

            _readings.Clear();
            foreach (var item in value)
            {
                _readings.Add(item);
            }
        }
    }

    public DataEntryViewModel(
        ISukiToastManager toastManager,
        ILogger logger,
        WaterQualityCsvService csvService, // Injected CSV service
        IFilePickerService filePickerService, // Injected FilePicker service
        IUnitConversionService unitConversionService)
        : base("Data Entry", "fa-solid fa-database", 3)
    {
        _logger = logger;
        _logger.Information("DataEntryViewModel initialized.");

        _toastService = toastManager;
        _csvService = csvService; // Assigned CSV service
        _filePickerService = filePickerService; // Assigned FilePicker service
        _unitConversionService = unitConversionService;

        _currentConductivityUnit = SelectedConductivityUnit;
        _currentTurbidityUnit = SelectedTurbidityUnit;
        _currentChlorineUnit = SelectedChlorineUnit;

        AddBlankReadingRowCommand = new RelayCommand(AddBlankReadingRow);
        SelectedDateChangedCommand = new RelayCommand<DateTime?>(SelectedDateChanged);
        UpdateChlorineUnitCommand = new RelayCommand<string?>(UpdateChlorineUnit);
        UpdateConductivityUnitCommand = new RelayCommand<string?>(UpdateConductivityUnit);
        UpdateTurbidityUnitCommand = new RelayCommand<string?>(UpdateTurbidityUnit);

        SelectedReadings.CollectionChanged += (_, _) => DeleteSelectedReadingsCommand.NotifyCanExecuteChanged();

        ShowNoDataMessage = true;

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                await LoadCurrentDateReadingsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load current date readings.");
            }
        });
    }


    private void UpdateChlorineUnit(string? unit)
    {
        _logger.Information("Updating chlorine unit to {Unit}", unit);
        if (unit != null && unit != _currentChlorineUnit)
        {
            ConvertReadings(Parameter.Chlorine, _currentChlorineUnit, unit);
            _currentChlorineUnit = unit;
        }
    }

    private void UpdateConductivityUnit(string? unit)
    {
        _logger.Information("Updating conductivity unit to {Unit}", unit);
        if (unit != null && unit != _currentConductivityUnit)
        {
            ConvertReadings(Parameter.Conductivity, _currentConductivityUnit, unit);
            _currentConductivityUnit = unit;
        }
    }

    private void UpdateTurbidityUnit(string? unit)
    {
        _logger.Information("Updating turbidity unit to {Unit}", unit);
        if (unit != null && unit != _currentTurbidityUnit)
        {
            ConvertReadings(Parameter.Turbidity, _currentTurbidityUnit, unit);
            _currentTurbidityUnit = unit;
        }
    }

    /// <summary>
    /// Design time constructor
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DataEntryViewModel() : base("Data Entry", "fa-solid fa-database", 3)
    {
    }

    private void ConvertReadings(Parameter parameter, string fromUnit, string toUnit)
    {
        foreach (var reading in Readings)
        {
            switch (parameter)
            {
                case Parameter.Conductivity:
                    reading.Conductivity =
                        _unitConversionService.Convert(reading.Conductivity, fromUnit, toUnit, parameter);
                    break;
                case Parameter.Turbidity:
                    reading.Turbidity = _unitConversionService.Convert(reading.Turbidity, fromUnit, toUnit, parameter);
                    break;
                case Parameter.Chlorine:
                    reading.ChlorineResidual =
                        _unitConversionService.Convert(reading.ChlorineResidual, fromUnit, toUnit, parameter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }
    }

    private void SelectedDateChanged(DateTime? date)
    {
        if (date.HasValue && CurrentDate != date.Value)
        {
            CurrentDate = date.Value;
        }

        // Clear the displayed reading and load the reading for the current selected date
        LoadCurrentDateReadingsCommand.Execute(null);
    }


    [RelayCommand]
    private Task LoadAllReadingsAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task LoadCurrentDateReadingsAsync()
    {
        return Task.CompletedTask;
    }


    [RelayCommand]
    private Task SaveReadings()
    {
        return Task.CompletedTask;
    }


    [RelayCommand(CanExecute = nameof(CanDelete))]
    private Task DeleteSelectedReadings()
    {
        return Task.CompletedTask;
    }

    private bool CanDelete()
    {
        return SelectedReadings.Any();
    }


    private void AddBlankReadingRow()
    {
        // Create a new, empty model instance and add it to the collection.
        // The DataGrid will automatically update.
        ShowNoDataMessage = false;
        Readings.Add(new WaterQualityReading
        {
            SampleId = WaterQualityReading.GenerateSampleId()
        });
        RequestScrollToBottom?.Invoke();
    }

    [RelayCommand]
    private async Task ExportReadings(Control control)
    {
        if (Readings.Count == 0)
        {
            _toastService.CreateToast()
                .WithTitle("Export Failed")
                .WithContent("No readings to export.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
            return;
        }

        var filePath = await _filePickerService.PickSaveFileAsync(control, "Export Water Quality Data", "csv", "*.csv");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.Information("Export operation cancelled by user.");
            return;
        }

        var success = await _csvService.ExportWaterQualityData(Readings, filePath, SelectedConductivityUnit,
            SelectedTurbidityUnit, SelectedChlorineUnit);

        if (success)
        {
            _toastService.CreateToast()
                .WithTitle("Export Successful")
                .WithContent($"Readings exported to {filePath}")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
        }
        else
        {
            _toastService.CreateToast()
                .WithTitle("Export Failed")
                .WithContent("Failed to export readings. Check logs for details.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
        }
    }
}
