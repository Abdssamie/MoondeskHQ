using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AquaPP.ViewModels.Pages;

public partial class ThresholdConfigurationViewModel : PageBase
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ILogger<ThresholdConfigurationViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ThresholdEditModel> _sensors = new();

    [ObservableProperty]
    private ObservableCollection<ThresholdEditModel> _filteredSensors = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showBulkUpdateDialog;

    [ObservableProperty]
    private SensorType? _selectedBulkSensorType;

    [ObservableProperty]
    private double? _bulkMinThreshold;

    [ObservableProperty]
    private double? _bulkMaxThreshold;

    [ObservableProperty]
    private ThresholdEditModel? _selectedSensor;

    [ObservableProperty]
    private int _previewAffectedReadings;

    [ObservableProperty]
    private int _previewNewAlerts;

    public ThresholdConfigurationViewModel(
        ISensorRepository sensorRepository,
        ILogger<ThresholdConfigurationViewModel> logger)
        : base("Threshold Configuration", "fa-solid fa-sliders", -85)
    {
        _sensorRepository = sensorRepository;
        _logger = logger;
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterSensors();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading threshold configuration data");

            var sensors = await _sensorRepository.GetAllAsync();
            var sensorsList = sensors.ToList();

            var thresholdModels = sensorsList.Select(s => new ThresholdEditModel
            {
                SensorId = s.Id,
                SensorName = s.Name,
                Type = s.Type,
                Unit = s.Unit,
                AssetName = s.Asset?.Name ?? "Unknown",
                CurrentMinThreshold = s.ThresholdLow,
                CurrentMaxThreshold = s.ThresholdHigh,
                NewMinThreshold = s.ThresholdLow,
                NewMaxThreshold = s.ThresholdHigh,
                MinValue = s.MinValue,
                MaxValue = s.MaxValue
            }).ToList();

            Sensors = new ObservableCollection<ThresholdEditModel>(thresholdModels);
            FilteredSensors = new ObservableCollection<ThresholdEditModel>(thresholdModels);

            _logger.LogInformation("Successfully loaded {Count} sensors for threshold configuration", Sensors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading threshold configuration data");
            ErrorMessage = "Failed to load sensor data. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterSensors()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredSensors = new ObservableCollection<ThresholdEditModel>(Sensors);
        }
        else
        {
            var searchLower = SearchText.ToLower();
            var filtered = Sensors.Where(s =>
                s.SensorName.ToLower().Contains(searchLower) ||
                s.AssetName.ToLower().Contains(searchLower) ||
                s.Type.ToString().ToLower().Contains(searchLower)
            ).ToList();

            FilteredSensors = new ObservableCollection<ThresholdEditModel>(filtered);
        }
    }

    [RelayCommand]
    private void UpdateThreshold(ThresholdEditModel model)
    {
        ValidateThreshold(model);
        CheckForUnsavedChanges();
    }

    private void ValidateThreshold(ThresholdEditModel model)
    {
        model.IsValid = true;
        model.ValidationMessage = string.Empty;

        // Check if min < max
        if (model.NewMinThreshold.HasValue && model.NewMaxThreshold.HasValue)
        {
            if (model.NewMinThreshold.Value >= model.NewMaxThreshold.Value)
            {
                model.IsValid = false;
                model.ValidationMessage = "Minimum threshold must be less than maximum threshold";
                return;
            }
        }

        // Check if within sensor range
        if (model.MinValue.HasValue && model.NewMinThreshold.HasValue)
        {
            if (model.NewMinThreshold.Value < model.MinValue.Value)
            {
                model.IsValid = false;
                model.ValidationMessage = $"Minimum threshold cannot be below sensor minimum ({model.MinValue.Value})";
                return;
            }
        }

        if (model.MaxValue.HasValue && model.NewMaxThreshold.HasValue)
        {
            if (model.NewMaxThreshold.Value > model.MaxValue.Value)
            {
                model.IsValid = false;
                model.ValidationMessage = $"Maximum threshold cannot exceed sensor maximum ({model.MaxValue.Value})";
                return;
            }
        }
    }

    private void CheckForUnsavedChanges()
    {
        HasUnsavedChanges = Sensors.Any(s => s.HasChanges);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // Validate all changes
            var invalidSensors = Sensors.Where(s => s.HasChanges && !s.IsValid).ToList();
            if (invalidSensors.Any())
            {
                ErrorMessage = $"Cannot save: {invalidSensors.Count} sensor(s) have invalid threshold values";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Saving threshold changes");

            var changedSensors = Sensors.Where(s => s.HasChanges).ToList();
            foreach (var model in changedSensors)
            {
                var sensor = await _sensorRepository.GetByIdAsync(model.SensorId);
                if (sensor != null)
                {
                    sensor.ThresholdLow = model.NewMinThreshold;
                    sensor.ThresholdHigh = model.NewMaxThreshold;
                    await _sensorRepository.UpdateAsync(sensor);

                    // Update current values
                    model.CurrentMinThreshold = model.NewMinThreshold;
                    model.CurrentMaxThreshold = model.NewMaxThreshold;
                }
            }

            HasUnsavedChanges = false;
            _logger.LogInformation("Successfully saved threshold changes for {Count} sensors", changedSensors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving threshold changes");
            ErrorMessage = "Failed to save changes. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("Canceling threshold changes");

        // Reset all changes
        foreach (var sensor in Sensors)
        {
            sensor.NewMinThreshold = sensor.CurrentMinThreshold;
            sensor.NewMaxThreshold = sensor.CurrentMaxThreshold;
            sensor.IsValid = true;
            sensor.ValidationMessage = string.Empty;
        }

        HasUnsavedChanges = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void Reset(ThresholdEditModel model)
    {
        model.NewMinThreshold = model.CurrentMinThreshold;
        model.NewMaxThreshold = model.CurrentMaxThreshold;
        model.IsValid = true;
        model.ValidationMessage = string.Empty;
        CheckForUnsavedChanges();
    }

    [RelayCommand]
    private void ShowBulkUpdate()
    {
        ShowBulkUpdateDialog = true;
        SelectedBulkSensorType = null;
        BulkMinThreshold = null;
        BulkMaxThreshold = null;
    }

    [RelayCommand]
    private void ApplyBulkUpdate()
    {
        if (!SelectedBulkSensorType.HasValue)
        {
            ErrorMessage = "Please select a sensor type for bulk update";
            return;
        }

        if (!BulkMinThreshold.HasValue && !BulkMaxThreshold.HasValue)
        {
            ErrorMessage = "Please enter at least one threshold value";
            return;
        }

        _logger.LogInformation("Applying bulk update to {Type} sensors", SelectedBulkSensorType);

        var sensorsToUpdate = Sensors.Where(s => s.Type == SelectedBulkSensorType.Value).ToList();
        foreach (var sensor in sensorsToUpdate)
        {
            if (BulkMinThreshold.HasValue)
            {
                sensor.NewMinThreshold = BulkMinThreshold.Value;
            }
            if (BulkMaxThreshold.HasValue)
            {
                sensor.NewMaxThreshold = BulkMaxThreshold.Value;
            }

            ValidateThreshold(sensor);
        }

        CheckForUnsavedChanges();
        ShowBulkUpdateDialog = false;
    }

    [RelayCommand]
    private void CancelBulkUpdate()
    {
        ShowBulkUpdateDialog = false;
    }

    [RelayCommand]
    private async Task PreviewImpactAsync(ThresholdEditModel model)
    {
        if (model == null) return;

        try
        {
            _logger.LogInformation("Previewing threshold impact for sensor {SensorId}", model.SensorId);

            var sensor = await _sensorRepository.GetByIdAsync(model.SensorId);
            if (sensor == null) return;

            // Calculate how many readings would be affected by the new thresholds
            var readings = sensor.Readings.ToList();
            PreviewAffectedReadings = readings.Count;

            // Calculate how many new alerts would be triggered
            PreviewNewAlerts = 0;
            foreach (var reading in readings)
            {
                bool currentlyInRange = true;
                if (model.CurrentMinThreshold.HasValue && reading.Value < model.CurrentMinThreshold.Value)
                    currentlyInRange = false;
                if (model.CurrentMaxThreshold.HasValue && reading.Value > model.CurrentMaxThreshold.Value)
                    currentlyInRange = false;

                bool newlyInRange = true;
                if (model.NewMinThreshold.HasValue && reading.Value < model.NewMinThreshold.Value)
                    newlyInRange = false;
                if (model.NewMaxThreshold.HasValue && reading.Value > model.NewMaxThreshold.Value)
                    newlyInRange = false;

                // Count readings that would trigger new alerts
                if (currentlyInRange && !newlyInRange)
                {
                    PreviewNewAlerts++;
                }
            }

            SelectedSensor = model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing threshold impact");
        }
    }
}

/// <summary>
/// Model for threshold editing with validation
/// </summary>
public partial class ThresholdEditModel : ObservableObject
{
    public int SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;

    [ObservableProperty]
    private double? _currentMinThreshold;

    [ObservableProperty]
    private double? _currentMaxThreshold;

    [ObservableProperty]
    private double? _newMinThreshold;

    [ObservableProperty]
    private double? _newMaxThreshold;

    [ObservableProperty]
    private double? _minValue;

    [ObservableProperty]
    private double? _maxValue;

    [ObservableProperty]
    private bool _isValid = true;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public bool HasChanges =>
        NewMinThreshold != CurrentMinThreshold ||
        NewMaxThreshold != CurrentMaxThreshold;

    partial void OnNewMinThresholdChanged(double? value)
    {
        OnPropertyChanged(nameof(HasChanges));
    }

    partial void OnNewMaxThresholdChanged(double? value)
    {
        OnPropertyChanged(nameof(HasChanges));
    }
}
