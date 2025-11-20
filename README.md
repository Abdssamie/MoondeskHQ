# Moondesk - Water Quality Monitoring Platform

**Real-time IIoT monitoring for water treatment facilities, distribution networks, and environmental compliance**

Moondesk is an industrial IoT platform purpose-built for water quality management. Monitor pH, turbidity, chlorine levels, flow rates, and pressure across your entire water infrastructure with sub-second latency and intelligent alerting.

![Architecture Diagram](AquaPP/Assets/diagram-export-11-16-2025-9_53_24-PM.png)

## Why Water Quality Monitoring?

Water utilities face critical challenges:
- **Regulatory Compliance**: EPA/WHO standards require continuous monitoring
- **Public Health**: Contamination detection must be immediate
- **Operational Efficiency**: Optimize chemical dosing and energy consumption
- **Infrastructure Aging**: Early detection of pipe failures and equipment degradation

Moondesk addresses these with purpose-built features for water operations.

## Core Capabilities

### 🔬 Water-Specific Sensor Support
- **Chemical Parameters**: pH, chlorine (free/total), fluoride, dissolved oxygen
- **Physical Parameters**: Turbidity, temperature, conductivity, TDS
- **Hydraulic Monitoring**: Flow rate, pressure, level sensors
- **Compliance Reporting**: Automated EPA/WHO threshold alerts

### 📊 Treatment Process Optimization
- Real-time visualization of treatment stages (coagulation, filtration, disinfection)
- Chemical dosing correlation with water quality outcomes
- Energy consumption tracking per treatment process
- Historical trend analysis for process optimization

### 🚨 Intelligent Alerting
- Multi-level thresholds (Warning/Critical/Emergency) per water quality parameter
- Cascade alert logic (e.g., low chlorine → high bacteria risk)
- Acknowledgment workflow with operator notes
- SMS/Email integration for critical violations

### 🌐 Multi-Site Management
- Monitor multiple treatment plants and distribution points
- Organization-level data isolation (multi-tenant architecture)
- Geographic asset mapping for distribution networks
- Remote pump station and reservoir monitoring

## Technical Architecture

### Stack
- **.NET 10**: High-performance backend API
- **TimescaleDB**: Optimized time-series storage for sensor data
- **MQTT/OPC UA**: Industrial protocol support for legacy SCADA integration
- **SignalR**: Real-time WebSocket updates to dashboards
- **Entity Framework Core**: Data persistence and migrations

### Deployment
- **Edge Gateway**: Raspberry Pi for Modbus/BACnet sensor integration
- **Cloud API**: Docker-containerized backend on VPS
- **MQTT Broker**: EMQX Cloud for reliable telemetry ingestion
- **Authentication**: Clerk for secure multi-tenant access

## Development Environment Setup

1.  **Install .NET SDK:**
    Ensure you have the .NET 10.0 SDK installed. Download from: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)

2.  **Install IDE:**
    *   **Visual Studio (Windows):** Install with ".NET desktop development" workload
    *   **JetBrains Rider (Cross-platform):** Recommended for .NET development on Windows, macOS, and Linux

3.  **Clone the Repository:**
    ```bash
    git clone https://github.com/Abdssamie/aqua-plus-plus.git
    cd Moondesk
    ```

4.  **Restore NuGet Packages:**
    ```bash
    dotnet restore
    ```

5.  **Apply Database Migrations:**
    ```bash
    dotnet ef database update --project Moondesk.DataAccess
    ```

## Running the Application

1.  **Build the Solution:**
    ```bash
    dotnet build
    ```

2.  **Run the API:**
    ```bash
    dotnet run --project Moondesk.API
    ```

3.  **Run the Edge Simulator (Optional):**
    ```bash
    dotnet run --project Moondesk.Edge.Simulator
    ```

## Use Cases

### Municipal Water Treatment
- Monitor chlorine residual across distribution network
- Track turbidity through filtration process
- Alert operators to pH deviations before EPA violations
- Optimize coagulant dosing based on raw water quality

### Industrial Water Systems
- Cooling tower water quality monitoring
- Boiler feedwater conductivity tracking
- Wastewater discharge compliance monitoring
- Process water contamination detection

### Environmental Monitoring
- River/lake water quality stations
- Groundwater monitoring wells
- Stormwater runoff analysis
- Aquaculture dissolved oxygen tracking

## License

MIT License - See LICENSE file for details

