# 🗺️ Project Roadmap: The "Semester Survival" Architecture

This roadmap reflects the final architecture optimized for a solo developer and a semester deadline. It focuses on high-impact features and eliminates infrastructure complexity by leveraging managed services (Clerk, EMQX Cloud) and consolidating the backend into a single, modular **ASP.NET API** that uses **SignalR** for real-time data streaming.

---

### 🏗️ 1. Final Technology Stack

| Layer | Technology | Deployment | Strategy |
| :--- | :--- | :--- | :--- |
| **Frontend (UI)** | Next.js 14, Recharts | Vercel (or similar) | High Velocity, focus on visualization. |
| **Authentication** | **Clerk** | Managed (Clerk Cloud) | Offloads B2B organization logic (huge time saver). |
| **API Backend** | ASP.NET Core 8 Web API | VPS (Docker Container) | Single source for REST, SignalR, and Ingestion. |
| **Real-time Push** | **SignalR** (WebSockets) | Hosted within the C# API | Ensures a "snappy," low-latency dashboard feel. |
| **Messaging (IoT)**| **EMQX Cloud** (MQTT) | Managed Service | Removes local broker hosting burden. |
| **Database** | PostgreSQL + TimescaleDB | VPS (Docker Container) | Single database for all data (metadata and time-series). |
| **Edge Device** | Python Script (`paho-mqtt`) | Raspberry Pi/Laptop | Simulates the industrial gateway. |

---

### 🏁 Phase 0: Foundation & Cleanup (Weeks 1-2)

**Goal:** Clean the project structure and establish the core infrastructure.

| Task | Detail |
| :--- | :--- |
| **A. Structure Cleanup** | **DELETE** redundant projects (`.Contracts`, `.Control.Relay`, `.Data.Processor`, `.Gateway.Grpc`, `.Ingestion.Broker`). **Rename** `.Core` to **`Moondesk.Domain`**. |
| **B. Project Creation** | Create **`Moondesk.API`** (ASP.NET Core Web API) and **`Moondesk.DataAccess`** (for EF Core configuration). |
| **C. Docker Compose** | Write `docker-compose.yml` to launch **TimescaleDB** only. Ensure it accepts remote connections. |
| **D. Auth Setup (Clerk)** | Configure Clerk for Organizations. Implement JWT validation in **`Moondesk.API`** `Program.cs` to enable access control based on `organization_id`. |
| **E. Data Model** | Finalize models in **`Moondesk.Domain`**. Set up **`Moondesk.DataAccess`** with the EF Core `DbContext` targeting TimescaleDB. |

---

### 🚀 Phase 1: The Data Pipeline (Ingestion & Persistence) (Weeks 3-5)

**Goal:** Achieve reliable data flow from the sensor to the database.

| Task | Detail |
| :--- | :--- |
| **A. Edge Simulator (Python)**| Write the Python script using `paho-mqtt`. Configure it to connect directly to the **EMQX Cloud endpoint** (host, port, credentials). Publish mock telemetry to a structured topic (e.g., `org_clerkid/device_id/telemetry`). |
| **B. MQTT Ingestion Service** | Implement a C# **`BackgroundService`** class within **`Moondesk.API`**. This service connects to EMQX Cloud via `MQTTnet`. It subscribes to a wildcard topic (`+/+/telemetry`) to capture all data. |
| **C. Data Persistence Logic** | In the Ingestion Service's message handler, extract the `organization_id` and `device_id` from the MQTT topic. Insert the payload into TimescaleDB via **`Moondesk.DataAccess`**. |
| **D. API Endpoints (REST)** | Create basic C# API controllers for `Devices` and `Readings`. These endpoints **MUST** enforce Row-Level Security by filtering all queries with the authenticated Clerk `organization_id`. |

**✅ Milestone:** Python script runs, publishes to EMQX Cloud, and data is visible and correctly tagged by `organization_id` in TimescaleDB.

---

### 📊 Phase 2: The Live Dashboard & Snappiness (Weeks 6-9)

**Goal:** Build the dynamic UI and implement real-time push.

| Task | Detail |
| :--- | :--- |
| **A. SignalR Hub Setup** | Configure SignalR in **`Moondesk.API`** (`services.AddSignalR()`). Create the core data Hub for broadcasting telemetry. |
| **B. Real-Time Push** | **Modify the MQTT Ingestion Service (Task 1B)**: After saving data to TimescaleDB, immediately broadcast the new reading to the SignalR Hub, using the organization/device ID to route the message efficiently. |
| **C. Dynamic Dashboard** | Build the core Next.js layout. Implement the Clerk `<OrganizationSwitcher />`. Build a component that uses the SignalR client hook (`@microsoft/signalr`) to listen for incoming data specific to the current Clerk organization. |
| **D. Visualization** | Use Recharts to render live line graphs, updating the chart data array every time a new SignalR message is received. |

**✅ Milestone:** Real-time data streams to the browser. Switching the Clerk Organization instantly changes the displayed data set.

---

### 🎛️ Phase 3: Control & Alerting (Weeks 10-12)

**Goal:** Implement two-way communication and business logic.

| Task | Detail |
| :--- | :--- |
| **A. Device Provisioning UI** | Build the "Add Device" modal in Next.js. The API generates a unique device token, which is displayed to the user for manual configuration on the Pi. |
| **B. Commanding (Control Loop)**| Implement a C# API endpoint (`POST /devices/{id}/command`). This endpoint publishes a command MQTT message (e.g., `TURN_ON`) to the EMQX Cloud command topic (e.g., `/devices/{id}/cmd`). |
| **C. Edge Listener Update** | Update the Python script to subscribe to its command topic and execute local actions, closing the control loop. |
| **D. Basic Alerting Logic** | Implement threshold checks in the C# Ingestion Service. On violating a threshold, save an "Alert" record to the DB and push a critical notification via a dedicated SignalR notification channel. |

---

### 🚢 Phase 4: Deployment & Final Polish (Weeks 13-14)

**Goal:** Secure and deploy the working system for the final presentation.

| Task | Detail |
| :--- | :--- |
| **A. Docker Deployment** | Finalize the Docker image for **`Moondesk.API`** and deploy the `docker-compose.yml` (TimescaleDB + API) to the VPS. |
| **B. Frontend Deployment** | Deploy the Next.js app to Vercel. Configure production environment variables for Clerk and the C# API URL. |
| **C. Final Testing** | Verify that unauthorized users cannot access data belonging to other Clerk organizations. |
