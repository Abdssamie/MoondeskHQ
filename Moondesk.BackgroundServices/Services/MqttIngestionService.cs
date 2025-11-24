using System.Text.Json;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Interfaces.Services;
using Moondesk.Domain.Models.IoT;
using MQTTnet;
using Microsoft.Extensions.DependencyInjection; // Added for CreateScope

namespace Moondesk.BackgroundServices.Services;

public class MqttIngestionService : BackgroundService
{
    private readonly ILogger<MqttIngestionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private IMqttClient? _mqttClient;

    public MqttIngestionService(
        ILogger<MqttIngestionService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT Ingestion Service starting...");

        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        string host = _configuration["MQTT:Host"] ?? "localhost";
        int port = int.Parse(_configuration["MQTT:Port"] ?? "1883");
        string username = _configuration["MQTT:Username"] ?? "";
        string password = _configuration["MQTT:Password"] ?? "";
        string clientId = $"moondesk-ingestion-{Guid.NewGuid()}";

        // Try to load from DB
        try 
        {
            using var scope = _serviceProvider.CreateScope();
            var credentialRepo = scope.ServiceProvider.GetRequiredService<IConnectionCredentialRepository>();
            var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
            var orgRepo = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();

            // Strategy: Find ANY active MQTT credential. 
            // In a real multi-tenant system, we might iterate all of them.
            // For now, we pick the first one we find to support the "Frontend Configured" requirement.
            
            var orgs = await orgRepo.GetAllAsync();
            foreach (var org in orgs)
            {
                var creds = await credentialRepo.GetByProtocolAsync(Protocol.Mqtt, org.Id);
                var activeCred = creds.FirstOrDefault(c => c.IsActive);
                
                if (activeCred != null)
                {
                    _logger.LogInformation("Found database credential for Organization {OrgId}: {Name}", org.Id, activeCred.Name);
                    
                    var uri = new Uri(activeCred.EndpointUri);
                    host = uri.Host;
                    port = uri.Port > 0 ? uri.Port : 1883;
                    username = activeCred.Username;
                    password = encryptionService.Decrypt(activeCred.EncryptedPassword, activeCred.EncryptionIV);
                    if (!string.IsNullOrEmpty(activeCred.ClientId))
                    {
                        clientId = activeCred.ClientId;
                    }
                    
                    break; // Found one, use it.
                }
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to load credentials from database. Falling back to appsettings.");
        }

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .WithCleanSession()
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

        try
        {
            await _mqttClient.ConnectAsync(options, stoppingToken);
            _logger.LogInformation("Connected to MQTT broker");

            // Subscribe to telemetry topics: {org_id}/{device_id}/telemetry
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("+/+/telemetry")
                .Build();
            
            await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);
            _logger.LogInformation("Subscribed to telemetry topics");

            // Keep service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MQTT Ingestion Service");
        }
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            _logger.LogDebug("Received message on topic: {Topic}", topic);

            // Parse topic: org_id/device_id/telemetry
            var parts = topic.Split('/');
            if (parts.Length != 3)
            {
                _logger.LogWarning("Invalid topic format: {Topic}", topic);
                return;
            }

            var organizationId = parts[0];
            var deviceId = parts[1];

            // Deserialize payload
            var telemetry = JsonSerializer.Deserialize<TelemetryPayload>(payload);
            if (telemetry == null)
            {
                _logger.LogWarning("Failed to deserialize payload from {Topic}", topic);
                return;
            }

            if (deviceId != telemetry.SensorId.ToString())
            {
                _logger.LogWarning("deviceId {DeviceId} in topic does not match the sensorId {SensorId} in the payload", deviceId, telemetry.SensorId);
                throw new Exception("There is inconsistency between the telemetry topic and the payload");
            }
 
            // Store reading in database
            using var scope = _serviceProvider.CreateScope();
            var readingRepo = scope.ServiceProvider.GetRequiredService<IReadingRepository>();
            var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
            var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var reading = new Reading
            {
                SensorId = telemetry.SensorId,
                OrganizationId = organizationId,
                Timestamp = telemetry.Timestamp,
                Value = telemetry.Value,
                Quality = telemetry.Quality,
                Protocol = Protocol.Mqtt
            };

            await readingRepo.BulkInsertReadingsAsync(new[] { reading });
            _logger.LogInformation("Stored reading for sensor {SensorId}: {Value}", 
                telemetry.SensorId, telemetry.Value);

            // Send real-time update via SignalR
            if (notificationService is SignalRNotificationService signalRService)
            {
                await signalRService.SendSensorReadingAsync(reading, organizationId);
            }

            // Check thresholds and create alerts
            var sensor = await sensorRepo.GetByIdAsync((int)telemetry.SensorId);
            if (sensor != null && 
                (telemetry.Value > sensor.ThresholdHigh || telemetry.Value < sensor.ThresholdLow))
            {
                var alert = new Alert
                {
                    SensorId = telemetry.SensorId,
                    OrganizationId = organizationId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Severity = DetermineSeverity(telemetry.Value, sensor),
                    Message = $"Sensor {sensor.Name} threshold violation: {telemetry.Value} {sensor.Unit}",
                    TriggerValue = telemetry.Value,
                    ThresholdValue = telemetry.Value > sensor.ThresholdHigh 
                        ? sensor.ThresholdHigh 
                        : sensor.ThresholdLow
                };

                await alertRepo.CreateAlertAsync(alert);
                _logger.LogWarning("Alert created for sensor {SensorId}", telemetry.SensorId);

                // Send alert notification
                await notificationService.SendAlertNotificationAsync(alert, organizationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MQTT message");
        }
    }

    private AlertSeverity DetermineSeverity(double value, Sensor sensor)
    {
        var threshold = value > sensor.ThresholdHigh 
            ? sensor.ThresholdHigh.GetValueOrDefault() 
            : sensor.ThresholdLow.GetValueOrDefault();
        
        var percentOver = Math.Abs((value - threshold) / threshold * 100);

        return percentOver switch
        {
            > 50 => AlertSeverity.Emergency,
            > 25 => AlertSeverity.Critical,
            > 10 => AlertSeverity.Warning,
            _ => AlertSeverity.Info
        };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Ingestion Service stopping...");
        
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    private class TelemetryPayload
    {
        public long SensorId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public double Value { get; set; }
        public ReadingQuality Quality { get; set; } = ReadingQuality.Good;
    }
}
