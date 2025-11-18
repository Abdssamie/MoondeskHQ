# 🏛️ Moondesk.Domain

## Overview
The **Domain** layer is the core business logic of the Moondesk platform. This is a pure .NET class library containing all domain models, business entities, and interfaces with **zero infrastructure dependencies**.

## Purpose
- Define the business domain model (Assets, Sensors, Readings, Alerts)
- Declare repository interfaces for data access abstraction
- Contain business validation rules and logic
- Provide a stable contract for other layers

## Key Components

### 📊 Domain Models

#### **Asset** (`Models/IoT/Asset.cs`)
Represents physical industrial equipment being monitored.

**Properties:**
- `Id`, `Name`, `Type`, `Location`
- `Status` (Online, Offline, Warning, Critical, Maintenance)
- `LastSeen` - timestamp of last communication
- `Sensors` - collection of attached sensors

**Business Logic:**
- Status is calculated based on sensor health
- LastSeen updates on each telemetry message

---

#### **Sensor** (`Models/IoT/Sensor.cs`)
Represents a measurement device attached to an asset.

**Properties:**
- `Type` (Temperature, Pressure, Vibration, FlowRate, etc.)
- `Unit` (°C, PSI, Hz, L/min)
- `ThresholdLow` / `ThresholdHigh` - alert boundaries
- `SamplingIntervalMs` - how often to read
- `IsActive` - enable/disable sensor

**Business Logic:**
- Threshold validation ensures Low < High
- Active sensors participate in monitoring
- Each sensor belongs to exactly one Asset

---

#### **Reading** (`Models/IoT/Reading.cs`)
A single time-series data point from a sensor.

**Properties:**
- `SensorId`, `Timestamp`, `Value`
- `Quality` (Good, Uncertain, Bad, Simulated)

**Optimization:**
- Designed for high-volume inserts (millions of rows)
- Timestamp indexed for time-range queries
- Long Id for scalability

---

#### **Alert** (`Models/IoT/Alert.cs`)
Notification when a sensor reading exceeds thresholds.

**Properties:**
- `Severity` (Info, Warning, Critical, Emergency)
- `Message`, `TriggerValue`, `ThresholdValue`
- `Acknowledged`, `AcknowledgedAt`, `AcknowledgedBy`

**Workflow:**
1. Alert created when threshold violated
2. Broadcast to dashboard via SignalR
3. User acknowledges alert
4. Alert marked as resolved

---

### 🔌 Interfaces

#### **IAssetRepository**
```csharp
Task<Asset?> GetByIdAsync(int id);
Task<IEnumerable<Asset>> GetAllAsync();
Task<Asset> AddAsync(Asset asset);
Task UpdateAsync(Asset asset);
Task DeleteAsync(int id);
```

#### **ISensorRepository**
```csharp
Task<Sensor?> GetByIdAsync(int id);
Task<IEnumerable<Sensor>> GetByAssetIdAsync(int assetId);
Task<IEnumerable<Sensor>> GetSensorsWithThresholdViolationsAsync();
```

#### **IDataStreamService**
```csharp
Task StreamReadingAsync(Reading reading);
Task StreamAlertAsync(Alert alert);
```

---

### 🌍 Unit Conversion System

Located in `Units/` directory:
- **UnitsCulture.cs** - Culture-aware unit formatting
- **Units.resx** - English resource strings
- **Units.fr-FR.resx** - French translations

**Example Usage:**
```csharp
var tempCelsius = 25.0;
var tempFahrenheit = UnitsCulture.ConvertTemperature(tempCelsius, "F");
// Returns "77°F" in en-US, "77°F" in fr-FR
```

---

## Design Principles

### ✅ Clean Architecture
- **No dependencies** on infrastructure (databases, APIs, messaging)
- **Dependency Inversion**: Other layers depend on Domain, not vice versa
- **Testable**: Pure C# logic, easy to unit test

### ✅ Domain-Driven Design (DDD)
- Rich domain models with behavior
- Entities have identity (Id property)
- Value objects for complex types
- Repository pattern for persistence abstraction

### ✅ SOLID Principles
- **Single Responsibility**: Each model has one clear purpose
- **Open/Closed**: Extend via inheritance (e.g., custom sensor types)
- **Interface Segregation**: Small, focused interfaces

---

## Dependencies
**None** - This is a pure .NET 8.0 class library.

---

## Usage Example

```csharp
// Create a new asset
var pump = new Asset
{
    Name = "Hydraulic Pump #1",
    Type = "Pump",
    Location = "Building A - Floor 2",
    Status = AssetStatus.Online
};

// Add a temperature sensor
var tempSensor = new Sensor
{
    Name = "Bearing Temperature",
    Type = SensorType.Temperature,
    Unit = "°C",
    ThresholdLow = 10.0,
    ThresholdHigh = 80.0,
    SamplingIntervalMs = 1000,
    IsActive = true
};

pump.Sensors.Add(tempSensor);

// Create a reading
var reading = new Reading
{
    SensorId = tempSensor.Id,
    Timestamp = DateTimeOffset.UtcNow,
    Value = 75.5,
    Quality = ReadingQuality.Good
};

// Check if alert needed
if (reading.Value > tempSensor.ThresholdHigh)
{
    var alert = new Alert
    {
        SensorId = tempSensor.Id,
        Severity = AlertSeverity.Warning,
        Message = $"Temperature exceeded threshold: {reading.Value}°C",
        TriggerValue = reading.Value,
        ThresholdValue = tempSensor.ThresholdHigh
    };
}
```

---

## Developer Notes

### When to Modify This Project
- Adding new domain entities (e.g., MaintenanceLog, User)
- Changing business rules (e.g., new alert severity levels)
- Adding new sensor types or units
- Defining new repository interfaces

### What NOT to Put Here
- ❌ Database connection strings
- ❌ API endpoints or controllers
- ❌ MQTT client code
- ❌ SignalR hubs
- ❌ Entity Framework DbContext

Those belong in **Moondesk.DataAccess** or **Moondesk.API**.

---

## Next Steps for Developers

1. **Review the models** - Understand the domain entities
2. **Check the interfaces** - These define contracts for data access
3. **Implement repositories** - Go to `Moondesk.DataAccess` to implement these interfaces
4. **Use in API** - Reference this project from `Moondesk.API` to use domain models

---

**Project Type**: Class Library  
**Target Framework**: .NET 8.0  
**Dependencies**: None
