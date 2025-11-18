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

public partial class MonitoringDashboardViewModel : PageBase
{
    private readonly IAssetRepository _assetRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IDataStreamService _dataStreamService;
    private readonly ILogger<MonitoringDashboardViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<AssetCardModel> _assets = new();

    [ObservableProperty]
    private int _totalAssets;

    [ObservableProperty]
    private int _onlineSensors;

    [ObservableProperty]
    private int _activeAlerts;

    [ObservableProperty]
    private string _systemUptime = "0h 0m";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isStreaming;

    private readonly DateTime _startTime;

    public MonitoringDashboardViewModel(
        IAssetRepository assetRepository,
        ISensorRepository sensorRepository,
        IDataStreamService dataStreamService,
        ILogger<MonitoringDashboardViewModel> logger)
        : base("Monitoring Dashboard", "fa-solid fa-gauge-high", -95)
    {
        _assetRepository = assetRepository;
        _sensorRepository = sensorRepository;
        _dataStreamService = dataStreamService;
        _logger = logger;
        _startTime = DateTime.Now;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading monitoring dashboard data");

            // Load assets with sensors
            var assets = await _assetRepository.GetAllAsync();
            var assetsList = assets.ToList();

            // Load all sensors
            var sensors = await _sensorRepository.GetAllAsync();
            var sensorsList = sensors.ToList();

            // Update KPIs
            TotalAssets = assetsList.Count;
            OnlineSensors = sensorsList.Count(s => s.IsActive);
            
            // Count active alerts (sensors exceeding thresholds)
            ActiveAlerts = 0; // Will be calculated from actual readings in future

            // Calculate system uptime
            var uptime = DateTime.Now - _startTime;
            SystemUptime = $"{(int)uptime.TotalHours}h {uptime.Minutes}m";

            // Create asset card models
            var assetCards = new ObservableCollection<AssetCardModel>();
            foreach (var asset in assetsList)
            {
                var assetWithSensors = await _assetRepository.GetByIdWithSensorsAsync(asset.Id);
                if (assetWithSensors != null)
                {
                    var assetSensors = assetWithSensors.Sensors.ToList();
                    var keyMetrics = new System.Collections.Generic.Dictionary<string, double>();

                    // Get key metrics from sensors (up to 3)
                    foreach (var sensor in assetSensors.Take(3))
                    {
                        var latestReading = sensor.Readings.OrderByDescending(r => r.Timestamp).FirstOrDefault();
                        if (latestReading != null)
                        {
                            keyMetrics[$"{sensor.Name}"] = latestReading.Value;
                        }
                    }

                    assetCards.Add(new AssetCardModel
                    {
                        AssetId = asset.Id,
                        Name = asset.Name,
                        Status = asset.Status,
                        SensorCount = assetSensors.Count,
                        ActiveAlerts = 0, // Will be calculated from actual alerts
                        KeyMetrics = keyMetrics,
                        Location = asset.Location,
                        Type = asset.Type
                    });
                }
            }

            Assets = assetCards;

            _logger.LogInformation("Successfully loaded {Count} assets", Assets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading monitoring dashboard data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private void NavigateToAsset(int assetId)
    {
        _logger.LogInformation("Navigating to asset detail: {AssetId}", assetId);
        // Navigation will be implemented when asset detail view is created
    }

    public string GetStatusColor(AssetStatus status)
    {
        return status switch
        {
            AssetStatus.Online => "#4CAF50",      // Green
            AssetStatus.Offline => "#9E9E9E",     // Gray
            AssetStatus.Warning => "#FF9800",     // Orange
            AssetStatus.Critical => "#F44336",    // Red
            AssetStatus.Maintenance => "#2196F3", // Blue
            _ => "#9E9E9E"                        // Gray
        };
    }

    public string GetStatusIcon(AssetStatus status)
    {
        return status switch
        {
            AssetStatus.Online => "fa-solid fa-circle-check",
            AssetStatus.Offline => "fa-solid fa-circle-xmark",
            AssetStatus.Warning => "fa-solid fa-triangle-exclamation",
            AssetStatus.Critical => "fa-solid fa-circle-exclamation",
            AssetStatus.Maintenance => "fa-solid fa-wrench",
            _ => "fa-solid fa-circle-question"
        };
    }
}

/// <summary>
/// Model for asset cards displayed on the monitoring dashboard
/// </summary>
public class AssetCardModel
{
    public int AssetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public AssetStatus Status { get; set; }
    public int SensorCount { get; set; }
    public int ActiveAlerts { get; set; }
    public System.Collections.Generic.Dictionary<string, double> KeyMetrics { get; set; } = new();
}
