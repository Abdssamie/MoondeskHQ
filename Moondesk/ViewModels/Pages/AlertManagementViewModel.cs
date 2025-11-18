using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AquaPP.Core.Models.IoT;
using AquaPP.Core.Interfaces;

namespace AquaPP.ViewModels.Pages;

public partial class AlertManagementViewModel : PageBase
{
    private readonly IAssetRepository? _assetRepository;
    private readonly ISensorRepository? _sensorRepository;

    [ObservableProperty] private ObservableCollection<AlertViewModel> _alerts = new();
    [ObservableProperty] private ObservableCollection<AlertViewModel> _filteredAlerts = new();
    [ObservableProperty] private AlertViewModel? _selectedAlert;
    [ObservableProperty] private int _unacknowledgedCount;
    
    // Filter properties
    [ObservableProperty] private DateTime? _startDate = DateTime.Now.AddDays(-7);
    [ObservableProperty] private DateTime? _endDate = DateTime.Now;
    [ObservableProperty] private string? _selectedAssetFilter = "All Assets";
    [ObservableProperty] private string? _selectedSeverityFilter = "All Severities";
    
    [ObservableProperty] private ObservableCollection<string> _assetFilterOptions = new();
    [ObservableProperty] private ObservableCollection<string> _severityFilterOptions = new();
    
    [ObservableProperty] private bool _isLoading;

    public AlertManagementViewModel() : base("Alert Management", "fa-solid fa-bell", 3)
    {
        InitializeFilterOptions();
        LoadSampleData();
    }

    public AlertManagementViewModel(IAssetRepository assetRepository, ISensorRepository sensorRepository) 
        : base("Alert Management", "fa-solid fa-bell", 3)
    {
        _assetRepository = assetRepository;
        _sensorRepository = sensorRepository;
        
        InitializeFilterOptions();
        _ = LoadAlertsAsync();
    }

    private void InitializeFilterOptions()
    {
        AssetFilterOptions = new ObservableCollection<string>
        {
            "All Assets"
        };
        
        SeverityFilterOptions = new ObservableCollection<string>
        {
            "All Severities",
            "Info",
            "Warning",
            "Critical",
            "Emergency"
        };
    }

    private async Task LoadAlertsAsync()
    {
        if (_assetRepository == null || _sensorRepository == null)
            return;

        IsLoading = true;
        
        try
        {
            // Load alerts from repository
            // This will be implemented when repositories have alert methods
            await Task.Delay(500); // Simulate loading
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadSampleData()
    {
        // Sample data for design-time and initial testing
        var sampleAlerts = new List<AlertViewModel>
        {
            new AlertViewModel
            {
                Id = 1,
                Timestamp = DateTimeOffset.Now.AddHours(-2),
                AssetName = "Pump Station A",
                SensorName = "Temperature Sensor 1",
                Severity = AlertSeverity.Critical,
                Message = "Temperature exceeded maximum threshold",
                Status = "Unacknowledged",
                TriggerValue = 85.5,
                ThresholdValue = 80.0,
                Acknowledged = false
            },
            new AlertViewModel
            {
                Id = 2,
                Timestamp = DateTimeOffset.Now.AddHours(-5),
                AssetName = "Water Tank B",
                SensorName = "Level Sensor 2",
                Severity = AlertSeverity.Warning,
                Message = "Water level below recommended threshold",
                Status = "Acknowledged",
                TriggerValue = 45.2,
                ThresholdValue = 50.0,
                Acknowledged = true,
                AcknowledgedAt = DateTimeOffset.Now.AddHours(-4),
                AcknowledgedBy = "Operator"
            },
            new AlertViewModel
            {
                Id = 3,
                Timestamp = DateTimeOffset.Now.AddMinutes(-30),
                AssetName = "Compressor Unit C",
                SensorName = "Pressure Sensor 3",
                Severity = AlertSeverity.Emergency,
                Message = "Critical pressure spike detected",
                Status = "Unacknowledged",
                TriggerValue = 125.8,
                ThresholdValue = 100.0,
                Acknowledged = false
            },
            new AlertViewModel
            {
                Id = 4,
                Timestamp = DateTimeOffset.Now.AddHours(-1),
                AssetName = "Pump Station A",
                SensorName = "Vibration Sensor 4",
                Severity = AlertSeverity.Warning,
                Message = "Abnormal vibration detected",
                Status = "Unacknowledged",
                TriggerValue = 7.2,
                ThresholdValue = 5.0,
                Acknowledged = false
            },
            new AlertViewModel
            {
                Id = 5,
                Timestamp = DateTimeOffset.Now.AddDays(-1),
                AssetName = "Water Tank B",
                SensorName = "pH Sensor 5",
                Severity = AlertSeverity.Info,
                Message = "pH level slightly outside normal range",
                Status = "Acknowledged",
                TriggerValue = 7.8,
                ThresholdValue = 7.5,
                Acknowledged = true,
                AcknowledgedAt = DateTimeOffset.Now.AddHours(-20),
                AcknowledgedBy = "System"
            }
        };

        Alerts = new ObservableCollection<AlertViewModel>(sampleAlerts);
        
        // Update asset filter options
        var assetNames = sampleAlerts.Select(a => a.AssetName).Distinct().OrderBy(n => n);
        AssetFilterOptions = new ObservableCollection<string>(new[] { "All Assets" }.Concat(assetNames));
        
        ApplyFilters();
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var filtered = Alerts.AsEnumerable();

        // Date range filter
        if (StartDate.HasValue)
        {
            filtered = filtered.Where(a => a.Timestamp >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            filtered = filtered.Where(a => a.Timestamp <= EndDate.Value.AddDays(1));
        }

        // Asset filter
        if (!string.IsNullOrEmpty(SelectedAssetFilter) && SelectedAssetFilter != "All Assets")
        {
            filtered = filtered.Where(a => a.AssetName == SelectedAssetFilter);
        }

        // Severity filter
        if (!string.IsNullOrEmpty(SelectedSeverityFilter) && SelectedSeverityFilter != "All Severities")
        {
            if (Enum.TryParse<AlertSeverity>(SelectedSeverityFilter, out var severity))
            {
                filtered = filtered.Where(a => a.Severity == severity);
            }
        }

        FilteredAlerts = new ObservableCollection<AlertViewModel>(filtered.OrderByDescending(a => a.Timestamp));
        UnacknowledgedCount = FilteredAlerts.Count(a => !a.Acknowledged);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        StartDate = DateTime.Now.AddDays(-7);
        EndDate = DateTime.Now;
        SelectedAssetFilter = "All Assets";
        SelectedSeverityFilter = "All Severities";
        ApplyFilters();
    }

    [RelayCommand]
    private void AcknowledgeAlert()
    {
        if (SelectedAlert == null || SelectedAlert.Acknowledged)
            return;

        SelectedAlert.Acknowledged = true;
        SelectedAlert.Status = "Acknowledged";
        SelectedAlert.AcknowledgedAt = DateTimeOffset.Now;
        SelectedAlert.AcknowledgedBy = "Current User";
        
        ApplyFilters();
    }

    [RelayCommand]
    private void AcknowledgeAll()
    {
        foreach (var alert in FilteredAlerts.Where(a => !a.Acknowledged))
        {
            alert.Acknowledged = true;
            alert.Status = "Acknowledged";
            alert.AcknowledgedAt = DateTimeOffset.Now;
            alert.AcknowledgedBy = "Current User";
        }
        
        ApplyFilters();
    }

    [RelayCommand]
    private async Task ExportAlerts()
    {
        // Export functionality to be implemented
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void Refresh()
    {
        _ = LoadAlertsAsync();
    }

    partial void OnStartDateChanged(DateTime? value)
    {
        ApplyFilters();
    }

    partial void OnEndDateChanged(DateTime? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedAssetFilterChanged(string? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedSeverityFilterChanged(string? value)
    {
        ApplyFilters();
    }
}

public partial class AlertViewModel : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private DateTimeOffset _timestamp;
    [ObservableProperty] private string _assetName = string.Empty;
    [ObservableProperty] private string _sensorName = string.Empty;
    [ObservableProperty] private AlertSeverity _severity;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private double _triggerValue;
    [ObservableProperty] private double? _thresholdValue;
    [ObservableProperty] private bool _acknowledged;
    [ObservableProperty] private DateTimeOffset? _acknowledgedAt;
    [ObservableProperty] private string? _acknowledgedBy;
    [ObservableProperty] private string? _notes;
}
