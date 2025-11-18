using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using AquaPP.Common;
using AquaPP.Data;
using AquaPP.Services;
using AquaPP.Services.Csv;
using AquaPP.Services.File;
using AquaPP.Services.Units;
using AquaPP.Core.Interfaces;
using AquaPP.Infrastructure.Data;
using AquaPP.Infrastructure.Data.Repositories;
using AquaPP.Infrastructure.Streaming;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Markup.Xaml;
using AquaPP.ViewModels;
using AquaPP.ViewModels.Pages;
using AquaPP.Views;
using AquaPP.Views.Pages;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using Prism.Events;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Enrichers.CallStack;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace AquaPP;

#pragma warning disable SKEXP0070

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
public partial class App : Application
{
    public static IServiceProvider Services { get; set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        BindingPlugins.DataValidators.RemoveAt(0);


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var services = new ServiceCollection();
            
            services.AddSingleton(desktop);
            var views = ConfigureViews(services);
            var provider = ConfigureServices(services);
            
            Services = provider;
            
            DataTemplates.Add(new ViewLocator(views));
            desktop.MainWindow = views.CreateView<MainWindowViewModel>(provider) as Window;

            try
            {
                Log.Information("Attempting to get CustomSplitViewModel for the MainWindowView.");
                desktop.MainWindow = views.CreateView<MainWindowViewModel>(provider) as Window;
                Log.Information("MainWindowView and its DataContext set successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while getting the CustomSplitViewModel. This means the view model or one of its dependencies could not be resolved.");
                throw;
            }
        }

        try
        {
            Log.Information("Attempting to create a database scope and perform migration.");
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Log.Information("Successfully retrieved ApplicationDbContext from the service provider.");
            
            dbContext.Database.Migrate();
            Log.Information("Database migration completed successfully.");
            
            // Seed IoT test data
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            seeder.SeedAsync().Wait();
            Log.Information("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database migration. This is the most likely culprit if your app crashes here.");
            throw;
        }

        Log.Information("Avalonia application initialization completed successfully.");
        base.OnFrameworkInitializationCompleted();
    }
    
    private static SukiViews ConfigureViews(ServiceCollection services)
    {
        return new SukiViews()

            // Add main view
            .AddView<MainWindowView, MainWindowViewModel>(services)

            // Add pages
            .AddView<DialogView, DialogViewModel>(services)
            .AddView<ChatView, ChatViewModel>(services)
            .AddView<SimpleAppView, SimpleAppViewModel>(services)
            .AddView<SettingsView, SettingsViewModel>(services)
            .AddView<DataEntryView, DataEntryViewModel>(services)
            .AddView<AssetDirectoryView, AssetDirectoryViewModel>(services)
            .AddView<MonitoringDashboardView, MonitoringDashboardViewModel>(services)
            .AddView<SensorListView, SensorListViewModel>(services)
            .AddView<SensorDetailView, SensorDetailViewModel>(services)
            .AddView<AlertManagementView, AlertManagementViewModel>(services)
            .AddView<ThresholdConfigurationView, ThresholdConfigurationViewModel>(services)
            .AddView<AlertConfigurationView, AlertConfigurationViewModel>(services);
    }
    
    private static ServiceProvider ConfigureServices(ServiceCollection collection)
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithCallerInfo(includeFileInfo:true, allowedAssemblies:  ["AquaPP", "SukiUI", "Avalonia"])
            .WriteTo.Console()
            .WriteTo.File("app-debug.log")
            .Enrich.FromLogContext()
            .Enrich.WithCallStack()
            .CreateLogger();
        
        collection.AddSingleton<ILogger>(logger); // Register ILogger as a singleton
        
        // Add logging (optional but highly recommended for debugging Semantic Kernel)
        collection.AddLogging(builder =>
        {
            builder.AddSerilog(logger);
        });
        
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "aquapp.db");
        
        // Setup database context
        collection.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            );
        
        
        
        collection.AddSingleton<IUrlService, UrlService>();
        collection.AddSingleton<WaterQualityCsvService>(); // Register WaterQualityCsvService
        collection.AddSingleton<IFilePickerService, FilePickerService>(); // Register IFilePickerService
        collection.AddSingleton<IUnitConversionService, UnitConversionService>(); // Register IFilePickerService
        
        // IoT Services
        collection.AddScoped<IAssetRepository, AssetRepository>();
        collection.AddScoped<ISensorRepository, SensorRepository>();
        collection.AddSingleton<IDataStreamService, MockDataStreamService>();
        collection.AddTransient<DbSeeder>();
        
        collection.AddSingleton<PageNavigationService>();
        collection.AddSingleton<ISukiToastManager, SukiToastManager>();
        collection.AddSingleton<IEventAggregator, EventAggregator>();
        collection.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        
        return collection.BuildServiceProvider();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.AsEnumerable().OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
