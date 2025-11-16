using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AquaPP.ViewModels.Pages;

public partial class SensorDetailViewModel : PageBase
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IDataStreamService _dataStreamService;
    private readonly ILogger<SensorDetailViewModel> _logger;

    [ObservableProperty]
    private Sensor? _sensor;

    [ObservableProperty]
    private double _currentValue;

    [ObservableProperty]
    private double _minValue;

    [ObservableProperty]
    private double _maxValue;

    [ObservableProperty]
    private double _avgValue;

    [ObservableProperty]
    private DateTime? _lastUpdate;

    [ObservableProperty]
    private double _currentValuePercent;

    [ObservableProperty]
    private ObservableCollection<TimeWindowOption> _timeWindows = new();

    [ObservableProperty]
    private TimeWindowOption? _selectedTimeWindow;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isStreaming;

    [ObservableProperty]
    private string _statusText = "Unknown";

    [ObservableProperty]
    private string _statusColor = "#9E9E9E";

    [ObservableProperty]
    private ObservableCollection<SensorReading> _sensorReadings = new();

    public SensorDetailViewModel(
        ISensorRepository sensorRepository,
        IDataStreamService dataStreamService,
        ILogger<SensorDetailViewModel> logger)
        : base("Sensor Detail", "fa-solid fa-gauge", -90)
    {
        _sensorRepository = sensorRepository;
        _dataStreamService = dataStreamService;
        _logger = logger;

        InitializeTimeWindows();
    }

    private void InitializeTimeWindows()
    {
        TimeWindows = new ObservableCollection<TimeWindowOption>
        {
            new TimeWindowOption { Label = "1 Minute", Minutes = 1 },
            new TimeWindowOption { Label = "5 Minutes", Minutes = 5 },
            new TimeWindowOption { Label = "15 Minutes", Minutes = 15 },
            new TimeWindowOption { Label = "1 Hour", Minutes = 60 },
            new TimeWindowOption { Label = "24 Hours", Minutes = 1440 }
        };

        SelectedTimeWindow = TimeWindows[2]; // Default to 15 minutes
    }

    public async Task LoadSensorAsync(int sensorId)
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading sensor detail for sensor ID: {SensorId}", sensorId);

            var sensor = await _sensorRepository.GetByIdAsync(sensorId);
            if (sensor == null)
            {
                _logger.LogWarning("Sensor not found: {SensorId}", sensorId);
                return;
            }

            Sensor = sensor;
            DisplayName = $"Sensor: {sensor.Name}";

            // Update status
            UpdateStatus();

            // Calculate statistics from readings
            await CalculateStatisticsAsync();

            // Populate sensor readings for the DataGrid
            SensorReadings.Clear();
            foreach (var reading in sensor.Readings.OrderByDescending(r => r.Timestamp))
            {
                SensorReadings.Add(reading);
            }

            _logger.LogInformation("Successfully loaded sensor: {SensorName}", sensor.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sensor detail");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateStatus()
    {
        if (Sensor == null) return;

        if (!Sensor.IsActive)
        {
            StatusText = "Offline";
            StatusColor = "#9E9E9E";
        }
        else if (Sensor.ThresholdHigh.HasValue && CurrentValue > Sensor.ThresholdHigh.Value)
        {
            StatusText = "Critical";
            StatusColor = "#F44336";
        }
        else if (Sensor.ThresholdLow.HasValue && CurrentValue < Sensor.ThresholdLow.Value)
        {
            StatusText = "Warning";
            StatusColor = "#FF9800";
        }
        else
        {
            StatusText = "Normal";
            StatusColor = "#4CAF50";
        }
    }

    private async Task CalculateStatisticsAsync()
    {
        if (Sensor == null) return;

        try
        {
            var readings = Sensor.Readings.OrderByDescending(r => r.Timestamp).ToList();

            if (readings.Any())
            {
                var latestReading = readings.First();
                CurrentValue = latestReading.Value;
                LastUpdate = latestReading.Timestamp.DateTime;

                // Calculate min, max, avg from recent readings
                var recentReadings = readings.Take(100).ToList();
                if (recentReadings.Any())
                {
                    MinValue = recentReadings.Min(r => r.Value);
                    MaxValue = recentReadings.Max(r => r.Value);
                    AvgValue = recentReadings.Average(r => r.Value);
                }

                // Calculate percentage for gauge (0-100%)
                if (Sensor.MinValue.HasValue && Sensor.MaxValue.HasValue)
                {
                    var range = Sensor.MaxValue.Value - Sensor.MinValue.Value;
                    if (range > 0)
                    {
                        CurrentValuePercent = ((CurrentValue - Sensor.MinValue.Value) / range) * 100;
                        CurrentValuePercent = Math.Max(0, Math.Min(100, CurrentValuePercent));
                    }
                }
                else
                {
                    // Default to 50% if no range defined
                    CurrentValuePercent = 50;
                }
            }
            else
            {
                CurrentValue = 0;
                MinValue = 0;
                MaxValue = 0;
                AvgValue = 0;
                CurrentValuePercent = 0;
                LastUpdate = null;
            }

            UpdateStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating sensor statistics");
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ChangeTimeWindowAsync()
    {
        if (SelectedTimeWindow == null) return;

        _logger.LogInformation("Changing time window to: {TimeWindow}", SelectedTimeWindow.Label);
        // Chart data will be updated based on selected time window
        // This will be implemented when chart integration is added
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportChartAsync()
    {
        _logger.LogInformation("Exporting chart data for sensor: {SensorId}", Sensor?.Id);
        // Export functionality will be implemented in future phase
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ZoomIn()
    {
        _logger.LogInformation("Zoom in chart");
        // Chart zoom functionality will be implemented when ScottPlot is integrated
    }

    [RelayCommand]
    private void ZoomOut()
    {
        _logger.LogInformation("Zoom out chart");
        // Chart zoom functionality will be implemented when ScottPlot is integrated
    }

    [RelayCommand]
    private void ResetZoom()
    {
        _logger.LogInformation("Reset chart zoom");
        // Chart reset functionality will be implemented when ScottPlot is integrated
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (Sensor != null)
        {
            await LoadSensorAsync(Sensor.Id);
        }
    }
}

/// <summary>
/// Model for time window selection options
/// </summary>
public class TimeWindowOption
{
    public string Label { get; set; } = string.Empty;
    public int Minutes { get; set; }
}
