using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moondesk.BackgroundServices.Services;
using Moondesk.DataAccess.Configuration;
using Moondesk.DataAccess.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Configure Serilog
    builder.Services.AddSerilog();

    // Add database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found");
    
    builder.Services.AddMoondeskDataAccess(connectionString);
    builder.Services.AddRepositories();
    
    // Register Encryption Service
    builder.Services.AddSingleton<Moondesk.Domain.Interfaces.Services.IEncryptionService, Moondesk.BackgroundServices.Services.EncryptionService>();

    // Add MQTT ingestion service
    builder.Services.AddHostedService<MqttIngestionService>();

    var host = builder.Build();
    
    Log.Information("Moondesk Background Services starting...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

