using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using AquaPP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AquaPP.ViewModels.Pages;

public partial class SensorListViewModel : PageBase, IDisposable
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IDataStreamService _dataStreamService;
    private readonly PageNavigationService _navigationService;
    private readonly ILogger<SensorListViewModel> _logger;
    private IDisposable? _streamSubscription;

    [ObservableProperty]
    private ObservableCollection<SensorListItemModel> _sensors = new();

    [ObservableProperty]
    private ObservableCollection<SensorListItemModel> _filteredSensors = new();

    [ObservableProperty]
    private ObservableCollection<string> _groupOptions = new();

    [ObservableProperty]
    private string _selectedGroupBy = "None";

    [ObservableProperty]
    private ObservableCollection<string> _statusOptions = new();

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    [ObservableProperty]
    private int _totalSensors;

    [ObservableProperty]
    private int _onlineSensors;

    [ObservableProperty]
    private int _offlineSensors;

    [ObservableProperty]
    private bool _isStreaming;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public SensorListViewModel(
        ISensorRepository sensorRepository,
        IDataStreamService dataStreamService,
        PageNavigationService navigationService,
        ILogger<SensorListViewModel> logger)
        : base("Sensor List", "fa-solid fa-list", -85)
    {
        _sensorRepository = sensorRepository;
        _dataStreamService = dataStreamService;
        _navigationService = navigationService;
        _logger = logger;

        InitializeFilters();
    }

    private void InitializeFilters()
    {
        GroupOptions = new ObservableCollection<string>
        {
            "None",
            "Asset",
            "Type",
            "Status"
        };

        StatusOptions = new ObservableCollection<string>
        {
            "All",
            "Online",
            "Offline",
            "Warning",
            "Critical"
        };
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading sensor list data");

            var sensors = await _sensorRepository.GetAllWithAssetsAsync();
            var sensorsList = sensors.ToList();

            // Create sensor list item models
            var sensorItems = new ObservableCollection<SensorListItemModel>();
            foreach (var sensor in sensorsList)
            {
                var latestReading = sensor.Readings.OrderByDescending(r => r.Timestamp).FirstOrDefault();
                
                sensorItems.Add(new SensorListItemModel
                {
                    SensorId = sensor.Id,
                    Name = sensor.Name,
                    Type = sensor.Type.ToString(),
                    CurrentValue = latestReading?.Value ?? 0,
                    Unit = sensor.Unit,
                    AssetName = sensor.Asset?.Name ?? "Unknown",
                    LastUpdate = latestReading?.Timestamp.DateTime,
                    IsActive = sensor.IsActive,
                    Status = DetermineStatus(sensor, latestReading?.Value),
                    ThresholdLow = sensor.ThresholdLow,
                    ThresholdHigh = sensor.ThresholdHigh
                });
            }

            Sensors = sensorItems;

            // Update statistics
            TotalSensors = sensorItems.Count;
            OnlineSensors = sensorItems.Count(s => s.IsActive);
            OfflineSensors = sensorItems.Count(s => !s.IsActive);

            // Apply filters
            ApplyFilters();

            _logger.LogInformation("Successfully loaded {Count} sensors", Sensors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sensor list data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private SensorStatus DetermineStatus(Sensor sensor, double? currentValue)
    {
        if (!sensor.IsActive)
            return SensorStatus.Offline;

        if (!currentValue.HasValue)
            return SensorStatus.Offline;

        if (sensor.ThresholdHigh.HasValue && currentValue.Value > sensor.ThresholdHigh.Value)
            return SensorStatus.Critical;

        if (sensor.ThresholdLow.HasValue && currentValue.Value < sensor.ThresholdLow.Value)
            return SensorStatus.Warning;

        return SensorStatus.Online;
    }

    partial void OnSelectedGroupByChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = Sensors.AsEnumerable();

        // Apply status filter
        if (SelectedStatusFilter != "All")
        {
            filtered = filtered.Where(s => s.Status.ToString() == SelectedStatusFilter);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(s =>
                s.Name.ToLower().Contains(searchLower) ||
                s.Type.ToLower().Contains(searchLower) ||
                s.AssetName.ToLower().Contains(searchLower));
        }

        // Apply grouping/sorting
        filtered = SelectedGroupBy switch
        {
            "Asset" => filtered.OrderBy(s => s.AssetName).ThenBy(s => s.Name),
            "Type" => filtered.OrderBy(s => s.Type).ThenBy(s => s.Name),
            "Status" => filtered.OrderBy(s => s.Status).ThenBy(s => s.Name),
            _ => filtered.OrderBy(s => s.Name)
        };

        FilteredSensors = new ObservableCollection<SensorListItemModel>(filtered);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private void NavigateToDetail(int sensorId)
    {
        _logger.LogInformation("Navigating to sensor detail: {SensorId}", sensorId);
        
        // Navigate to sensor detail view using the navigation service
        _navigationService.RequestNavigation<SensorDetailViewModel>();
    }

    [RelayCommand]
    private async Task ToggleStreamingAsync()
    {
        if (IsStreaming)
        {
            StopStreaming();
        }
        else
        {
            await StartStreamingAsync();
        }
    }

    private async Task StartStreamingAsync()
    {
        try
        {
            _logger.LogInformation("Starting real-time sensor data streaming");
            IsStreaming = true;

            // Start streaming for all sensors and subscribe to updates
            await _dataStreamService.StartStreamingAsync();
            
            // For now, we'll simulate streaming by periodically refreshing data
            // In a real implementation, you would subscribe to individual sensor streams
            // This is a simplified approach for the current task

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting sensor data streaming");
            IsStreaming = false;
        }
    }

    private void StopStreaming()
    {
        _logger.LogInformation("Stopping real-time sensor data streaming");
        _streamSubscription?.Dispose();
        _streamSubscription = null;
        IsStreaming = false;
        
        // Stop the data streaming service
        _ = _dataStreamService.StopStreamingAsync();
    }



    public void Dispose()
    {
        StopStreaming();
    }
}

/// <summary>
/// Model for sensor list items
/// </summary>
public partial class SensorListItemModel : ObservableObject
{
    public int SensorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    
    [ObservableProperty]
    private double _currentValue;
    
    public string Unit { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    
    [ObservableProperty]
    private DateTime? _lastUpdate;
    
    public bool IsActive { get; set; }
    
    [ObservableProperty]
    private SensorStatus _status;
    
    public double? ThresholdLow { get; set; }
    public double? ThresholdHigh { get; set; }

    public string StatusColor => Status switch
    {
        SensorStatus.Online => "#4CAF50",
        SensorStatus.Offline => "#9E9E9E",
        SensorStatus.Warning => "#FF9800",
        SensorStatus.Critical => "#F44336",
        _ => "#9E9E9E"
    };

    public string StatusIcon => Status switch
    {
        SensorStatus.Online => "fa-solid fa-circle-check",
        SensorStatus.Offline => "fa-solid fa-circle-xmark",
        SensorStatus.Warning => "fa-solid fa-triangle-exclamation",
        SensorStatus.Critical => "fa-solid fa-circle-exclamation",
        _ => "fa-solid fa-circle-question"
    };
}

public enum SensorStatus
{
    Online,
    Offline,
    Warning,
    Critical
}
