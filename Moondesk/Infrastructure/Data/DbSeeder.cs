using System;
using System.Threading.Tasks;
using AquaPP.Core.Models.IoT;
using AquaPP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AquaPP.Infrastructure.Data;

/// <summary>
/// Seeds the database with initial test data for development
/// </summary>
public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        if (await _context.Assets.AnyAsync())
        {
            _logger.LogInformation("Database already seeded, skipping...");
            return;
        }

        _logger.LogInformation("Seeding database with test data...");

        var assets = new[]
        {
            new Asset
            {
                Name = "Primary Water Pump #1",
                Type = "Centrifugal Pump",
                Location = "Building A - Basement",
                Status = AssetStatus.Online,
                LastSeen = DateTimeOffset.UtcNow,
                Description = "Main water circulation pump for cooling system",
                Manufacturer = "Grundfos",
                ModelNumber = "CR 64-2",
                InstallationDate = new DateTimeOffset(2020, 3, 15, 0, 0, 0, TimeSpan.Zero)
            },
            new Asset
            {
                Name = "Cooling Tower CT-01",
                Type = "Cooling Tower",
                Location = "Rooftop - North Wing",
                Status = AssetStatus.Online,
                LastSeen = DateTimeOffset.UtcNow,
                Description = "Primary cooling tower for HVAC system",
                Manufacturer = "Baltimore Aircoil",
                ModelNumber = "VTI-1500",
                InstallationDate = new DateTimeOffset(2019, 8, 22, 0, 0, 0, TimeSpan.Zero)
            },
            new Asset
            {
                Name = "Air Compressor AC-02",
                Type = "Rotary Screw Compressor",
                Location = "Building B - Mechanical Room",
                Status = AssetStatus.Warning,
                LastSeen = DateTimeOffset.UtcNow.AddMinutes(-5),
                Description = "Backup air compressor for pneumatic systems",
                Manufacturer = "Atlas Copco",
                ModelNumber = "GA 37",
                InstallationDate = new DateTimeOffset(2021, 1, 10, 0, 0, 0, TimeSpan.Zero)
            },
            new Asset
            {
                Name = "Storage Tank TK-05",
                Type = "Storage Tank",
                Location = "Building A - Ground Floor",
                Status = AssetStatus.Online,
                LastSeen = DateTimeOffset.UtcNow,
                Description = "Process water storage tank - 5000L capacity",
                Manufacturer = "Enduramaxx",
                ModelNumber = "5000VT",
                InstallationDate = new DateTimeOffset(2018, 11, 5, 0, 0, 0, TimeSpan.Zero)
            },
            new Asset
            {
                Name = "Chiller Unit CH-03",
                Type = "Water Chiller",
                Location = "Building C - Mechanical Room",
                Status = AssetStatus.Maintenance,
                LastSeen = DateTimeOffset.UtcNow.AddHours(-2),
                Description = "Process cooling chiller - scheduled maintenance",
                Manufacturer = "Carrier",
                ModelNumber = "30XA-1002",
                InstallationDate = new DateTimeOffset(2022, 6, 18, 0, 0, 0, TimeSpan.Zero)
            }
        };

        _context.Assets.AddRange(assets);
        await _context.SaveChangesAsync();

        // Add sensors for each asset
        var sensors = new[]
        {
            // Primary Water Pump #1 sensors
            new Sensor
            {
                AssetId = assets[0].Id,
                Name = "Discharge Pressure",
                Type = SensorType.Pressure,
                Unit = "PSI",
                ThresholdLow = 85,
                ThresholdHigh = 115,
                MinValue = 0,
                MaxValue = 150,
                SamplingIntervalMs = 1000,
                IsActive = true,
                Description = "Pump discharge pressure monitoring"
            },
            new Sensor
            {
                AssetId = assets[0].Id,
                Name = "Motor Temperature",
                Type = SensorType.Temperature,
                Unit = "°C",
                ThresholdLow = 10,
                ThresholdHigh = 75,
                MinValue = 0,
                MaxValue = 100,
                SamplingIntervalMs = 2000,
                IsActive = true,
                Description = "Motor winding temperature"
            },
            new Sensor
            {
                AssetId = assets[0].Id,
                Name = "Vibration Level",
                Type = SensorType.Vibration,
                Unit = "mm/s",
                ThresholdLow = 0,
                ThresholdHigh = 7.1,
                MinValue = 0,
                MaxValue = 20,
                SamplingIntervalMs = 500,
                IsActive = true,
                Description = "Bearing vibration monitoring"
            },
            new Sensor
            {
                AssetId = assets[0].Id,
                Name = "Flow Rate",
                Type = SensorType.FlowRate,
                Unit = "L/min",
                ThresholdLow = 200,
                ThresholdHigh = 400,
                MinValue = 0,
                MaxValue = 500,
                SamplingIntervalMs = 1000,
                IsActive = true,
                Description = "Water flow rate"
            },

            // Cooling Tower CT-01 sensors
            new Sensor
            {
                AssetId = assets[1].Id,
                Name = "Water Temperature In",
                Type = SensorType.Temperature,
                Unit = "°C",
                ThresholdLow = 25,
                ThresholdHigh = 40,
                MinValue = 0,
                MaxValue = 60,
                SamplingIntervalMs = 2000,
                IsActive = true,
                Description = "Inlet water temperature"
            },
            new Sensor
            {
                AssetId = assets[1].Id,
                Name = "Water Temperature Out",
                Type = SensorType.Temperature,
                Unit = "°C",
                ThresholdLow = 15,
                ThresholdHigh = 30,
                MinValue = 0,
                MaxValue = 50,
                SamplingIntervalMs = 2000,
                IsActive = true,
                Description = "Outlet water temperature"
            },
            new Sensor
            {
                AssetId = assets[1].Id,
                Name = "Fan Speed",
                Type = SensorType.Speed,
                Unit = "RPM",
                ThresholdLow = 500,
                ThresholdHigh = 1800,
                MinValue = 0,
                MaxValue = 2000,
                SamplingIntervalMs = 1500,
                IsActive = true,
                Description = "Cooling fan speed"
            },

            // Air Compressor AC-02 sensors
            new Sensor
            {
                AssetId = assets[2].Id,
                Name = "Discharge Pressure",
                Type = SensorType.Pressure,
                Unit = "PSI",
                ThresholdLow = 100,
                ThresholdHigh = 125,
                MinValue = 0,
                MaxValue = 150,
                SamplingIntervalMs = 1000,
                IsActive = true,
                Description = "Compressed air pressure"
            },
            new Sensor
            {
                AssetId = assets[2].Id,
                Name = "Oil Temperature",
                Type = SensorType.Temperature,
                Unit = "°C",
                ThresholdLow = 60,
                ThresholdHigh = 95,
                MinValue = 0,
                MaxValue = 120,
                SamplingIntervalMs = 2000,
                IsActive = true,
                Description = "Compressor oil temperature"
            },

            // Storage Tank TK-05 sensors
            new Sensor
            {
                AssetId = assets[3].Id,
                Name = "Water Level",
                Type = SensorType.Level,
                Unit = "%",
                ThresholdLow = 20,
                ThresholdHigh = 90,
                MinValue = 0,
                MaxValue = 100,
                SamplingIntervalMs = 3000,
                IsActive = true,
                Description = "Tank fill level percentage"
            },
            new Sensor
            {
                AssetId = assets[3].Id,
                Name = "Water pH",
                Type = SensorType.pH,
                Unit = "pH",
                ThresholdLow = 6.5,
                ThresholdHigh = 8.5,
                MinValue = 0,
                MaxValue = 14,
                SamplingIntervalMs = 5000,
                IsActive = true,
                Description = "Water pH level"
            },

            // Chiller Unit CH-03 sensors
            new Sensor
            {
                AssetId = assets[4].Id,
                Name = "Chilled Water Temp",
                Type = SensorType.Temperature,
                Unit = "°C",
                ThresholdLow = 5,
                ThresholdHigh = 12,
                MinValue = 0,
                MaxValue = 30,
                SamplingIntervalMs = 2000,
                IsActive = false, // Maintenance mode
                Description = "Chilled water supply temperature"
            },
            new Sensor
            {
                AssetId = assets[4].Id,
                Name = "Power Consumption",
                Type = SensorType.Power,
                Unit = "kW",
                ThresholdLow = 0,
                ThresholdHigh = 800,
                MinValue = 0,
                MaxValue = 1000,
                SamplingIntervalMs = 1000,
                IsActive = false, // Maintenance mode
                Description = "Electrical power consumption"
            }
        };

        _context.Sensors.AddRange(sensors);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Database seeded successfully with {AssetCount} assets and {SensorCount} sensors",
            assets.Length, sensors.Length);
    }
}
