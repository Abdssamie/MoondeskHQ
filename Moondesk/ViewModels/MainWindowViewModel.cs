using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaPP.Services;
using AquaPP.Services.File;
using AquaPP.ViewModels.Pages;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Events;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using ILogger = Serilog.ILogger;

namespace AquaPP.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly IUrlService _urlService;

    private readonly IFilePickerService _filePickerService;
    private readonly IEventAggregator _eventAggregator;
    public ISukiToastManager ToastManager { get; }
    public ISukiDialogManager DialogManager { get; }
    public PageNavigationService PageNavigationService { get; set; }
    public IAvaloniaReadOnlyList<PageBase> Pages { get; }
    [ObservableProperty] private PageBase? _activePage;
    
    /// <summary>
    /// Design time only constructor
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public MainWindowViewModel()
    {
        
    }
#pragma warning restore CS8618
    public MainWindowViewModel(IEnumerable<PageBase> pages,
        PageNavigationService pageNavigationService,
        ISukiToastManager toastManager,
        ISukiDialogManager dialogManager,
        ILogger logger,
        IEventAggregator eventAggregator,
        IFilePickerService filePickerService,
        IUrlService urlService)

    {
        _logger = logger;
        _eventAggregator = eventAggregator;
        _filePickerService = filePickerService;
        _urlService = urlService;
        ToastManager = toastManager;
        DialogManager = dialogManager;
        Pages = new AvaloniaList<PageBase>(pages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        PageNavigationService = pageNavigationService;


        // Subscribe to the navigation service (when a page navigation is requested)
        PageNavigationService.NavigationRequested += pageType =>
        {
            var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
            if (page is null || ActivePage?.GetType() == pageType) return;
            ActivePage = page;
        };
    }
    
    [RelayCommand]
    private async Task OnOpenFile(Control control)
    {
        var localFilePath = await _filePickerService.PickOpenFileAsync(control, "Open file", ["csv", "excel"]);
        if (localFilePath != null) _eventAggregator.GetEvent<OpenFileEvent>().Publish(localFilePath);
    }

    [RelayCommand]
    private async Task OpenUrlAsync(object parameter)
    {
        _logger.Information("OpenUrlCommand executed. Received parameter type: {type}", parameter.GetType());

        // --- defensive code starts here ---
        if (parameter is not object[] parameters || parameters.Length < 2)
        {
            _logger.Error("Invalid CommandParameter. Expected an object array with at least 2 elements.");
            return; // Exit gracefully
        }

        if (parameters[0] is not Control control)
        {
            _logger.Error("First element in CommandParameter was not a valid Control.");
            return;
        }

        var url = parameters[1] as string;

        if (string.IsNullOrEmpty(url))
        {
            _logger.Error("Second element in CommandParameter was not a valid URL string.");
            return;
        }
        // --- defensive code ends here ---

        try
        {
            await _urlService.OpenUrlAsync(control, url);
            _logger.Information("OpenUrlCommand: Url opened successfully: {url}", url);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error opening url: {url}", url);
        }
    }
}