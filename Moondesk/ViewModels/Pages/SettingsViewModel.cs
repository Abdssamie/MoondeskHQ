using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Serilog;
using AquaPP.Data;

namespace AquaPP.ViewModels.Pages;

public partial class SettingsViewModel : PageBase
{
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _dbContext;
    
    public List<ThemeVariant> Themes { get; } = new() { ThemeVariant.Light, ThemeVariant.Dark };

    private ThemeVariant _selectedTheme;

    public ThemeVariant SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                if (Application.Current is not null)
                {
                    Application.Current.RequestedThemeVariant = value;
                }
            }
        }
    }

    // Streaming Configuration
    [ObservableProperty]
    private int _updateInterval = 500;

    [ObservableProperty]
    private int _bufferSize = 1000;

    [ObservableProperty]
    private bool _autoSaveStreaming = true;

    // Data Retention
    [ObservableProperty]
    private int _retentionDays = 30;

    // System Information
    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _databaseSize = "Calculating...";

    [ObservableProperty]
    private int _totalSensorCount = 0;

    public SettingsViewModel(ILogger logger, ApplicationDbContext dbContext) : base("Settings", "fa-solid fa-gear", 4)
    {
        _logger = logger;
        _dbContext = dbContext;
        _selectedTheme = Application.Current?.RequestedThemeVariant ?? ThemeVariant.Default;
        
        InitializeSystemInformation();
    }

    public SettingsViewModel() : base("Settings", "fa-solid fa-gear", 4)
    {
        throw new System.NotImplementedException();
    }

    private async void InitializeSystemInformation()
    {
        // Get application version
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";

        // Get database size
        try
        {
            var dbPath = _dbContext.Database.GetConnectionString();
            if (!string.IsNullOrEmpty(dbPath))
            {
                // Extract file path from connection string
                var dataSourcePrefix = "Data Source=";
                var startIndex = dbPath.IndexOf(dataSourcePrefix);
                if (startIndex >= 0)
                {
                    var filePath = dbPath.Substring(startIndex + dataSourcePrefix.Length).Split(';')[0];
                    if (File.Exists(filePath))
                    {
                        var fileInfo = new FileInfo(filePath);
                        var sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                        DatabaseSize = $"{sizeInMB:F2} MB";
                    }
                    else
                    {
                        DatabaseSize = "N/A";
                    }
                }
                else
                {
                    DatabaseSize = "N/A";
                }
            }
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Failed to get database size");
            DatabaseSize = "Error";
        }

        // Get total sensor count
        try
        {
            TotalSensorCount = await _dbContext.Sensors.CountAsync();
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Failed to get sensor count");
            TotalSensorCount = 0;
        }
    }

    partial void OnUpdateIntervalChanged(int value)
    {
        if (AutoSaveStreaming)
        {
            _logger.Information("Update interval changed to {Interval}ms", value);
            // TODO: Apply streaming configuration changes
        }
    }

    partial void OnBufferSizeChanged(int value)
    {
        if (AutoSaveStreaming)
        {
            _logger.Information("Buffer size changed to {Size} points", value);
            // TODO: Apply streaming configuration changes
        }
    }

    partial void OnRetentionDaysChanged(int value)
    {
        _logger.Information("Data retention changed to {Days} days", value);
        // TODO: Apply data retention policy
    }
}