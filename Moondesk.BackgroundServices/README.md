# Moondesk.BackgroundServices

Background services for MQTT telemetry ingestion and processing.

## Services

### MqttIngestionService
Subscribes to MQTT topics and ingests sensor telemetry data.

**Features:**
- Connects to MQTT broker (EMQX Cloud or local)
- Subscribes to: `{org_id}/{device_id}/telemetry`
- Stores readings in TimescaleDB
- Checks sensor thresholds
- Creates alerts on violations
- Automatic reconnection on failure

**Message Format:**
```json
{
  "sensorId": 123,
  "timestamp": "2025-11-19T20:00:00Z",
  "value": 25.5,
  "quality": "Good"
}
```

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=moondeskdb;..."
  },
  "MQTT": {
    "Host": "broker.emqx.io",
    "Port": "1883",
    "Username": "your-username",
    "Password": "your-password"
  }
}
```

### Environment Variables (Production)
```bash
ConnectionStrings__DefaultConnection="Host=..."
MQTT__Host="your-broker.emqx.io"
MQTT__Port="8883"
MQTT__Username="device_user"
MQTT__Password="secure_password"
```

## Running

### Development
```bash
cd Moondesk.BackgroundServices
dotnet run
```

### Production (systemd)
```bash
# Build
dotnet publish -c Release -o /opt/moondesk-services

# Create service file: /etc/systemd/system/moondesk-services.service
[Unit]
Description=Moondesk Background Services
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/moondesk-services
ExecStart=/usr/bin/dotnet /opt/moondesk-services/Moondesk.BackgroundServices.dll
Restart=always
RestartSec=10
User=moondesk

[Install]
WantedBy=multi-user.target

# Enable and start
sudo systemctl enable moondesk-services
sudo systemctl start moondesk-services
sudo systemctl status moondesk-services
```

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "Moondesk.BackgroundServices.dll"]
```

## Testing

### Local MQTT Broker
```bash
# Start Mosquitto
docker run -d -p 1883:1883 eclipse-mosquitto

# Publish test message
mosquitto_pub -h localhost -t "org_123/device_001/telemetry" \
  -m '{"sensorId":1,"timestamp":"2025-11-19T20:00:00Z","value":25.5,"quality":"Good"}'
```

### EMQX Cloud
1. Sign up at https://www.emqx.com/en/cloud
2. Create deployment
3. Create device credentials
4. Update appsettings.json with connection details

## Monitoring

### Logs
```bash
# View logs
journalctl -u moondesk-services -f

# Or with Docker
docker logs -f moondesk-services
```

### Health Check
Service logs connection status and message processing:
```
[20:00:00 INF] MQTT Ingestion Service starting...
[20:00:01 INF] Connected to MQTT broker
[20:00:01 INF] Subscribed to telemetry topics
[20:00:05 INF] Stored reading for sensor 1: 25.5
```

## Architecture

```
MQTT Broker (EMQX)
       ↓
MqttIngestionService
       ↓
   ┌───────────────┐
   │ Parse Message │
   └───────┬───────┘
           ↓
   ┌───────────────┐
   │ Store Reading │
   └───────┬───────┘
           ↓
   ┌───────────────┐
   │ Check Threshold│
   └───────┬───────┘
           ↓
   ┌───────────────┐
   │ Create Alert  │ (if needed)
   └───────────────┘
```

## Future Enhancements

- [ ] Add SignalR broadcasting to connected clients
- [ ] Implement message buffering for offline scenarios
- [ ] Add metrics and monitoring (Prometheus)
- [ ] Support multiple MQTT brokers
- [ ] Add data validation and sanitization
- [ ] Implement dead letter queue for failed messages
