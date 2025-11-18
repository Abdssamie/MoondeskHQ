using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;

namespace AquaPP.ViewModels.Pages;

public partial class AlertConfigurationViewModel : PageBase
{
    private readonly ISukiToastManager? _toastManager;

    // Notification Channel Properties
    [ObservableProperty] private bool _desktopToastEnabled = true;
    [ObservableProperty] private bool _soundEnabled = true;
    [ObservableProperty] private bool _inAppEnabled = true;

    // Severity Level Selection per Channel
    [ObservableProperty] private string _desktopToastSeverity = "Warning";
    [ObservableProperty] private string _soundSeverity = "Critical";
    [ObservableProperty] private string _inAppSeverity = "Info";

    // Sound Selection
    [ObservableProperty] private string _selectedSound = "Default Alert";
    
    // Available Options
    [ObservableProperty] private ObservableCollection<string> _severityLevels = new();
    [ObservableProperty] private ObservableCollection<string> _availableSounds = new();
    
    // Notification History
    [ObservableProperty] private ObservableCollection<NotificationHistoryItem> _notificationHistory = new();
    
    [ObservableProperty] private bool _hasUnsavedChanges;

    public AlertConfigurationViewModel() : base("Alert Configuration", "fa-solid fa-bell-concierge", 7)
    {
        InitializeOptions();
        LoadSampleHistory();
    }

    public AlertConfigurationViewModel(ISukiToastManager toastManager) 
        : base("Alert Configuration", "fa-solid fa-bell-concierge", 7)
    {
        _toastManager = toastManager;
        InitializeOptions();
        LoadSampleHistory();
    }

    private void InitializeOptions()
    {
        SeverityLevels = new ObservableCollection<string>
        {
            "Info",
            "Warning",
            "Critical",
            "Emergency"
        };

        AvailableSounds = new ObservableCollection<string>
        {
            "Default Alert",
            "Chime",
            "Bell",
            "Siren",
            "Beep",
            "Alarm",
            "None"
        };
    }

    private void LoadSampleHistory()
    {
        NotificationHistory = new ObservableCollection<NotificationHistoryItem>
        {
            new NotificationHistoryItem
            {
                Timestamp = DateTimeOffset.Now.AddMinutes(-5),
                Channel = "Desktop Toast",
                Severity = "Critical",
                Message = "Temperature exceeded maximum threshold",
                Delivered = true
            },
            new NotificationHistoryItem
            {
                Timestamp = DateTimeOffset.Now.AddMinutes(-15),
                Channel = "Sound",
                Severity = "Warning",
                Message = "Water level below recommended threshold",
                Delivered = true
            },
            new NotificationHistoryItem
            {
                Timestamp = DateTimeOffset.Now.AddMinutes(-30),
                Channel = "In-App",
                Severity = "Info",
                Message = "Sensor maintenance scheduled",
                Delivered = true
            }
        };
    }

    [RelayCommand]
    private async Task TestNotification()
    {
        if (_toastManager != null)
        {
            _toastManager.CreateSimpleInfoToast()
                .WithTitle("Test Alert Notification")
                .WithContent("This is a test notification to verify your alert settings are working correctly.")
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Queue();
        }

        // Add to history
        NotificationHistory.Insert(0, new NotificationHistoryItem
        {
            Timestamp = DateTimeOffset.Now,
            Channel = "Test",
            Severity = "Info",
            Message = "Test notification sent",
            Delivered = true
        });

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Save()
    {
        // Save settings to preferences/database
        // This would typically persist to a settings service or database
        
        if (_toastManager != null)
        {
            _toastManager.CreateSimpleInfoToast()
                .WithTitle("Settings Saved")
                .WithContent("Alert notification settings have been saved successfully.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        HasUnsavedChanges = false;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ResetToDefault()
    {
        DesktopToastEnabled = true;
        SoundEnabled = true;
        InAppEnabled = true;
        
        DesktopToastSeverity = "Warning";
        SoundSeverity = "Critical";
        InAppSeverity = "Info";
        
        SelectedSound = "Default Alert";
        
        HasUnsavedChanges = true;
    }

    // Track changes
    partial void OnDesktopToastEnabledChanged(bool value) => HasUnsavedChanges = true;
    partial void OnSoundEnabledChanged(bool value) => HasUnsavedChanges = true;
    partial void OnInAppEnabledChanged(bool value) => HasUnsavedChanges = true;
    partial void OnDesktopToastSeverityChanged(string value) => HasUnsavedChanges = true;
    partial void OnSoundSeverityChanged(string value) => HasUnsavedChanges = true;
    partial void OnInAppSeverityChanged(string value) => HasUnsavedChanges = true;
    partial void OnSelectedSoundChanged(string value) => HasUnsavedChanges = true;
}

public partial class NotificationHistoryItem : ObservableObject
{
    [ObservableProperty] private DateTimeOffset _timestamp;
    [ObservableProperty] private string _channel = string.Empty;
    [ObservableProperty] private string _severity = string.Empty;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private bool _delivered;
}
