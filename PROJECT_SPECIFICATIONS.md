# 📋 Moondesk - Water Quality Monitoring Platform Specifications

> **Industry Focus**: Water Treatment & Distribution  
> **Purpose**: Real-time water quality monitoring, compliance, and treatment optimization  
> **Target Users**: Water utilities, treatment plant operators, environmental agencies

---

## 🎯 System Overview

**Moondesk** is a specialized Industrial IoT platform for water quality monitoring and management. The system enables water utilities to monitor critical parameters (pH, chlorine, turbidity, pressure, flow) across treatment facilities and distribution networks, ensuring regulatory compliance and public health safety.

### Core Value Proposition
- **Regulatory Compliance**: Automated EPA/WHO threshold monitoring and reporting
- **Public Health Protection**: Real-time contamination detection with immediate alerts
- **Treatment Optimization**: Data-driven chemical dosing and process control
- **Multi-Site Management**: Centralized monitoring of treatment plants and distribution points
- **Predictive Maintenance**: Early detection of equipment failures and pipe breaks

---

## 🏗️ Water Industry Architecture

### Water Treatment Monitoring Flow

```
┌─────────────────────────────────────────────────────────┐
│  Water Treatment Plant (On-Premises)                    │
│  ┌──────────────────────────────────────────┐           │
│  │  Treatment Stages                        │           │
│  │  • Raw Water Intake                      │           │
│  │  • Coagulation/Flocculation             │           │
│  │  • Sedimentation                         │           │
│  │  • Filtration                            │           │
│  │  • Disinfection (Chlorination)          │           │
│  │  • Distribution                          │           │
│  └──────────────┬───────────────────────────┘           │
│                 │                                        │
│  ┌──────────────▼───────────────────────────┐           │
│  │  Water Quality Sensors (Modbus/BACnet)   │           │
│  │  • pH Meters                             │           │
│  │  • Turbidity Sensors                     │           │
│  │  • Chlorine Analyzers                    │           │
│  │  • Flow Meters                           │           │
│  │  • Pressure Transducers                  │           │
│  └──────────────┬───────────────────────────┘           │
│                 │                                        │
│  ┌──────────────▼───────────────────────────┐           │
│  │  Edge Gateway (Raspberry Pi)             │           │
│  │  • Protocol Translation                  │           │
│  │  • Local Data Buffering                  │           │
│  │  • MQTT Publishing                       │           │
│  └──────────────┬───────────────────────────┘           │
└─────────────────┼────────────────────────────────────────┘
                  │ MQTT over TLS
                  │ Internet
                  ▼
┌─────────────────────────────────────────────────────────┐
│  Moondesk Cloud Platform                                │
│  ┌─────────────────────────────┐                        │
│  │  Water Quality API          │                        │
│  │  • Real-time Ingestion      │                        │
│  │  • EPA Compliance Checks    │                        │
│  │  • Treatment Analytics      │                        │
│  │  • Alert Management         │                        │
│  │  • Historical Reporting     │                        │
│  └─────────────────────────────┘                        │
│  ┌─────────────────────────────┐                        │
│  │  TimescaleDB                │                        │
│  │  • Water Quality Time-Series│                        │
│  │  • Compliance Audit Logs    │                        │
│  └─────────────────────────────┘                        │
└─────────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│  Operator Dashboard                                     │
│  • Treatment Process Visualization                      │
│  • Real-time Parameter Monitoring                       │
│  • Compliance Reports                                   │
│  • Alert Management                                     │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 Project 1: Moondesk.Domain

### Water Quality Domain Models

#### 1. **WaterAsset** (Treatment Equipment)
- **Types**: 
  - Treatment Plant
  - Pump Station
  - Reservoir/Tank
  - Distribution Point
  - Monitoring Station
- **Properties**: 
  - Capacity (MGD - Million Gallons per Day)
  - Treatment Processes
  - Service Population
  - GPS Coordinates
- **Business Logic**: 
  - Calculate treatment efficiency
  - Track chemical inventory
  - Monitor energy consumption

#### 2. **WaterQualitySensor**
- **Chemical Sensors**:
  - pH Meter (6.5-8.5 range, EPA compliance)
  - Free Chlorine Analyzer (0.2-4.0 mg/L)
  - Total Chlorine Analyzer
  - Fluoride Sensor
  - Dissolved Oxygen (DO)
  
- **Physical Sensors**:
  - Turbidity Meter (0-5 NTU for drinking water)
  - Temperature Sensor
  - Conductivity Meter
  - Total Dissolved Solids (TDS)
  
- **Hydraulic Sensors**:
  - Flow Meter (GPM/MGD)
  - Pressure Transducer (PSI)
  - Level Sensor (feet/meters)

- **Compliance Thresholds**:
  - EPA Primary Standards (health-based)
  - EPA Secondary Standards (aesthetic)
  - WHO Guidelines
  - State-specific regulations

#### 3. **WaterQualityReading**
- **Extended Properties**:
  - Compliance Status (Pass/Fail/Warning)
  - Treatment Stage (Raw/Filtered/Finished)
  - Sample Location
  - Lab Verification Flag
- **Validation Logic**:
  - Range validation per EPA standards
  - Cross-parameter validation (e.g., pH affects chlorine efficacy)
  - Data quality indicators

#### 4. **ComplianceAlert**
- **Severity Levels**:
  - **Advisory**: Approaching threshold (90% of limit)
  - **Warning**: Exceeded secondary standard
  - **Violation**: Exceeded primary standard (public notification required)
  - **Emergency**: Immediate health risk (boil water advisory)
  
- **Alert Types**:
  - High Turbidity (>5 NTU)
  - Low Chlorine Residual (<0.2 mg/L)
  - pH Out of Range (<6.5 or >8.5)
  - High Bacteria Risk (low chlorine + high temperature)
  - Pressure Loss (potential pipe break)
  
- **Workflow**:
  - Auto-notification to operators
  - Escalation to supervisors if unacknowledged
  - Regulatory reporting integration

---

## 📦 Project 2: Moondesk.DataAccess

### Water-Specific Data Optimization

#### 1. **TimescaleDB Hypertables**
- **water_quality_readings**: Partitioned by time (1-hour chunks)
- **compliance_events**: Audit trail for regulatory reporting
- **treatment_batches**: Track chemical dosing per batch

#### 2. **Continuous Aggregates**
- **hourly_water_quality**: Pre-computed averages for dashboards
- **daily_compliance_summary**: EPA reporting format
- **treatment_efficiency**: Chemical usage vs. water quality outcomes

#### 3. **Data Retention Policies**
- Raw readings: 90 days (EPA minimum)
- Hourly aggregates: 3 years
- Compliance violations: 10 years (regulatory requirement)
- Audit logs: Indefinite

#### 4. **Water Industry Queries**
- Chlorine residual trends across distribution network
- Turbidity breakthrough detection during filtration
- Chemical dosing correlation with raw water quality
- Pressure zone monitoring for leak detection

---

## 📦 Project 3: Moondesk.API

### Water Quality Endpoints

#### **WaterAssetsController** (`/api/water/assets`)
- `GET /api/water/assets/treatment-plants`: List all treatment facilities
- `GET /api/water/assets/{id}/treatment-process`: Get current treatment stage data
- `GET /api/water/assets/{id}/efficiency`: Calculate treatment efficiency metrics
- `POST /api/water/assets/{id}/chemical-dose`: Log chemical dosing event

#### **WaterQualityController** (`/api/water/quality`)
- `GET /api/water/quality/compliance-status`: Current compliance across all sites
- `GET /api/water/quality/parameters`: Real-time water quality dashboard data
- `GET /api/water/quality/trends`: Historical trends for specific parameters
- `POST /api/water/quality/lab-verification`: Submit lab test results

#### **ComplianceController** (`/api/water/compliance`)
- `GET /api/water/compliance/violations`: List EPA violations
- `GET /api/water/compliance/reports/monthly`: Generate monthly compliance report
- `POST /api/water/compliance/alerts/{id}/acknowledge`: Operator acknowledgment
- `GET /api/water/compliance/audit-trail`: Regulatory audit log

#### **TreatmentController** (`/api/water/treatment`)
- `GET /api/water/treatment/chemical-inventory`: Current chemical stock levels
- `POST /api/water/treatment/optimize-dosing`: AI-driven dosing recommendations
- `GET /api/water/treatment/energy-usage`: Energy consumption per treatment stage

### Water-Specific Background Services

#### **ComplianceMonitoringService**
- Continuously evaluate readings against EPA/WHO thresholds
- Generate compliance reports
- Auto-escalate violations to regulatory portals

#### **TreatmentOptimizationService**
- Analyze chemical dosing effectiveness
- Recommend dosing adjustments based on raw water quality
- Track cost savings from optimization

#### **PredictiveMaintenanceService**
- Detect sensor drift (calibration needed)
- Identify pump performance degradation
- Predict filter breakthrough based on turbidity trends

---

## 📦 Project 4: Moondesk.Edge.Simulator

### Water Treatment Simulation

#### Realistic Water Quality Patterns
- **pH**: Stable baseline with gradual drift (simulates chemical dosing)
- **Turbidity**: Spikes during rain events, gradual decline through filtration
- **Chlorine**: Decay over time in distribution network
- **Flow Rate**: Diurnal patterns (high morning/evening, low overnight)
- **Pressure**: Drops during high-demand periods

#### Treatment Process Simulation
```json
{
  "treatmentPlant": {
    "stages": [
      {
        "name": "Raw Water Intake",
        "sensors": ["turbidity", "temperature", "pH"]
      },
      {
        "name": "Post-Filtration",
        "sensors": ["turbidity", "flow"]
      },
      {
        "name": "Post-Chlorination",
        "sensors": ["freeChlorine", "pH"]
      },
      {
        "name": "Distribution Entry",
        "sensors": ["pressure", "flow", "chlorine"]
      }
    ]
  }
}
```

---

## 🎯 Water Industry Use Cases

### Municipal Water Utility
- Monitor 5 treatment plants + 20 distribution points
- Track chlorine residual across 100-mile distribution network
- Automated EPA compliance reporting
- Public notification system for violations

### Industrial Water Treatment
- Cooling tower water quality monitoring
- Boiler feedwater chemistry control
- Wastewater discharge compliance
- Process water contamination detection

### Environmental Monitoring
- River/lake water quality stations
- Groundwater monitoring wells
- Stormwater runoff analysis
- Aquaculture dissolved oxygen tracking

---

## 📊 Success Metrics

- **Compliance**: 100% EPA violation detection within 2 seconds
- **Uptime**: 99.9% sensor data availability
- **Response Time**: <500ms from sensor reading to dashboard update
- **Data Retention**: 90 days raw + 3 years aggregated
- **Alert Accuracy**: <1% false positive rate for compliance alerts

---

**Document Version**: 2.0 - Water Quality Focus  
**Last Updated**: 2025-11-20  
**Target Framework**: .NET 10.0

### Core Value Proposition
- **Real-time Monitoring**: Live sensor data streaming with sub-second latency
- **Multi-tenant Architecture**: Complete data isolation per organization using Clerk
- **Industrial Scale**: TimescaleDB for efficient time-series data storage
- **Bi-directional Communication**: Monitor sensors AND control devices remotely
- **Smart Alerting**: Threshold-based alerts with acknowledgment workflow

---

## 🏗️ Protocol-Agnostic Architecture

### Overview
Moondesk is designed as a **protocol-agnostic IoT platform** using the **Strategy Pattern** to support multiple industrial communication protocols. This architecture enables seamless integration with:

**Cloud-Native Protocols** (Direct device-to-cloud):
- **MQTT**: Lightweight pub/sub for IoT devices
- **OPC UA**: Industrial automation standard with built-in security
- **HTTP/REST**: Simple polling for basic devices

**Edge-Only Protocols** (Via on-premises gateway):
- **Modbus TCP/RTU**: Legacy industrial sensors and PLCs
- **BACnet**: Building automation systems
- **Custom protocols**: Extensible through adapter pattern

### Two-Tier Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Customer Site (On-Premises)                            │
│  ┌──────────────┐                                       │
│  │ Modbus/BACnet│  Local Protocol                       │
│  │   Devices    │  ───────────────┐                     │
│  └──────────────┘                 │                     │
│                                   ▼                     │
│                        ┌─────────────────────┐          │
│                        │  Edge Gateway       │          │
│                        │  (Raspberry Pi)     │          │
│                        │  • Protocol Reader  │          │
│                        │  • Translator       │          │
│                        │  • Cloud Publisher  │          │
│                        └──────────┬──────────┘          │
└───────────────────────────────────┼─────────────────────┘
                                    │ MQTT/OPC UA (TLS)
                                    │ Internet
                                    ▼
┌─────────────────────────────────────────────────────────┐
│  Cloud (VPS)                                            │
│  ┌─────────────────────────────┐                        │
│  │  Moondesk.API               │                        │
│  │  • Protocol Adapters        │                        │
│  │    - MQTT Adapter           │                        │
│  │    - OPC UA Adapter         │                        │
│  │    - HTTP Adapter           │                        │
│  │  • Unified Ingestion        │                        │
│  │  • TimescaleDB Storage      │                        │
│  │  • SignalR Broadcasting     │                        │
│  └─────────────────────────────┘                        │
└─────────────────────────────────────────────────────────┘
```

### Strategy Pattern Implementation

All protocol implementations conform to the `IProtocolAdapter` interface, enabling:
- **Runtime protocol selection** per device
- **Zero-code protocol additions** (just implement interface)
- **Protocol-agnostic business logic** (same code for all protocols)
- **Independent testing** of each protocol

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
