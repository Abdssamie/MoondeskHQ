# Phase 0: Architecture & Setup - Implementation Guide
## Industrial IoT Monitoring & Alerting Dashboard

**Duration:** Weeks 1-2  
**Status:** 🚧 In Progress  
**Target Completion:** [Set your date]

---

## Overview

This document outlines the detailed implementation plan for Phase 0 of the AquaPP Industrial IoT Dashboard MVP. Phase 0 establishes the core architecture, tooling, and development environment that will support all subsequent phases.

### Phase 0 Goals

✅ Establish robust solution architecture with clear separation of concerns  
✅ Implement MVVM pattern with CommunityToolkit.Mvvm  
✅ Set up Dark/Light theme system  
✅ Create mock/simulated data services for IoT sensor data  
✅ Configure dependency injection for scalability  
✅ Prepare for real-time data streaming (SignalR/gRPC)

---

## Current State Analysis

### ✅ Already Implemented
- **.NET 9.0** with Avalonia 11.3.8
- **CommunityToolkit.Mvvm** (8.4.0) for MVVM pattern
- **Microsoft.Extensions.DependencyInjection** configured in `App.axaml.cs`
- **Entity Framework Core** with SQLite for data persistence
- **Serilog** logging infrastructure
- **SukiUI** theme system with Dark/Light mode support
- Basic navigation with `PageNavigationService`
- Existing views: Dashboard, Chat, DataEntry, Settings

### 🔨 To Be Implemented (Phase 0)
1. **IoT-specific domain models** (Asset, Sensor, Reading)
2. **Data streaming service layer** (SignalR client or gRPC)
3. **Mock sensor data simulator** for development
4. **Reactive Extensions (Rx.NET)** for real-time data handling
5. **Asset directory/management infrastructure**
6. **Enhanced theme customization** for industrial UI

---

## Architecture Design

### Solution Structure

```
AquaPP/
├── Core/                          # Domain models & interfaces
│   ├── Models/
│   │   ├── IoT/
│   │   │   ├── Asset.cs          # NEW: Industrial asset entity
│   │   │   ├── Sensor.cs         # NEW: Sensor metadata
│   │   │   ├── Reading.cs        # NEW: Time-series sensor reading
│   │   │   └── Alert.cs          # NEW: Alert/threshold model
│   │   └── ...
│   ├── Interfaces/
│   │   ├── IDataStreamService.cs # NEW: Real-time data abstraction
│   │   ├── IAssetRepository.cs   # NEW: Asset data access
│   │   └── ISensorService.cs     # NEW: Sensor management
│   └── ...
├── Infrastructure/                # NEW: Data access & external services
│   ├── Data/
│   │   ├── Repositories/
│   │   │   ├── AssetRepository.cs
│   │   │   └── SensorRepository.cs
│   │   └── ApplicationDbContext.cs (extend existing)
│   ├── Streaming/
│   │   ├── SignalRDataStreamService.cs
│   │   └── MockDataStreamService.cs  # For Phase 0 development
│   └── Simulators/
│       └── SensorDataSimulator.cs    # Generates realistic test data
├── Services/                      # Application services
│   ├── IoT/
│   │   ├── AssetManagementService.cs
│   │   └── AlertingService.cs
│   └── ...
├── ViewModels/
│   ├── Pages/
│   │   ├── AssetDirectoryViewModel.cs  # NEW
│   │   ├── MonitoringDashboardViewModel.cs  # NEW (rename from Dashboard)
│   │   └── ...
│   └── ...
└── Views/
    ├── Pages/
    │   ├── AssetDirectoryView.axaml    # NEW
    │   ├── MonitoringDashboardView.axaml  # NEW
    │   └── ...
    └── ...
```

---

## Implementation Checklist

### Week 1: Core Architecture & Domain Models

#### Task 1.1: Create IoT Domain Models
**Priority:** 🔴 Critical  
**Estimated Time:** 4 hours

- [ ] Create `Core/Models/IoT/` directory
- [ ] Implement `Asset.cs` (Id, Name, Type, Location, Status, LastSeen)
- [ ] Implement `Sensor.cs` (Id, AssetId, Type, Unit, ThresholdHigh, ThresholdLow)
- [ ] Implement `Reading.cs` (Id, SensorId, Timestamp, Value, Quality)
- [ ] Implement `Alert.cs` (Id, SensorId, Timestamp, Severity, Message, Acknowledged)
- [ ] Add EF Core configurations for new entities

#### Task 1.2: Extend Database Context
**Priority:** 🔴 Critical  
**Estimated Time:** 2 hours

- [ ] Add DbSets for Asset, Sensor, Reading, Alert to `ApplicationDbContext`
- [ ] Create EF Core migration: `dotnet ef migrations add AddIoTEntities`
- [ ] Test migration: `dotnet ef database update`

#### Task 1.3: Create Core Interfaces
**Priority:** 🔴 Critical  
**Estimated Time:** 3 hours

- [ ] Create `Core/Interfaces/IDataStreamService.cs`
  - Methods: `IObservable<Reading> StreamReadings(int sensorId)`
  - Methods: `Task<IEnumerable<Reading>> GetHistoricalReadings(int sensorId, DateTimeOffset from, DateTimeOffset to)`
- [ ] Create `Core/Interfaces/IAssetRepository.cs`
  - Standard CRUD operations
- [ ] Create `Core/Interfaces/ISensorService.cs`
  - Sensor configuration and management

#### Task 1.4: Install Required NuGet Packages
**Priority:** 🔴 Critical  
**Estimated Time:** 1 hour

```bash
# Add System.Reactive for Rx.NET
dotnet add package System.Reactive --version 6.0.1

# Add SignalR Client (for future real-time streaming)
dotnet add package Microsoft.AspNetCore.SignalR.Client --version 9.0.0

# Optional: Add ScottPlot for charting (Phase 2, but can install now)
dotnet add package ScottPlot.Avalonia --version 5.0.47
```

- [ ] Add System.Reactive
- [ ] Add Microsoft.AspNetCore.SignalR.Client
- [ ] (Optional) Add ScottPlot.Avalonia for future charting

### Week 2: Mock Services & UI Shell

#### Task 2.1: Implement Mock Data Simulator
**Priority:** 🟡 High  
**Estimated Time:** 6 hours

- [ ] Create `Infrastructure/Simulators/SensorDataSimulator.cs`
  - Generate realistic sensor data (temperature, pressure, vibration, flow rate)
  - Use `System.Reactive` to emit readings at configurable intervals
  - Support multiple sensor types with different characteristics
- [ ] Create `Infrastructure/Streaming/MockDataStreamService.cs`
  - Implement `IDataStreamService` using the simulator
  - Provide 5-10 mock sensors for testing

#### Task 2.2: Implement Repository Layer
**Priority:** 🟡 High  
**Estimated Time:** 4 hours

- [ ] Create `Infrastructure/Data/Repositories/AssetRepository.cs`
- [ ] Create `Infrastructure/Data/Repositories/SensorRepository.cs`
- [ ] Implement basic CRUD operations using EF Core
- [ ] Add seed data for 3-5 test assets with sensors

#### Task 2.3: Create Asset Directory View
**Priority:** 🟡 High  
**Estimated Time:** 5 hours

- [ ] Create `ViewModels/Pages/AssetDirectoryViewModel.cs`
  - ObservableCollection of Assets
  - Commands: RefreshAssets, SelectAsset, NavigateToMonitoring
- [ ] Create `Views/Pages/AssetDirectoryView.axaml`
  - DataGrid or TreeView showing assets
  - Display: Name, Type, Status, Last Seen
  - Connection status indicator (Green/Red/Yellow)
- [ ] Register view/viewmodel in `App.axaml.cs`

#### Task 2.4: Update Navigation & Theme
**Priority:** 🟢 Medium  
**Estimated Time:** 3 hours

- [ ] Add "Asset Directory" to main navigation menu
- [ ] Rename existing "Dashboard" to "Monitoring Dashboard" (optional)
- [ ] Verify Dark/Light theme switching works across all new views
- [ ] Add industrial color palette to SukiUI theme (optional enhancement)

#### Task 2.5: Configure Dependency Injection
**Priority:** 🔴 Critical  
**Estimated Time:** 2 hours

- [ ] Register `IDataStreamService` → `MockDataStreamService` in `App.axaml.cs`
- [ ] Register `IAssetRepository` → `AssetRepository`
- [ ] Register `ISensorService` (if implemented)
- [ ] Verify all ViewModels can resolve dependencies

#### Task 2.6: Testing & Documentation
**Priority:** 🟢 Medium  
**Estimated Time:** 3 hours

- [ ] Manual testing: Launch app, navigate to Asset Directory
- [ ] Verify mock data is generated and displayed
- [ ] Test theme switching
- [ ] Document any issues or technical debt
- [ ] Update this document with completion status

---

## Success Criteria (Phase 0 Deliverables)

### M0: Milestone Checklist

✅ **Architecture**
- [ ] Clean separation: Core → Infrastructure → Services → ViewModels → Views
- [ ] All IoT domain models created and migrated to database
- [ ] Dependency injection configured for all new services

✅ **Mock Data System**
- [ ] `MockDataStreamService` generates realistic sensor readings
- [ ] At least 5 different sensor types simulated
- [ ] Data streams using Rx.NET observables

✅ **UI Shell**
- [ ] Asset Directory view displays mock assets
- [ ] Connection status indicators working
- [ ] Dark/Light theme applied consistently
- [ ] Navigation between views functional

✅ **Developer Experience**
- [ ] Application builds without errors
- [ ] Database migrations apply successfully
- [ ] Logging captures key events
- [ ] Code follows MVVM pattern

---

## Technical Decisions & Rationale

### Why System.Reactive (Rx.NET)?
- **Reason:** Industrial IoT generates high-frequency data streams. Rx.NET provides powerful operators (`Throttle`, `Sample`, `Buffer`) to manage backpressure and prevent UI thread overload.
- **Alternative Considered:** Manual threading with `Task.Run()` - rejected due to complexity.

### Why SignalR over gRPC?
- **Reason:** SignalR provides easier setup for bi-directional streaming and has excellent .NET client support. gRPC is more performant but adds complexity.
- **Decision:** Start with SignalR, evaluate gRPC in Phase 4 if performance issues arise.

### Why Mock Data in Phase 0?
- **Reason:** Allows UI/UX development to proceed in parallel with backend API development. Reduces external dependencies during early phases.

---

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| EF Core migration conflicts with existing water quality schema | High | Medium | Use separate DbSets and tables; avoid schema overlap |
| Rx.NET learning curve for team | Medium | High | Provide code examples and pair programming sessions |
| Mock data doesn't reflect real sensor behavior | Medium | Medium | Consult with domain experts; add noise and anomalies to simulator |
| Performance issues with high-frequency data | High | Low | Implement throttling from day 1; profile early |

---

## Next Steps (Phase 1 Preview)

After completing Phase 0, Phase 1 will focus on:
1. **Real SignalR integration** with a .NET backend API
2. **Historical data persistence** for sensor readings
3. **Basic data visualization** (line charts for 1-2 metrics)

---

## Notes & Decisions Log

| Date | Decision | Rationale |
|------|----------|-----------|
| [Today] | Use existing SukiUI theme system | Already integrated; provides professional Dark/Light themes |
| [Today] | Keep water quality features alongside IoT features | Allows gradual migration; both domains can coexist |
| | | |

---

**Document Owner:** Development Team  
**Last Updated:** [Today's Date]  
**Next Review:** End of Week 1
