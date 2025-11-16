# AquaPP - Industrial IoT Monitoring Dashboard
## 6-Month MVP Development Roadmap

**Project Start Date:** November 15, 2024
**Target MVP Completion:** May 15, 2025
**Development Team:** 1 Developer (Focus on Efficiency)
**Technology Stack:** .NET 9.0, Avalonia UI 11.3.8, Entity Framework Core, SQLite, **gRPC**

---

## 🎯 Project Vision

Build a cross-platform Industrial IoT Monitoring & Alerting Dashboard that provides **ultra-low-latency, real-time visualization** of sensor data using a consolidated gRPC pipeline.

---

## 📊 Phase Overview

| Phase | Duration | Status | Completion |
|-------|----------|--------|------------|
| **Phase 0: Architecture & Setup** | Weeks 1-2 | ✅ Complete | 100% |
| **Phase 1: Data Ingestion & Backend** | Weeks 3-6 | ⏳ Pending | 0% |
| **Phase 2: Minimum Viable Dashboard** | Weeks 7-12 | ⏳ Pending | 0% |
| **Phase 3: Alerting & Configuration** | Weeks 13-16 | ⏳ Pending | 0% |
| **Phase 4: Cross-Platform Hardening** | Weeks 17-22 | ⏳ Pending | 0% |
| **Phase 5: Review & Planning** | Weeks 23-24 | ⏳ Pending | 0% |

---

## 🔧 Phase 0: Architecture & Setup (Weeks 1-2)

**(No changes needed here, but removing SignalR packages from the summary):**

### 📦 Phase 0 Deliverables
- [x] ...
- [x] Mock data simulator with 10 sensor types
- [x] Asset Directory view with real-time status
- [x] Database seeding with 5 assets and 13 sensors
- [x] Fully functional application (assuming testing complete)

---

## 🚀 Phase 1: Data Ingestion & Backend Core (Weeks 3-6)

**Goal:** Implement **secure, high-performance gRPC data ingress** and persistence.
**Status:** ⏳ Pending
**Target Start:** Week 3

### Planned Features

#### Week 3-4: Backend API Development (Consolidated Project)
- [ ] **ASP.NET Core gRPC Service (AquaPP.Api)**
  - [ ] Create separate API project (AquaPP.Api)
  - [ ] **Add Protobuf definitions** (`.proto` file) for sensor messages and streaming service.
  - [ ] Implement **MQTT Client** to **Subscribe** to raw sensor topics (the **Ingestion Gateway** role).
  - [ ] Implement **gRPC Server** (e.g., `DataService`) to handle two methods:
    - [ ] `StoreReading` (Unary RPC) - Used internally by the MQTT Subscriber logic to save data.
    - [ ] `StreamReadings` (**Server Streaming RPC**) - Used by the **Avalonia Desktop Client**.
  - [ ] Add authentication/authorization (JWT or API keys)
  - [ ] Configure CORS/gRPC-Web for necessary endpoints.

- [ ] **Real-Time Broadcast Logic**
  - [ ] Implement a **Singleton Data Observable/Rx.NET Subject** in the backend service.
  - [ ] When a new MQTT message arrives, it is saved, and then **published** to the Observable.
  - [ ] The `StreamReadings` gRPC method subscribes to this Observable to push data to the client in real-time.

- [ ] **Data Persistence**
  - [ ] Historical reading storage strategy (EF Core/SQLite).
  - [ ] Optimize database indexes for time-series queries.

#### Week 5-6: Client Integration
- [ ] **gRPC Data Stream Service**
  - [ ] Replace `MockDataStreamService` with `GrpcDataStreamService`.
  - [ ] **Implement gRPC Client** in Avalonia project.
  - [ ] Call the `StreamReadings` method once to establish the connection.
  - [ ] Implement `await foreach` loop to consume the stream from the server.
  - [ ] Handle connection state changes and automatic reconnection logic.

- [ ] **Historical Data Service**
  - [ ] Implement `AssetReadingRepository` for unary gRPC calls to fetch historical data from the backend.
  - [ ] Time-range queries for historical data.

- [ ] **Testing**
  - [ ] Integration tests for gRPC streaming/storage.
  - [ ] Load testing with simulated sensors over the full pipeline.

### Phase 1 Deliverables
- [ ] Working ASP.NET Core gRPC API (consolidated backend).
- [ ] **Low-latency gRPC Server Streaming** from API to client.
- [ ] MQTT Ingestion Gateway logic implemented inside the API.
- [ ] Historical data storage and retrieval.
- [ ] Authentication and security implemented.

---

**(Remaining phases 2-5 remain unchanged as they focus on the UI and Alerting logic)**

---

## 📋 Technical Stack Summary

### Frontend
- **Framework:** Avalonia UI 11.3.8
- **Charting:** ScottPlot.Avalonia
- **Reactive:** System.Reactive 6.0.1
- **Real-Time Client:** **Grpc.Net.Client** (replacing SignalR Client)

### Backend
- **Runtime:** .NET 9.0
- **Database:** SQLite (dev), PostgreSQL/SQL Server (production option)
- **ORM:** Entity Framework Core 9.0.11
- **Real-Time Server:** **Grpc.AspNetCore**
- **Messaging Client:** MQTTnet (to be added)
- **Logging:** Serilog 4.3.0

### Development Tools
- **Code Generation:** **Grpc.Tools** (for Protobuf generation)
- **IDE:** JetBrains Rider / Visual Studio
- **Version Control:** Git

---

Choosing gRPC for your C# desktop client provides the performance you need for real-time visualization while simplifying your technology stack compared to introducing SignalR. [SignalR vs gRPC on ASP.NET Core](https://www.youtube.com/watch?v=KXJgP3A3FRY) provides a helpful comparison of these real-time technologies in the .NET ecosystem.

