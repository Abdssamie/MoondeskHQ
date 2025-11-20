# Implementation Plan: Water Quality Parameter Types

## Objective
Add water quality parameter classification to sensors and readings to enable specialized data processing, validation rules, and compliance monitoring.

## Current State Analysis

### Existing Structure
- **Sensor.Type**: Generic `SensorType` enum (Temperature, Pressure, PH, etc.)
- **Sensor.Unit**: String field (e.g., "°C", "PSI", "mg/L")
- **No parameter categorization**: Cannot distinguish between water quality vs. hydraulic vs. environmental sensors
- **No EPA threshold mapping**: Thresholds are manual, not linked to regulatory standards

## Proposed Solution

### Step 1: Create Water Quality Parameter Enum ✅
**File**: `Moondesk.Domain/Enums/WaterQualityParameter.cs`

```csharp
namespace Moondesk.Domain.Enums;

/// <summary>
/// Water quality parameters for specialized monitoring and compliance
/// </summary>
public enum WaterQualityParameter
{
    // Not a water quality parameter
    None = 0,
    
    // Chemical Parameters
    pH = 1,
    FreeChlorine = 2,
    TotalChlorine = 3,
    Fluoride = 4,
    DissolvedOxygen = 5,
    
    // Physical Parameters
    Turbidity = 10,
    Temperature = 11,
    Conductivity = 12,
    TotalDissolvedSolids = 13,
    
    // Hydraulic Parameters
    FlowRate = 20,
    Pressure = 21,
    Level = 22
}
```

### Step 2: Add Parameter to Sensor Model ✅
**File**: `Moondesk.Domain/Models/IoT/Sensor.cs`

**Changes**:
```csharp
// Add new property after Type
public WaterQualityParameter Parameter { get; set; } = WaterQualityParameter.None;
```

### Step 3: Add Parameter to Reading Model ✅
**File**: `Moondesk.Domain/Models/IoT/Reading.cs`

**Changes**:
```csharp
// Add new property after Value
public WaterQualityParameter Parameter { get; set; } = WaterQualityParameter.None;
```

**Rationale**: Readings should store parameter for efficient querying without joining to Sensor table

### Step 4: Create EPA Compliance Configuration ✅
**File**: `Moondesk.Domain/Configuration/EpaStandards.cs`

```csharp
namespace Moondesk.Domain.Configuration;

public static class EpaStandards
{
    public static readonly Dictionary<WaterQualityParameter, EpaThreshold> Thresholds = new()
    {
        { WaterQualityParameter.pH, new EpaThreshold(6.5, 8.5, "pH units") },
        { WaterQualityParameter.Turbidity, new EpaThreshold(0, 0.3, "NTU") },
        { WaterQualityParameter.FreeChlorine, new EpaThreshold(0.2, 4.0, "mg/L") },
        { WaterQualityParameter.TotalChlorine, new EpaThreshold(0.2, 4.0, "mg/L") },
        { WaterQualityParameter.Fluoride, new EpaThreshold(0.7, 4.0, "mg/L") },
        { WaterQualityParameter.Turbidity, new EpaThreshold(0, 0.3, "NTU") },
        { WaterQualityParameter.TotalDissolvedSolids, new EpaThreshold(0, 500, "mg/L") }
    };
}

public record EpaThreshold(double Min, double Max, string Unit);
```

### Step 5: Database Migration ✅
**Command**: 
```bash
dotnet ef migrations add AddParameterToSensorAndReading --project Moondesk.DataAccess
```

**Migration will**:
- Add `Parameter` column to Sensors table (int, default 0)
- Add `Parameter` column to Readings table (int, default 0)
- Non-nullable integer
- No data loss

### Step 6: Update Alert Logic ✅
**File**: `Moondesk.API/Services/AlertService.cs` (if exists) or inline in ingestion

**Changes**:
```csharp
// Check if sensor has water parameter
if (sensor.Parameter != WaterQualityParameter.None && 
    EpaStandards.Thresholds.ContainsKey(sensor.Parameter))
{
    var epaThreshold = EpaStandards.Thresholds[sensor.Parameter];
    if (reading.Value < epaThreshold.Min || reading.Value > epaThreshold.Max)
    {
        // Create EPA compliance alert
    }
}
else if (sensor.ThresholdLow.HasValue || sensor.ThresholdHigh.HasValue)
{
    // Use manual thresholds
}
```

### Step 7: Update API Controllers ✅
**File**: `Moondesk.API/Controllers/SensorsController.cs`

**Changes**:
- Add `Parameter` to sensor DTOs
- Return EPA thresholds in sensor details endpoint

### Step 8: Update Seed Data ✅
**File**: `Moondesk.DataAccess/Data/SeedData.cs` (if exists)

**Changes**:
- Update example sensors with water parameters
- Create sample water treatment plant sensors

### Step 9: Update Edge Simulator ✅
**File**: `Moondesk.Edge.Simulator/SimulateSensorData.cs`

**Changes**:
- Add Parameter field to simulated sensor data
- Generate readings with matching parameter

## Implementation Status

### ✅ Phase 1: Foundation (COMPLETED)
1. ✅ Created `Parameter` enum in `Moondesk.Domain/Enums/Parameter.cs`
2. ✅ Added `Parameter` property to Sensor model (defaults to None)
3. ✅ Added `Parameter` property to Reading model (defaults to None)
4. ✅ Created `EpaStandards` configuration in `Moondesk.Domain/Configuration/EpaStandards.cs`
5. ✅ Generated database migration: `AddParameterToSensorAndReading`
6. ⏳ Apply migration: `dotnet ef database update --project Moondesk.DataAccess`

### ⏳ Phase 2: Integration (PENDING)
7. ⏳ Update alert logic to use EPA thresholds
8. ⏳ Update sensor DTOs with parameter
9. ⏳ Update controllers
10. ⏳ Update seed data
11. ⏳ Update edge simulator

### ⏳ Phase 3: Testing (PENDING)
12. ⏳ Test sensor creation with parameter
13. ⏳ Test reading ingestion with parameter
14. ⏳ Test EPA threshold alerts
15. ⏳ Validate backward compatibility

## Implementation Order

### Phase 1: Foundation (No Breaking Changes)
1. ✅ Create `WaterQualityParameter` enum
2. ✅ Add `Parameter` property to Sensor (default None)
3. ✅ Add `Parameter` property to Reading (default None)
4. ✅ Create `EpaStandards` configuration
5. ✅ Generate and apply database migration
6. ✅ Test migration on development database

### Phase 2: Integration
7. ✅ Update alert logic to use EPA thresholds
8. ✅ Update sensor DTOs with parameter
9. ✅ Update controllers
10. ✅ Update seed data
11. ✅ Update edge simulator

### Phase 3: Testing
12. ✅ Test sensor creation with parameter
13. ✅ Test reading ingestion with parameter
14. ✅ Test EPA threshold alerts
15. ✅ Validate backward compatibility

## Rollback Plan

### If Issues Occur
1. **Database**: `dotnet ef database update <previous-migration>`
2. **Code**: Property defaults to None, existing code unaffected
3. **API**: DTOs backward compatible (optional field)

### Safety Checks
- ✅ Default value prevents null issues
- ✅ Existing sensors work without parameter
- ✅ Manual thresholds still respected
- ✅ No data loss in migration

## Benefits After Implementation

### 1. Automated EPA Compliance
```csharp
if (sensor.Parameter != WaterQualityParameter.None)
{
    var epaThreshold = EpaStandards.Thresholds[sensor.Parameter];
    if (reading.Value > epaThreshold.Max) { /* EPA violation */ }
}
```

### 2. Efficient Querying
```csharp
// Query readings by parameter without joining Sensor table
var chlorineReadings = await _context.Readings
    .Where(r => r.Parameter == WaterQualityParameter.FreeChlorine)
    .ToListAsync();
```

### 3. Compliance Reporting
```csharp
var waterSensors = sensors.Where(s => s.Parameter != WaterQualityParameter.None);
var complianceReport = GenerateEpaReport(waterSensors);
```

## Files to Create/Modify

### New Files (2)
1. `Moondesk.Domain/Enums/WaterQualityParameter.cs`
2. `Moondesk.Domain/Configuration/EpaStandards.cs`

### Modified Files (5)
1. `Moondesk.Domain/Models/IoT/Sensor.cs` - Add Parameter property
2. `Moondesk.Domain/Models/IoT/Reading.cs` - Add Parameter property
3. `Moondesk.DataAccess/Data/MoondeskDbContext.cs` - Configure properties
4. `Moondesk.API/Controllers/SensorsController.cs` - DTO updates (if exists)
5. `Moondesk.Edge.Simulator/SimulateSensorData.cs` - Add parameter field

### Database Changes (1)
1. New migration: `AddParameterToSensorAndReading`

## Estimated Effort
- **Phase 1**: 1-2 hours (foundation + migration)
- **Phase 2**: 1-2 hours (integration)
- **Phase 3**: 1 hour (testing)
- **Total**: 3-5 hours

## Success Criteria
✅ Existing sensors continue to work without changes
✅ Existing readings continue to work without changes
✅ New sensors can specify water quality parameters
✅ Readings store parameter for efficient querying
✅ EPA thresholds automatically applied to water sensors
✅ All tests pass
✅ No data loss in migration
✅ API backward compatible

## Next Steps
1. Review and approve this plan
2. Create feature branch: `feature/water-quality-parameters`
3. Implement Phase 1 (foundation)
4. Test migration on dev database
5. Proceed with Phase 2 and 3
6. Merge to main after testing

