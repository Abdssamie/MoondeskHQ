using System;
using System.Threading.Tasks;
using AquaPP.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Splat;
using SukiUI.Toasts;
using ILogger = Serilog.ILogger;


namespace AquaPP.Services;

public interface IUrlService
{
    Task OpenUrlAsync(Control control, string url);
}

public class UrlService : IUrlService
{   
    private readonly ILogger _logger;
    private readonly ISukiToastManager _toastManager;

    public UrlService(ISukiToastManager toastManager, ILogger logger)
    {
        _logger = logger;
        _toastManager = toastManager;
    }
    
    public async Task OpenUrlAsync(Control control, string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            var topLevel = TopLevel.GetTopLevel(control);
            
            if (topLevel == null)
            {
                _logger.Error("Failed to open URL: {url}. TopLevel is null.", url);
                return;
            }
            
            var launcher = topLevel.Launcher;
            
            _logger.Information("Opening URL: {url}", url);
            
            // LaunchUriAsync returns a boolean indicating success
            var success = await launcher.LaunchUriAsync(uri);
            
            
            if (!success)
            {
                _logger.Error("Failed to open URL: {url}", url);
                _toastManager.CreateToast()
                    .WithTitle("Error Opening URL")
                    .OfType(NotificationType.Error)
                    .WithContent("Failed to open the URL. Please check your network settings and try again.")
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Dismiss().ByClicking()
                    .Queue();
            }
            else
            {
                _logger.Information("URL opened successfully: {success}", success);
            }
        }
    }
}