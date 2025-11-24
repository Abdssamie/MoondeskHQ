using System.Text.Json;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;
using MQTTnet;
using MQTTnet.Protocol;

namespace Moondesk.Edge.Simulator;

public abstract class SensorReadingSimulator
{
    public static async Task PublishAsync(IMqttClient client, string organizationId)
    {
        var random = new Random();
        var sensor = new Sensor { OrganizationId = organizationId, Asset = new Asset { Location = "Home", OrganizationId = organizationId } };

        while (true)
        {
            var reading = new Reading
            {
                OrganizationId = organizationId,
                Quality = GetReadingQuality(),
                Sensor = sensor,
                SensorId = sensor.Id,
                Value = random.NextDouble(),
                Timestamp = DateTimeOffset.UtcNow,
            };

            var payload = JsonSerializer.Serialize(reading);
            
            // Topic format must match Backend: {OrganizationId}/{SensorId}/telemetry
            string topic = $"{reading.OrganizationId}/{reading.SensorId}/telemetry";

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
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

    public static async Task SimulateAlert(IMqttClient client, string organizationId)
    {
        var random = new Random();
        var sensor = new Sensor { OrganizationId = organizationId, Asset = new Asset { Location = "Home", OrganizationId = organizationId } };

        while (true)
        {
            var alert = new Alert
            {
                OrganizationId = organizationId,
                Acknowledged = false,
                Sensor = sensor,
                SensorId = sensor.Id,
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