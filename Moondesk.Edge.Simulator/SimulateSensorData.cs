using System.Text.Json;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;
using MQTTnet;
using MQTTnet.Protocol;

namespace Moondesk.Edge.Simulator;

public abstract class SensorReadingSimulator
{
    private static Sensor _sensor = new Sensor {OrganizationId = "78976" ,Asset = new Asset { Location = "Home" , OrganizationId = "78976"} };

    public static async Task PublishAsync(IMqttClient client)
    {
        var random = new Random();

        while (true)
        {
            var reading = new Reading
            {
                OrganizationId = "78976",
                Id = random.NextInt64(minValue: 1000000000000000, maxValue: 9999999999999999),
                Quality = GetReadingQuality(),
                Sensor = _sensor,
                SensorId = _sensor.Id,
                Value = random.NextDouble(),
                Timestamp = DateTimeOffset.UtcNow,
            };

            var payload = JsonSerializer.Serialize(reading);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("sensor-reading")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(message, CancellationToken.None);
                Console.WriteLine($@"[READING PUBLISHED] {message}");
            }
            
            // Generate a random delay in seconds
            var delay = random.NextDouble() * 9 + 1;
            await Task.Delay(TimeSpan.FromSeconds(delay));
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public static async Task SimulateAlert(IMqttClient client)
    {
        var random = new Random();

        while (true)
        {
            var alert = new Alert
            {
                OrganizationId = "78976",
                Acknowledged = false,
                Sensor = _sensor,
                SensorId = _sensor.Id,
                Message = "Unknown issue detected",
                Severity = AlertSeverity.Emergency
            };

            var payload = JsonSerializer.Serialize(alert);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("sensor-alert")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(message, CancellationToken.None);
                Console.WriteLine($@"[ALERT PUBLISHED] {message}");
                
                var delay = random.NextDouble() * 30 + 30;
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }
        // ReSharper disable once FunctionNeverReturns
        // This is a feature and not a bug
    }

    private static ReadingQuality GetReadingQuality()
    {
        Array values = Enum.GetValues(typeof(ReadingQuality));

        int index = Random.Shared.Next(values.Length);

        return (ReadingQuality)(values.GetValue(index) ?? ReadingQuality.Simulated);
    }
}