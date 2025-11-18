That's a strong start. To make your project not only technically complete but also deployable and marketable for the niche job market you're targeting, you need to break down your **Phase 1** goals into the four distinct, deployable **microservice projects** we discussed, fully incorporating **Kafka** alongside your existing **gRPC** plan.

The solution will now contain **six** primary C# projects: your **Client UI** (`AquaPP`), the **Contracts** project, plus **four** new backend/edge projects.

---

## 🗺️ Final Project Roadmap: Complete Deployable Architecture

This roadmap updates **Phase 1** to incorporate the **M-K-T-G (Mosquitto, Kafka, TimescaleDB, Grafana)** stack, shifting away from a monolithic "Backend API" to **decoupled Worker Services** which is better practice for microservices.

---

## 🔧 Phase 0: Architecture & Setup (Weeks 1-2)

**(Completed - Establishing the Foundation)**

### 📦 Phase 0 Deliverables
- [x] Solution structure created with `AquaPP` and `AquaPP.Contracts`.
- [x] Mock data simulator (`SensorDataSimulator.cs`) integrated into **`AquaPP`**.
- [x] Database context (`ApplicationDbContext.cs`) and SQLite persistence set up.
- [x] Initial UI views (Asset Directory, Alert Management, Dashboard) designed.

---

## 🚀 Phase 1: Data Pipeline Implementation (Weeks 3-8)

**Goal:** Implement the full **Edge-to-Kafka-to-DB** ingestion pipeline and expose data via **gRPC**. 
**Status:** ⏳ Pending
**Target Start:** Week 3

### New C# Projects to Create:

1.  **`AquaPP.Edge.Simulator`**
2.  **`AquaPP.Control.Relay`**
3.  **`AquaPP.Data.Processor`**
4.  **`AquaPP.Gateway.Grpc`** (Your centralized API service)

### Task Breakdown (8 Weeks)

#### Weeks 3-4: Infrastructure & Ingestion Bridge
- [ ] **Infrastructure Setup:** Write a **`docker-compose.yml`** file to deploy the **Kafka** cluster (with Zookeeper), **TimescaleDB** (PostgreSQL), and **Grafana** onto your VM.
- [ ] **`AquaPP.Edge.Simulator` (Pi Project):**
    - [ ] Create as a **.NET Console App**.
    - [ ] Install **`MQTTnet`** client package.
    - [ ] Replace mock logic: Use `SensorDataSimulator.cs` to publish readings to a **Mosquitto** topic (e.g., `/edge/telemetry`).
    - [ ] Implement an **MQTT Subscriber** to listen for commands (e.g., `/edge/command`).
- [ ] **Data Serialization:** Standardize all data models (`Reading.cs`, etc.) to serialize as **JSON** strings for transmission over Kafka and MQTT.

#### Weeks 5-6: Core Processing & Persistence
- [ ] **`AquaPP.Data.Processor` (Persistence Service):**
    - [ ] Create as a **.NET Worker Service**.
    - [ ] Install **`Confluent.Kafka`** and **`Npgsql.EntityFrameworkCore.PostgreSQL`**.
    - [ ] Implement the **Kafka Consumer** loop, subscribing to the raw telemetry topic.
    - [ ] **Persistence Logic:** Use EF Core to map and save consumed Kafka messages to the **TimescaleDB**.
- [ ] **Database Migration:** Apply initial EF Core migrations to the TimescaleDB instance.

#### Weeks 7-8: gRPC Gateway & Control Loop
- [ ] **`AquaPP.Gateway.Grpc` (API Gateway):**
    - [ ] Create as an **ASP.NET Core gRPC Service**.
    - [ ] Implement Kafka **Producer** for command messages (e.g., `control-commands` topic).
    - [ ] Implement gRPC Server Streaming method (e.g., `StreamReadings`) that **subscribes internally** to the raw telemetry Kafka topic and pushes data to the client.
- [ ] **`AquaPP.Control.Relay` (Control Logic):**
    - [ ] Create as a **.NET Worker Service**.
    - [ ] Implement a **Kafka Consumer** subscribing to the `control-commands` topic.
    - [ ] **Relay Logic:** On consuming a command, use the **`MQTTnet` Producer** to publish the command to the **Mosquitto** topic (`/edge/command`).
- [ ] **Client Integration:** Update your main `AquaPP` client to use the **gRPC Client** for both reading the data stream and sending commands.

---

## 📊 Remaining Phases (Summarized)

| Phase | Target Duration | Key Goal | C# Projects Involved |
| :--- | :--- | :--- | :--- |
| **Phase 2: Minimum Viable Dashboard** | Weeks 9-14 | **Visualization & Charting:** Update `MonitoringDashboardViewModel` to process the real-time gRPC stream. Implement threshold violation logic in a centralized C# service and display real-time alerts. | `AquaPP`, `AquaPP.Gateway.Grpc` |
| **Phase 3: Alerting & Control** | Weeks 15-20 | **Closed-Loop Control:** Implement the full **Control Loop UI** in the client. Add **Grafana** visualization for historical trend analysis. Define alert triggers in C# based on business logic and publish those alerts back to a Kafka topic. | `AquaPP`, `AquaPP.Data.Processor`, `AquaPP.Control.Relay` |
| **Phase 4: Hardening & Final Polish**| Weeks 21-24 | **Security & Deployment:** Secure all external ports (VM Firewall). Implement robust error handling, logging (using Serilog across all services), and finalized Docker production configuration. | All 6 projects |
