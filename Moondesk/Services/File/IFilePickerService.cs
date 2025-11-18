using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SukiUI.Toasts;
using ILogger = Serilog.ILogger;

namespace AquaPP.Services.File;

public interface IFilePickerService
{
    Task<string?> PickSaveFileAsync(Control control, string title, string defaultExtension, params string[] filters);
    Task<string?> PickOpenFileAsync(Control control, string title, params string[] filters);
    Task<IEnumerable<string>> PickOpenFilesAsync(Control control, string title, params string[] filters);
}

public class FilePickerService : IFilePickerService
{
    private readonly ILogger _logger;
    private readonly ISukiToastManager _toastManager;

    public FilePickerService(ISukiToastManager toastManager, ILogger logger)
    {
        _toastManager = toastManager;
        _logger = logger;
    }

    public async Task<string?> PickSaveFileAsync(Control control, string title, string defaultExtension,
        params string[] filters)
    {
        var topLevel = TopLevel.GetTopLevel(control);

        if (topLevel == null)
        {
            _logger.Error("Failed to open file dialog. TopLevel is null.");
            _toastManager.CreateToast()
                .WithTitle("Error")
                .OfType(NotificationType.Error)
                .WithContent("Failed to open file dialog.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
#if DEBUG
            _toastManager.CreateToast()
                .WithTitle("Debug Error")
                .OfType(NotificationType.Error)
                .WithContent("Failed to open file dialog. Application window not found. (DEBUG)")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
#endif
            return null;
        }

        var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = defaultExtension,
            FileTypeChoices = filters.Select(f => new FilePickerFileType(f) { Patterns = [f] }).ToList()
        });

        return storageFile?.Path.LocalPath;
    }

    public async Task<string?> PickOpenFileAsync(Control control, string title, params string[] filters)
    {
        var topLevel = TopLevel.GetTopLevel(control);

        if (topLevel == null)
        {
            _logger.Error("Failed to open file dialog. TopLevel is null.");
            _toastManager.CreateToast()
                .WithTitle("Error")
                .OfType(NotificationType.Error)
                .WithContent("Failed to open file dialog.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
#if DEBUG
            _toastManager.CreateToast()
                .WithTitle("Debug Error")
                .OfType(NotificationType.Error)
                .WithContent("Failed to open file dialog. Application window not found. (DEBUG)")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
#endif
            return null;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = filters.Select(f => new FilePickerFileType(f) { Patterns = [f] }).ToList()
        });

        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<IEnumerable<string>> PickOpenFilesAsync(Control control, string title, params string[] filters)
    {
        var topLevel = TopLevel.GetTopLevel(control);

        if (topLevel == null)
        {
            _logger.Error("Failed to open file dialog. TopLevel is null.");
            _toastManager.CreateToast()
                .WithTitle("Error")
                .OfType(NotificationType.Error)
                .WithContent("Failed to open file dialog.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
#if DEBUG
            _toastManager.CreateToast()
                .WithTitle("Debug Error")
                .OfType(NotificationType.Error)
                .WithContent("Failed to open file dialog. Application window not found. (DEBUG)")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
#endif
            return new List<string>();
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = true,
            FileTypeFilter = filters.Select(f => new FilePickerFileType(f) { Patterns = [f] }).ToList()
        });

        return files.Select(f => f.Path.LocalPath).ToList();
    }
}