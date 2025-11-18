using System;
using System.Reactive.Linq;
using AquaPP.Core.Models.IoT;

namespace AquaPP.Infrastructure.Simulators;

/// <summary>
/// Generates realistic simulated sensor data for development and testing
/// </summary>
public class SensorDataSimulator
{
    private readonly Random _random = new();

    /// <summary>
    /// Create an observable stream of sensor readings
    /// </summary>
    public IObservable<Reading> SimulateSensor(Sensor sensor)
    {
        return Observable.Interval(TimeSpan.FromMilliseconds(sensor.SamplingIntervalMs))
            .Select(_ => GenerateReading(sensor));
    }

    private Reading GenerateReading(Sensor sensor)
    {
        var value = sensor.Type switch
        {
            SensorType.Temperature => GenerateTemperature(),
            SensorType.Pressure => GeneratePressure(),
            SensorType.Vibration => GenerateVibration(),
            SensorType.FlowRate => GenerateFlowRate(),
            SensorType.Level => GenerateLevel(),
            SensorType.Humidity => GenerateHumidity(),
            SensorType.Power => GeneratePower(),
            SensorType.Speed => GenerateSpeed(),
            SensorType.pH => GeneratePH(),
            SensorType.Conductivity => GenerateConductivity(),
            _ => GenerateGeneric(sensor.MinValue ?? 0, sensor.MaxValue ?? 100)
        };

        // Add some noise
        value += (_random.NextDouble() - 0.5) * (value * 0.02); // ±1% noise

        // Clamp to sensor range if defined
        if (sensor.MinValue.HasValue && value < sensor.MinValue.Value)
            value = sensor.MinValue.Value;
        if (sensor.MaxValue.HasValue && value > sensor.MaxValue.Value)
            value = sensor.MaxValue.Value;

        return new Reading
        {
            SensorId = sensor.Id,
            Timestamp = DateTimeOffset.UtcNow,
            Value = Math.Round(value, 2),
            Quality = ReadingQuality.Simulated
        };
    }

    private double GenerateTemperature()
    {
        // Simulate temperature between 15-35°C with slow drift
        var baseTemp = 25.0;
        var drift = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 60.0) * 5.0;
        return baseTemp + drift;
    }

    private double GeneratePressure()
    {
        // Simulate pressure between 80-120 PSI
        var basePressure = 100.0;
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 30.0) * 10.0;
        return basePressure + variation;
    }

    private double GenerateVibration()
    {
        // Simulate vibration 0-10 Hz with occasional spikes
        var baseVibration = 2.0;
        var spike = _random.NextDouble() < 0.05 ? _random.NextDouble() * 5.0 : 0;
        return baseVibration + spike + (_random.NextDouble() * 0.5);
    }

    private double GenerateFlowRate()
    {
        // Simulate flow rate 0-500 L/min
        var baseFlow = 250.0;
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 45.0) * 100.0;
        return Math.Max(0, baseFlow + variation);
    }

    private double GenerateLevel()
    {
        // Simulate tank level 0-100%
        var baseLevel = 60.0;
        var drift = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 120.0) * 20.0;
        return Math.Clamp(baseLevel + drift, 0, 100);
    }

    private double GenerateHumidity()
    {
        // Simulate humidity 30-70%
        var baseHumidity = 50.0;
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 90.0) * 15.0;
        return Math.Clamp(baseHumidity + variation, 0, 100);
    }

    private double GeneratePower()
    {
        // Simulate power consumption 0-1000 kW
        var basePower = 500.0;
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 60.0) * 200.0;
        return Math.Max(0, basePower + variation);
    }

    private double GenerateSpeed()
    {
        // Simulate motor speed 0-3000 RPM
        var baseSpeed = 1500.0;
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 40.0) * 300.0;
        return Math.Max(0, baseSpeed + variation);
    }

    private double GeneratePH()
    {
        // Simulate pH 6.5-8.5
        var basePH = 7.5;
        var drift = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 100.0) * 0.5;
        return basePH + drift;
    }

    private double GenerateConductivity()
    {
        // Simulate conductivity 100-500 µS/cm
        var baseCond = 300.0;
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 70.0) * 100.0;
        return baseCond + variation;
    }

    private double GenerateGeneric(double min, double max)
    {
        var range = max - min;
        var mid = min + (range / 2);
        var variation = Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds / 50.0) * (range / 4);
        return mid + variation;
    }
}
