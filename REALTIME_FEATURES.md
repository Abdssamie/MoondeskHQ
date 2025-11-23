# Real-time Features Implementation

## Overview

Implemented MQTT data ingestion, SignalR real-time updates, and intelligent alert system for water quality monitoring.

## 1. MQTT Integration

### Architecture
```
IoT Device → MQTT Broker → MqttIngestionService → Database → SignalR → Frontend
```

### Topic Structure
```
{organization_id}/{device_id}/telemetry
```

Example: `org_abc123/pump_001/telemetry`

### Payload Format
```json
{
  "sensorId": 123,
  "timestamp": "2025-11-23T05:00:00Z",
  "value": 7.2,
  "quality": "Good"
}
```

### Configuration
Add to `appsettings.json`:
```json
{
  "MQTT": {
    "Host": "broker.emqx.io",
    "Port": "1883",
    "Username": "your_username",
    "Password": "your_password"
  }
}
```

### Features
- ✅ Automatic reconnection
- ✅ Wildcard topic subscription (`+/+/telemetry`)
- ✅ Bulk insert for performance
- ✅ Quality tracking (Good/Bad/Uncertain)
- ✅ Protocol tagging (MQTT/HTTP/OPC-UA)

## 2. SignalR Real-time Updates

### Hub Endpoint
```
wss://your-domain.com/hubs/sensordata
```

### Client Connection (JavaScript)
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://your-domain.com/hubs/sensordata", {
    accessTokenFactory: () => getClerkToken()
  })
  .withAutomaticReconnect()
  .build();

// Receive sensor readings
connection.on("ReceiveReading", (reading) => {
  console.log("New reading:", reading);
  // Update UI
});

// Receive alerts
connection.on("ReceiveAlert", (alert) => {
  console.log("Alert:", alert);
  // Show notification
});

await connection.start();
```

### Events Pushed to Clients

#### ReceiveReading
```json
{
  "sensorId": 123,
  "timestamp": "2025-11-23T05:00:00Z",
  "value": 7.2,
  "quality": "Good"
}
```

#### ReceiveAlert
```json
{
  "id": 456,
  "sensorId": 123,
  "severity": "Critical",
  "message": "pH sensor threshold violation: 9.5 pH",
  "timestamp": "2025-11-23T05:00:00Z",
  "triggerValue": 9.5,
  "thresholdValue": 8.5
}
```

### Organization Isolation
- Clients automatically join their organization group
- Only receive data from their own organization
- Based on JWT `org_id` claim

## 3. Alert System

### Threshold Monitoring
Alerts are automatically created when sensor readings exceed thresholds:

```csharp
if (value > sensor.ThresholdHigh || value < sensor.ThresholdLow)
{
    // Create alert
}
```

### Severity Levels
Determined by percentage over threshold:

| % Over Threshold | Severity |
|-----------------|----------|
| > 50% | Emergency |
| > 25% | Critical |
| > 10% | Warning |
| ≤ 10% | Info |

### Alert Flow
```
1. Sensor reading received via MQTT
2. Check against sensor thresholds
3. Calculate severity
4. Store alert in database
5. Push to SignalR clients
6. Send email/SMS (TODO)
```

### Notification Channels

#### SignalR (Implemented)
- Real-time push to connected clients
- Organization-scoped

#### Email (TODO)
```csharp
await notificationService.SendEmailAsync(
    to: "operator@waterplant.com",
    subject: "Critical Alert: pH Violation",
    body: "Sensor pH-001 exceeded threshold..."
);
```

#### SMS (TODO)
```csharp
await notificationService.SendSmsAsync(
    phoneNumber: "+1234567890",
    message: "CRITICAL: pH sensor violation at Treatment Plant A"
);
```

## Testing

### 1. Test MQTT Ingestion

Using mosquitto_pub:
```bash
mosquitto_pub -h localhost -p 1883 \
  -t "org_test123/device_001/telemetry" \
  -m '{"sensorId":1,"timestamp":"2025-11-23T05:00:00Z","value":7.2,"quality":"Good"}'
```

### 2. Test SignalR Connection

```javascript
// Connect
await connection.start();
console.log("Connected to SignalR");

// Listen for readings
connection.on("ReceiveReading", (reading) => {
  console.log("Reading:", reading);
});

// Listen for alerts
connection.on("ReceiveAlert", (alert) => {
  console.log("Alert:", alert);
});
```

### 3. Simulate Alert

Send reading that exceeds threshold:
```bash
mosquitto_pub -h localhost -p 1883 \
  -t "org_test123/device_001/telemetry" \
  -m '{"sensorId":1,"timestamp":"2025-11-23T05:00:00Z","value":15.0,"quality":"Good"}'
```

## Performance Considerations

### MQTT
- Bulk insert readings for better throughput
- Async message handling
- Connection pooling for database

### SignalR
- Organization-based groups reduce broadcast overhead
- Only connected clients receive updates
- Automatic reconnection on disconnect

### Database
- TimescaleDB hypertables for time-series data
- Indexes on sensor_id and timestamp
- Partitioning by organization_id

## Security

### MQTT
- TLS encryption (configure in production)
- Username/password authentication
- Topic-based ACLs

### SignalR
- JWT authentication required
- Organization isolation via groups
- CORS configured for allowed origins

## Deployment

### MQTT Broker Options
1. **EMQX Cloud** (Recommended)
   - Managed service
   - Auto-scaling
   - Built-in monitoring

2. **Mosquitto** (Self-hosted)
   - Lightweight
   - Docker-ready
   - Free and open-source

3. **AWS IoT Core**
   - Fully managed
   - Integrates with AWS services
   - Pay per message

### Docker Compose
```yaml
version: '3.8'
services:
  mosquitto:
    image: eclipse-mosquitto:latest
    ports:
      - "1883:1883"
      - "9001:9001"
    volumes:
      - ./mosquitto.conf:/mosquitto/config/mosquitto.conf
```

## Monitoring

### Metrics to Track
- MQTT messages received/sec
- SignalR connections count
- Alert creation rate
- Database write latency
- Failed message count

### Logging
All events are logged with Serilog:
- MQTT connection status
- Message processing errors
- Alert creation
- SignalR client connections

## Next Steps

1. **Email Notifications**
   - Integrate SendGrid or AWS SES
   - Template-based emails
   - Configurable recipients per alert severity

2. **SMS Notifications**
   - Integrate Twilio or AWS SNS
   - Rate limiting to prevent spam
   - On-call rotation support

3. **Alert Rules Engine**
   - Complex conditions (e.g., "pH > 8.5 AND turbidity > 5")
   - Time-based rules (e.g., "only during business hours")
   - Escalation policies

4. **Historical Data Retention**
   - Automatic data aggregation
   - Compression for old data
   - Archival to cold storage

5. **Dashboard Enhancements**
   - Real-time charts with live updates
   - Alert history timeline
   - Sensor health indicators
