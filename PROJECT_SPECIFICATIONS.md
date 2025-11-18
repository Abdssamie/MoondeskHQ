# 📋 Moondesk Project Specifications

> **Role**: Product Manager & Software Designer  
> **Purpose**: Comprehensive project specifications for the Moondesk IoT monitoring platform  
> **Audience**: Developers implementing the system

---

## 🎯 System Overview

**Moondesk** is a multi-tenant Industrial IoT monitoring platform designed for real-time sensor data visualization, alerting, and device control. The system enables organizations to monitor industrial assets (pumps, tanks, compressors, etc.) through connected sensors, receive real-time telemetry, and respond to critical events.

### Core Value Proposition
- **Real-time Monitoring**: Live sensor data streaming with sub-second latency
- **Multi-tenant Architecture**: Complete data isolation per organization using Clerk
- **Industrial Scale**: TimescaleDB for efficient time-series data storage
- **Bi-directional Communication**: Monitor sensors AND control devices remotely
- **Smart Alerting**: Threshold-based alerts with acknowledgment workflow

---

## 📦 Project 1: Moondesk.Domain

### Description
The **Domain** project is the heart of the business logic layer. It contains all domain models, business entities, interfaces, and core business rules. This is a **pure .NET class library** with no external dependencies on infrastructure concerns (databases, APIs, messaging, etc.).

### Responsibilities
- Define all business entities (Asset, Sensor, Reading, Alert)
- Declare repository interfaces for data access abstraction
- Contain business validation logic
- Define enumerations for sensor types, alert severity, asset status
- Provide unit conversion utilities for international support

### Key Logic & Features

#### 1. **Entity Models**
- **Asset**: Represents physical equipment being monitored
  - Properties: Name, Type, Location, Status, LastSeen
  - Relationships: One-to-Many with Sensors
  - Business Logic: Status calculation based on sensor health
  
- **Sensor**: Represents a measurement device attached to an asset
  - Properties: Type, Unit, Thresholds (Low/High), Sampling Interval
  - Relationships: Many-to-One with Asset, One-to-Many with Readings and Alerts
  - Business Logic: Threshold validation, active/inactive state management
  
- **Reading**: Time-series data point from a sensor
  - Properties: Timestamp, Value, Quality indicator
  - Optimized for: High-volume inserts, time-based queries
  - Business Logic: Quality assessment (Good, Uncertain, Bad, Simulated)
  
- **Alert**: Notification when sensor exceeds thresholds
  - Properties: Severity, Message, TriggerValue, Acknowledgment status
  - Workflow: Created → Acknowledged → Resolved
  - Business Logic: Acknowledgment tracking with user and timestamp

#### 2. **Repository Interfaces**
```
IAssetRepository: CRUD operations for assets
ISensorRepository: CRUD operations for sensors + threshold queries
IDataStreamService: Real-time data streaming abstraction
```

#### 3. **Unit Conversion System**
- Localized unit display (Celsius/Fahrenheit, PSI/Bar, etc.)
- Resource files for multi-language support (en, fr-FR)
- Culture-aware formatting

### Technical Details
- **Target Framework**: .NET 8.0
- **Dependencies**: None (pure domain layer)
- **Design Pattern**: Domain-Driven Design (DDD)
- **Validation**: Data annotations for basic validation

---

## 📦 Project 2: Moondesk.DataAccess

### Description
The **DataAccess** project is the infrastructure layer responsible for all database interactions. It implements the repository interfaces defined in the Domain layer using Entity Framework Core and connects to a TimescaleDB (PostgreSQL) instance.

### Responsibilities
- Implement repository interfaces from Domain layer
- Configure Entity Framework Core DbContext
- Define database schema through EF migrations
- Optimize queries for time-series data
- Handle database connection management
- Implement Row-Level Security (RLS) for multi-tenancy

### Key Logic & Features

#### 1. **DbContext Configuration**
- **MoondeskDbContext**: Main database context
  - Configure entity relationships (one-to-many, foreign keys)
  - Set up indexes for performance (timestamp, sensor_id, organization_id)
  - Configure TimescaleDB hypertables for the Readings table
  - Apply global query filters for organization isolation

#### 2. **Repository Implementations**
- **AssetRepository**: 
  - CRUD operations with organization filtering
  - Include related sensors in queries
  - Track LastSeen timestamp updates
  
- **SensorRepository**:
  - CRUD with asset relationship management
  - Query sensors by threshold violations
  - Bulk operations for device provisioning
  
- **ReadingRepository**:
  - High-performance bulk inserts (batching)
  - Time-range queries with aggregation (AVG, MIN, MAX)
  - Automatic data retention policies (delete old data)

#### 3. **Multi-Tenancy Implementation**
- Add `OrganizationId` (from Clerk) to all entities
- Global query filter: `.Where(e => e.OrganizationId == currentOrgId)`
- Prevent cross-organization data leaks
- Index on OrganizationId for query performance

#### 4. **TimescaleDB Integration**
- Configure Readings table as a hypertable partitioned by time
- Set up continuous aggregates for dashboard queries
- Implement data retention policies (e.g., keep raw data for 90 days)
- Use TimescaleDB functions for time-bucket aggregations

### Technical Details
- **Target Framework**: .NET 8.0
- **Key NuGet Packages**:
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (PostgreSQL provider)
  - `Microsoft.EntityFrameworkCore` (ORM)
  - `Microsoft.EntityFrameworkCore.Design` (migrations)
- **Connection String**: Configured in appsettings.json
- **Migrations**: Code-first approach with EF migrations

---

## 📦 Project 3: Moondesk.API

### Description
The **API** project is the central backend service that exposes REST endpoints, handles real-time WebSocket connections via SignalR, ingests MQTT telemetry data, and orchestrates all business logic. This is an ASP.NET Core Web API application.

### Responsibilities
- Expose RESTful API endpoints for frontend consumption
- Authenticate and authorize requests using Clerk JWT tokens
- Implement SignalR hubs for real-time data push to clients
- Run background service to consume MQTT messages from EMQX Cloud
- Persist incoming telemetry to TimescaleDB
- Broadcast new readings to connected SignalR clients
- Handle device command publishing to MQTT
- Execute threshold-based alerting logic

### Key Logic & Features

#### 1. **REST API Controllers**

**DevicesController** (`/api/devices`)
- `GET /api/devices`: List all assets for the authenticated organization
- `GET /api/devices/{id}`: Get single asset with sensors
- `POST /api/devices`: Register a new asset
- `PUT /api/devices/{id}`: Update asset metadata
- `DELETE /api/devices/{id}`: Remove asset
- `POST /api/devices/{id}/command`: Send control command to device

**SensorsController** (`/api/sensors`)
- `GET /api/sensors`: List all sensors (filterable by asset)
- `GET /api/sensors/{id}`: Get sensor details
- `POST /api/sensors`: Add sensor to an asset
- `PUT /api/sensors/{id}`: Update sensor configuration (thresholds, etc.)
- `DELETE /api/sensors/{id}`: Remove sensor

**ReadingsController** (`/api/readings`)
- `GET /api/readings`: Query time-series data with filters
  - Query params: sensorId, startTime, endTime, aggregation (raw/avg/min/max)
- `GET /api/readings/latest`: Get most recent reading per sensor
- Logic: Use TimescaleDB time-bucket for aggregations

**AlertsController** (`/api/alerts`)
- `GET /api/alerts`: List alerts (filterable by severity, acknowledged status)
- `POST /api/alerts/{id}/acknowledge`: Mark alert as acknowledged
- Logic: Update acknowledgment timestamp and user

#### 2. **Authentication & Authorization**
- **Middleware**: JWT Bearer token validation
- **Clerk Integration**:
  - Validate JWT signature using Clerk public keys
  - Extract `organization_id` from token claims
  - Inject `organization_id` into request context
- **Authorization Policy**: All endpoints require authenticated user
- **Row-Level Security**: Automatically filter all queries by organization

#### 3. **SignalR Real-Time Hub**

**TelemetryHub** (`/hubs/telemetry`)
- **Client Methods**:
  - `ReceiveReading(sensorId, timestamp, value)`: Push new sensor reading
  - `ReceiveAlert(alertData)`: Push critical alerts
  - `DeviceStatusChanged(deviceId, status)`: Notify device online/offline
  
- **Server Methods**:
  - `SubscribeToDevice(deviceId)`: Client subscribes to specific device updates
  - `UnsubscribeFromDevice(deviceId)`: Client unsubscribes
  
- **Logic**:
  - Group connections by organization_id for targeted broadcasting
  - Only send data to clients in the same organization
  - Maintain connection state for reconnection handling

#### 4. **MQTT Ingestion Background Service**

**MqttIngestionService** (BackgroundService)
- **Startup Logic**:
  - Connect to EMQX Cloud using credentials from appsettings
  - Subscribe to wildcard topic: `+/+/telemetry` (org_id/device_id/telemetry)
  
- **Message Handler Logic**:
  1. Parse MQTT topic to extract `organization_id` and `device_id`
  2. Deserialize JSON payload to extract sensor readings
  3. Validate data quality and format
  4. Insert reading into TimescaleDB via ReadingRepository
  5. Check if reading violates sensor thresholds
  6. If threshold violated:
     - Create Alert entity
     - Persist to database
     - Broadcast alert via SignalR to organization's clients
  7. Broadcast reading via SignalR to subscribed clients
  8. Update Asset.LastSeen timestamp

- **Error Handling**:
  - Retry logic for MQTT connection failures
  - Dead-letter queue for malformed messages
  - Logging for debugging

#### 5. **Device Command Publishing**
- **Endpoint**: `POST /api/devices/{id}/command`
- **Request Body**: `{ "command": "TURN_ON", "parameters": {...} }`
- **Logic**:
  1. Validate user has permission for the device
  2. Construct MQTT command message
  3. Publish to device-specific topic: `{org_id}/{device_id}/cmd`
  4. Log command for audit trail
  5. Return acknowledgment to client

#### 6. **Alerting Logic**
- **Trigger**: Executed in MQTT message handler after each reading insert
- **Algorithm**:
  ```
  if (reading.Value > sensor.ThresholdHigh || reading.Value < sensor.ThresholdLow):
      severity = determine_severity(reading.Value, thresholds)
      alert = new Alert(severity, message, triggerValue)
      save_alert(alert)
      broadcast_alert_via_signalr(alert)
  ```
- **Severity Determination**:
  - Warning: 10% over threshold
  - Critical: 25% over threshold
  - Emergency: 50% over threshold

### Technical Details
- **Target Framework**: .NET 8.0
- **Key NuGet Packages**:
  - `Microsoft.AspNetCore.SignalR` (real-time WebSockets)
  - `MQTTnet` (MQTT client library)
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (Clerk JWT validation)
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (via DataAccess reference)
- **Hosting**: Docker container on VPS
- **Environment Variables**:
  - `CLERK_AUTHORITY`: Clerk JWT issuer URL
  - `EMQX_HOST`, `EMQX_PORT`, `EMQX_USERNAME`, `EMQX_PASSWORD`
  - `DATABASE_CONNECTION_STRING`

---

## 📦 Project 4: Moondesk.Edge.Simulator

### Description
The **Edge Simulator** is a Python-based application that mimics an industrial IoT gateway running on edge devices (Raspberry Pi, industrial PCs). It generates realistic sensor telemetry and publishes it to EMQX Cloud via MQTT. It also listens for commands from the cloud.

### Responsibilities
- Simulate realistic sensor data (temperature, pressure, vibration, etc.)
- Publish telemetry to EMQX Cloud at configurable intervals
- Subscribe to command topics and execute actions
- Handle connection failures and reconnection
- Provide configuration for multiple sensors per device

### Key Logic & Features

#### 1. **Sensor Data Simulation**
- **Realistic Patterns**:
  - Temperature: Sine wave with random noise (simulates daily cycles)
  - Pressure: Baseline with occasional spikes (simulates pump cycles)
  - Vibration: Low baseline with random bursts (simulates machinery)
  - Flow Rate: Step function with gradual changes
  
- **Configurable Parameters** (appsettings.json):
  ```json
  {
    "sensors": [
      {
        "id": "sensor_001",
        "type": "Temperature",
        "unit": "°C",
        "baseValue": 25.0,
        "variance": 5.0,
        "samplingIntervalMs": 1000
      }
    ]
  }
  ```

#### 2. **MQTT Publishing Logic**
- **Connection**:
  - Connect to EMQX Cloud using TLS
  - Use device-specific credentials (username/password or client certificate)
  
- **Publishing Loop**:
  ```python
  while True:
      for sensor in sensors:
          value = generate_sensor_value(sensor)
          payload = {
              "sensorId": sensor.id,
              "timestamp": datetime.utcnow().isoformat(),
              "value": value,
              "quality": "Good"
          }
          topic = f"{org_id}/{device_id}/telemetry"
          client.publish(topic, json.dumps(payload))
          sleep(sensor.samplingIntervalMs / 1000)
  ```

#### 3. **Command Listener**
- **Subscribe to**: `{org_id}/{device_id}/cmd`
- **Supported Commands**:
  - `TURN_ON`: Activate sensor data generation
  - `TURN_OFF`: Stop sensor data generation
  - `SET_INTERVAL`: Change sampling rate
  - `CALIBRATE`: Reset baseline values
  
- **Command Handler**:
  ```python
  def on_message(client, userdata, message):
      command = json.loads(message.payload)
      if command["action"] == "TURN_ON":
          start_sensors()
      elif command["action"] == "SET_INTERVAL":
          update_interval(command["sensorId"], command["intervalMs"])
  ```

#### 4. **Error Handling & Resilience**
- Automatic reconnection on network failure
- Exponential backoff for connection retries
- Local buffering of messages during disconnection
- Logging to file for debugging

### Technical Details
- **Language**: Python 3.9+
- **Key Libraries**:
  - `paho-mqtt`: MQTT client
  - `numpy`: For realistic data generation
  - `python-dotenv`: Configuration management
- **Deployment**: Runs as systemd service on Raspberry Pi
- **Configuration**: Environment variables or config file

---

## 🔄 System Integration Flow

### Data Ingestion Flow
```
Edge Device (Simulator) 
  → MQTT Publish to EMQX Cloud 
  → Moondesk.API (MqttIngestionService) 
  → Moondesk.DataAccess (Repository) 
  → TimescaleDB 
  → SignalR Broadcast 
  → Next.js Dashboard
```

### Command Flow
```
Next.js Dashboard 
  → REST API (POST /devices/{id}/command) 
  → Moondesk.API 
  → MQTT Publish to EMQX Cloud 
  → Edge Device (Simulator) 
  → Execute Action
```

### Alert Flow
```
Sensor Reading Ingested 
  → Threshold Check in API 
  → Alert Created in DB 
  → SignalR Broadcast (Critical Channel) 
  → Dashboard Notification 
  → User Acknowledges via REST API
```

---

## 🎨 Design Patterns & Principles

### Architecture Patterns
- **Clean Architecture**: Domain → DataAccess → API (dependency inversion)
- **Repository Pattern**: Abstract data access behind interfaces
- **CQRS Lite**: Separate read/write optimizations for time-series data
- **Pub/Sub**: MQTT for decoupled device communication

### SOLID Principles
- **Single Responsibility**: Each project has one clear purpose
- **Open/Closed**: Extend via interfaces (IRepository, IDataStreamService)
- **Liskov Substitution**: Repository implementations are interchangeable
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Domain doesn't depend on infrastructure

### Multi-Tenancy Strategy
- **Data Isolation**: OrganizationId on all entities
- **Query Filtering**: Global EF Core filters
- **Authentication**: Clerk JWT with organization claims
- **Authorization**: Row-level security in repositories

---

## 📊 Data Model Summary

```
Organization (Clerk - External)
  ↓ 1:N
Asset (Equipment being monitored)
  ↓ 1:N
Sensor (Measurement device)
  ↓ 1:N
Reading (Time-series data) ← TimescaleDB Hypertable
  
Sensor
  ↓ 1:N
Alert (Threshold violations)
```

---

## 🚀 Deployment Architecture

### Development Environment
- **Database**: Docker container (TimescaleDB)
- **API**: Local Kestrel server (dotnet run)
- **Simulator**: Python script on local machine
- **Frontend**: Next.js dev server (npm run dev)

### Production Environment
- **Database**: Docker container on VPS
- **API**: Docker container on VPS (behind Nginx reverse proxy)
- **MQTT Broker**: EMQX Cloud (managed service)
- **Authentication**: Clerk Cloud (managed service)
- **Frontend**: Vercel (serverless)
- **Edge Devices**: Raspberry Pi with systemd service

---

## 📝 Developer Notes

### For Moondesk.Domain
- Keep this layer pure - no external dependencies
- All business rules belong here
- Use value objects for complex types (e.g., ThresholdRange)

### For Moondesk.DataAccess
- Always use async/await for database operations
- Implement connection pooling for performance
- Use EF migrations for schema changes
- Test queries with realistic data volumes

### For Moondesk.API
- Validate all inputs at controller level
- Use DTOs for API contracts (don't expose domain entities directly)
- Implement rate limiting for public endpoints
- Log all MQTT messages for debugging
- Use health checks for monitoring

### For Moondesk.Edge.Simulator
- Make simulation patterns configurable
- Include device identification in all messages
- Implement graceful shutdown (flush buffers)
- Support multiple sensor profiles

---

## 🎯 Success Metrics

- **Latency**: < 500ms from sensor reading to dashboard update
- **Throughput**: Support 1000+ sensors per organization
- **Reliability**: 99.9% uptime for API
- **Data Retention**: 90 days of raw data, 1 year of aggregated data
- **Alert Response**: < 2 seconds from threshold violation to notification

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-18  
**Author**: Product Manager & Software Designer (AI Assistant)
