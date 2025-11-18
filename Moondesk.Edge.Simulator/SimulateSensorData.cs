using System.Text.Json;
using Moondesk.Core.Models.IoT;
using MQTTnet;
using MQTTnet.Protocol;

namespace AquaPP.Edge.Simulator;

public abstract class SensorReadingSimulator
{
    private static Sensor _sensor = new Sensor { Asset = new Asset { Location = "Home" } };

    public static async Task PublishAsync(IMqttClient client)
    {
        var random = new Random();

        while (true)
        {
            var reading = new Reading
            {
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
                Console.WriteLine($@"[PUBLISHED] {message}");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static ReadingQuality GetReadingQuality()
    {
        Array values = Enum.GetValues(typeof(ReadingQuality));

        int index = Random.Shared.Next(values.Length);

        return (ReadingQuality)(values.GetValue(index) ?? ReadingQuality.Simulated);
    }
}
