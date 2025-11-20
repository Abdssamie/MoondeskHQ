# 🚀 Moondesk.API - Water Quality Monitoring Backend

## Overview
The **API** project is the central backend service for water quality monitoring. It exposes REST endpoints for water treatment data, handles real-time WebSocket connections for live sensor updates, ingests MQTT telemetry from water quality sensors, and manages EPA compliance alerting.

## Purpose
- Expose RESTful API for water quality dashboards
- Authenticate/authorize requests using Clerk JWT tokens
- Provide real-time water quality data push via SignalR
- Ingest MQTT telemetry from treatment plants and distribution networks
- Persist water quality readings to TimescaleDB
- Execute EPA/WHO compliance threshold alerting
- Publish control commands to treatment equipment

---

## Architecture

```
┌─────────────────────────────────────┐
│  Water Treatment Plant              │
│  pH, Chlorine, Turbidity Sensors    │
└────────┬────────────────────────────┘
         │ MQTT
         ▼
┌─────────────────────────────────────┐
│      Moondesk.API                   │
│  ┌──────────────────────────────┐   │
│  │  Water Quality Controllers   │   │
│  │  - WaterAssetsController     │   │
│  │  - WaterQualityController    │   │
│  │  - ComplianceController      │   │
│  │  - TreatmentController       │   │
│  └──────────────────────────────┘   │
│                                      │
│  ┌──────────────────────────────┐   │
│  │  SignalR Hub                 │   │
│  │  - WaterQualityHub           │   │
│  └──────────────────────────────┘   │
│                                      │
│  ┌──────────────────────────────┐   │
│  │  Background Services         │   │
│  │  - ComplianceMonitoring      │   │
│  │  - TreatmentOptimization     │   │
│  └──────────────────────────────┘   │
└─────────┬────────────────────────────┘
          │
          ▼
┌─────────────────┐      ┌──────────────┐
│  TimescaleDB    │      │  EMQX Cloud  │
│  Water Quality  │      │  MQTT Broker │
│  Time-Series    │      └──────────────┘
└─────────────────┘
```

---

## Key Components

### 1. **REST API Controllers**

#### **DevicesController** (`/api/devices`)

**Endpoints:**
```csharp
GET    /api/devices              // List all assets for org
GET    /api/devices/{id}         // Get single asset with sensors
POST   /api/devices              // Register new asset
PUT    /api/devices/{id}         // Update asset metadata
DELETE /api/devices/{id}         // Remove asset
POST   /api/devices/{id}/command // Send control command
```

**Implementation Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires Clerk JWT
public class DevicesController : ControllerBase
{
    private readonly IAssetRepository _assetRepository;
    private readonly IMqttClient _mqttClient;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Asset>>> GetDevices()
    {
        // Organization filtering handled by DbContext global filter
        var assets = await _assetRepository.GetAllAsync();
        return Ok(assets);
    }

    [HttpPost("{id}/command")]
    public async Task<IActionResult> SendCommand(int id, [FromBody] DeviceCommand command)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null) return NotFound();

        // Get organization ID from JWT claims
        var orgId = User.FindFirst("org_id")?.Value;
        
        // Publish MQTT command
        var topic = $"{orgId}/{id}/cmd";
        var payload = JsonSerializer.Serialize(command);
        await _mqttClient.PublishAsync(topic, payload);

        return Accepted();
    }
}
```

---

#### **SensorsController** (`/api/sensors`)

**Endpoints:**
```csharp
GET    /api/sensors              // List all sensors (filterable by asset)
GET    /api/sensors/{id}         // Get sensor details
POST   /api/sensors              // Add sensor to asset
PUT    /api/sensors/{id}         // Update sensor config (thresholds)
DELETE /api/sensors/{id}         // Remove sensor
```

---

#### **ReadingsController** (`/api/readings`)

**Endpoints:**
```csharp
GET /api/readings?sensorId=1&start=2024-01-01&end=2024-01-02&aggregation=avg
GET /api/readings/latest
```

**Implementation Example:**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Reading>>> GetReadings(
    [FromQuery] int sensorId,
    [FromQuery] DateTimeOffset start,
    [FromQuery] DateTimeOffset end,
    [FromQuery] string aggregation = "raw")
{
    if (aggregation == "raw")
    {
        var readings = await _readingRepository.GetReadingsAsync(sensorId, start, end);
        return Ok(readings);
    }
    else
    {
        // Use TimescaleDB time_bucket for aggregation
        var aggregated = await _readingRepository.GetAggregatedReadingsAsync(
            sensorId, start, end, interval: "1 hour");
        return Ok(aggregated);
    }
}
```

---

#### **AlertsController** (`/api/alerts`)

**Endpoints:**
```csharp
GET  /api/alerts                    // List alerts (filterable)
POST /api/alerts/{id}/acknowledge   // Mark alert as acknowledged
```

---

### 2. **SignalR Real-Time Hub**

#### **TelemetryHub** (`/hubs/telemetry`)

**Purpose:** Push real-time sensor data and alerts to connected clients.

**Client Methods (Server → Client):**
```csharp
public class TelemetryHub : Hub
{
    // Broadcast new reading to all clients in organization
    public async Task BroadcastReading(string organizationId, Reading reading)
    {
        await Clients.Group(organizationId).SendAsync("ReceiveReading", reading);
    }

    // Broadcast critical alert
    public async Task BroadcastAlert(string organizationId, Alert alert)
    {
        await Clients.Group(organizationId).SendAsync("ReceiveAlert", alert);
    }

    // Notify device status change
    public async Task NotifyDeviceStatus(string organizationId, int deviceId, string status)
    {
        await Clients.Group(organizationId).SendAsync("DeviceStatusChanged", deviceId, status);
    }
}
```

**Server Methods (Client → Server):**
```csharp
public override async Task OnConnectedAsync()
{
    // Add client to their organization group
    var orgId = Context.User?.FindFirst("org_id")?.Value;
    if (!string.IsNullOrEmpty(orgId))
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orgId);
    }
    await base.OnConnectedAsync();
}

public async Task SubscribeToDevice(int deviceId)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceId}");
}
```

**Frontend Usage (Next.js):**
```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://api.moondesk.com/hubs/telemetry", {
        accessTokenFactory: () => getClerkToken()
    })
    .build();

connection.on("ReceiveReading", (reading) => {
    console.log("New reading:", reading);
    updateChart(reading);
});

connection.on("ReceiveAlert", (alert) => {
    showNotification(alert);
});

await connection.start();
```

---

### 3. **MQTT Ingestion Background Service**

#### **MqttIngestionService** (BackgroundService)

**Purpose:** Connect to EMQX Cloud, subscribe to telemetry topics, persist data, and trigger alerts.

**Implementation:**
```csharp
public class MqttIngestionService : BackgroundService
{
    private readonly IMqttClient _mqttClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TelemetryHub> _hubContext;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Connect to EMQX Cloud
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_config["EMQX:Host"], int.Parse(_config["EMQX:Port"]))
            .WithCredentials(_config["EMQX:Username"], _config["EMQX:Password"])
            .WithTls()
            .Build();

        await _mqttClient.ConnectAsync(options, stoppingToken);

        // Subscribe to all telemetry topics
        await _mqttClient.SubscribeAsync("+/+/telemetry", stoppingToken);

        // Handle incoming messages
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            await HandleTelemetryMessage(e.ApplicationMessage);
        };
    }

    private async Task HandleTelemetryMessage(MqttApplicationMessage message)
    {
        // Parse topic: org_id/device_id/telemetry
        var topicParts = message.Topic.Split('/');
        var organizationId = topicParts[0];
        var deviceId = int.Parse(topicParts[1]);

        // Deserialize payload
        var payload = JsonSerializer.Deserialize<TelemetryPayload>(
            Encoding.UTF8.GetString(message.Payload));

        using var scope = _serviceProvider.CreateScope();
        var readingRepo = scope.ServiceProvider.GetRequiredService<IReadingRepository>();
        var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();

        // Create reading
        var reading = new Reading
        {
            SensorId = payload.SensorId,
            Timestamp = payload.Timestamp,
            Value = payload.Value,
            Quality = ReadingQuality.Good
        };

        // Persist to database
        await readingRepo.AddAsync(reading);

        // Check thresholds
        var sensor = await sensorRepo.GetByIdAsync(payload.SensorId);
        if (sensor != null && 
            (reading.Value > sensor.ThresholdHigh || reading.Value < sensor.ThresholdLow))
        {
            // Create alert
            var alert = new Alert
            {
                SensorId = sensor.Id,
                Severity = DetermineSeverity(reading.Value, sensor),
                Message = $"Sensor {sensor.Name} exceeded threshold: {reading.Value} {sensor.Unit}",
                TriggerValue = reading.Value,
                ThresholdValue = reading.Value > sensor.ThresholdHigh 
                    ? sensor.ThresholdHigh 
                    : sensor.ThresholdLow
            };

            await alertRepo.AddAsync(alert);

            // Broadcast alert via SignalR
            await _hubContext.Clients.Group(organizationId)
                .SendAsync("ReceiveAlert", alert);
        }

        // Broadcast reading via SignalR
        await _hubContext.Clients.Group(organizationId)
            .SendAsync("ReceiveReading", reading);

        // Update asset LastSeen
        // ... (implementation)
    }

    private AlertSeverity DetermineSeverity(double value, Sensor sensor)
    {
        var threshold = value > sensor.ThresholdHigh 
            ? sensor.ThresholdHigh.Value 
            : sensor.ThresholdLow.Value;
        
        var percentOver = Math.Abs((value - threshold) / threshold * 100);

        if (percentOver > 50) return AlertSeverity.Emergency;
        if (percentOver > 25) return AlertSeverity.Critical;
        if (percentOver > 10) return AlertSeverity.Warning;
        return AlertSeverity.Info;
    }
}
```

---

### 4. **Authentication & Authorization**

#### **Clerk JWT Validation**

**Program.cs Configuration:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Clerk:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

// Add HttpContextAccessor for accessing JWT claims in DbContext
builder.Services.AddHttpContextAccessor();
```

**Extract Organization ID:**
```csharp
var organizationId = User.FindFirst("org_id")?.Value;
```

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=moondesk;Username=postgres;Password=yourpassword"
  },
  "Clerk": {
    "Authority": "https://clerk.yourapp.com"
  },
  "EMQX": {
    "Host": "broker.emqx.io",
    "Port": "8883",
    "Username": "your_username",
    "Password": "your_password"
  }
}
```

---

## Required NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="MQTTnet" Version="4.3.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

---

## Running the API

### Development
```bash
cd Moondesk.API
dotnet run
```

### Docker
```bash
docker build -t moondesk-api .
docker run -p 5000:8080 moondesk-api
```

---

## API Documentation

Once running, access OpenAPI documentation at:
- **Swagger UI**: `http://localhost:5000/swagger`
- **OpenAPI JSON**: `http://localhost:5000/openapi/v1.json`

---

## Developer Notes

### Adding New Endpoints
1. Create controller in `Controllers/` folder
2. Inherit from `ControllerBase`
3. Add `[ApiController]` and `[Authorize]` attributes
4. Inject repositories via constructor
5. Use async/await for all database operations

### Testing SignalR
Use the SignalR test client:
```bash
npm install -g @microsoft/signalr-client
signalr-client --url http://localhost:5000/hubs/telemetry
```

### Debugging MQTT
Enable verbose logging in appsettings.Development.json:
```json
{
  "Logging": {
    "LogLevel": {
      "MQTTnet": "Debug"
    }
  }
}
```

---

**Project Type**: ASP.NET Core Web API  
**Target Framework**: .NET 8.0  
**Dependencies**: Moondesk.Domain, Moondesk.DataAccess
