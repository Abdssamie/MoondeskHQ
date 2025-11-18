using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using AquaPP.Infrastructure.Simulators;
using Microsoft.Extensions.Logging;

namespace AquaPP.Infrastructure.Streaming;

/// <summary>
/// Mock implementation of IDataStreamService for development and testing
/// </summary>
public class MockDataStreamService : IDataStreamService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly SensorDataSimulator _simulator;
    private readonly ILogger<MockDataStreamService> _logger;
    private readonly Dictionary<int, IDisposable> _activeStreams = new();
    private readonly Subject<Reading> _readingsSubject = new();
    
    public bool IsStreaming { get; private set; }

    public MockDataStreamService(
        ISensorRepository sensorRepository,
        ILogger<MockDataStreamService> logger)
    {
        _sensorRepository = sensorRepository;
        _simulator = new SensorDataSimulator();
        _logger = logger;
    }

    public IObservable<Reading> StreamReadings(int sensorId)
    {
        return _readingsSubject
            .Where(r => r.SensorId == sensorId)
            .AsObservable();
    }

    public IObservable<Reading> StreamAssetReadings(int assetId)
    {
        return _readingsSubject
            .Where(r => r.Sensor?.AssetId == assetId)
            .AsObservable();
    }

    public async Task<IEnumerable<Reading>> GetHistoricalReadingsAsync(
        int sensorId,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        // For mock service, generate some historical data
        var sensor = await _sensorRepository.GetByIdAsync(sensorId);
        if (sensor == null)
            return [];

        var readings = new List<Reading>();
        var current = from;
        var interval = TimeSpan.FromMilliseconds(sensor.SamplingIntervalMs);

        while (current <= to)
        {
            var reading = new Reading
            {
                SensorId = sensorId,
                Timestamp = current,
                Value = GenerateMockValue(sensor.Type),
                Quality = ReadingQuality.Simulated
            };
            readings.Add(reading);
            current = current.Add(interval);
        }

        return readings;
    }

    public async Task StartStreamingAsync()
    {
        if (IsStreaming)
        {
            _logger.LogWarning("Streaming already started");
            return;
        }

        _logger.LogInformation("Starting mock data streaming...");

        var sensors = await _sensorRepository.GetActiveSensorsAsync();

        var enumerable = sensors as Sensor[] ?? sensors.ToArray();
        foreach (var sensor in enumerable)
        {
            var stream = _simulator.SimulateSensor(sensor)
                .Subscribe(
                    reading =>
                    {
                        reading.Sensor = sensor;
                        _readingsSubject.OnNext(reading);
                    },
                    error => _logger.LogError(error, "Error in sensor stream {SensorId}", sensor.Id)
                );

            _activeStreams[sensor.Id] = stream;
            _logger.LogInformation("Started streaming for sensor {SensorId} ({SensorName})", 
                sensor.Id, sensor.Name);
        }

        IsStreaming = true;
        _logger.LogInformation("Mock data streaming started for {Count} sensors", enumerable.Count());
    }

    public Task StopStreamingAsync()
    {
        if (!IsStreaming)
        {
            _logger.LogWarning("Streaming not active");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Stopping mock data streaming...");

        foreach (var stream in _activeStreams.Values)
        {
            stream.Dispose();
        }

        _activeStreams.Clear();
        IsStreaming = false;

        _logger.LogInformation("Mock data streaming stopped");
        return Task.CompletedTask;
    }

    private double GenerateMockValue(SensorType type)
    {
        var random = new Random();
        return type switch
        {
            SensorType.Temperature => 20 + random.NextDouble() * 15,
            SensorType.Pressure => 90 + random.NextDouble() * 20,
            SensorType.Vibration => random.NextDouble() * 5,
            SensorType.FlowRate => 200 + random.NextDouble() * 100,
            SensorType.Level => 40 + random.NextDouble() * 40,
            _ => random.NextDouble() * 100
        };
    }
}
