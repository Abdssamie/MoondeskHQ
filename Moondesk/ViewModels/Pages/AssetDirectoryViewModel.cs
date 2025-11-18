using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AquaPP.ViewModels.Pages;

public partial class AssetDirectoryViewModel : PageBase
{
    private readonly IAssetRepository _assetRepository;
    private readonly IDataStreamService _dataStreamService;
    private readonly ILogger<AssetDirectoryViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Asset> _assets = new();

    [ObservableProperty]
    private Asset? _selectedAsset;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isStreaming;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public AssetDirectoryViewModel(
        IAssetRepository assetRepository,
        IDataStreamService dataStreamService,
        ILogger<AssetDirectoryViewModel> logger) 
        : base("Asset Directory", "fa-solid fa-industry", -90)
    {
        _assetRepository = assetRepository;
        _dataStreamService = dataStreamService;
        _logger = logger;
    }

    partial void OnSelectedAssetChanged(Asset? value)
    {
        if (value != null)
        {
            _logger.LogInformation("Selected asset: {AssetName} (ID: {AssetId})", 
                value.Name, value.Id);
        }
    }

    [RelayCommand]
    private async Task LoadAssetsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading assets...";
            _logger.LogInformation("Loading assets from repository");

            var assets = await _assetRepository.GetAllAsync();
            Assets = new ObservableCollection<Asset>(assets);

            StatusMessage = $"Loaded {Assets.Count} assets";
            _logger.LogInformation("Successfully loaded {Count} assets", Assets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assets");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartStreamingAsync()
    {
        try
        {
            StatusMessage = "Starting data streaming...";
            _logger.LogInformation("Starting data stream service");

            await _dataStreamService.StartStreamingAsync();
            IsStreaming = true;

            StatusMessage = "Streaming active";
            _logger.LogInformation("Data streaming started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting data stream");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StopStreamingAsync()
    {
        try
        {
            StatusMessage = "Stopping data streaming...";
            _logger.LogInformation("Stopping data stream service");

            await _dataStreamService.StopStreamingAsync();
            IsStreaming = false;

            StatusMessage = "Streaming stopped";
            _logger.LogInformation("Data streaming stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping data stream");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAssetsAsync();
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
