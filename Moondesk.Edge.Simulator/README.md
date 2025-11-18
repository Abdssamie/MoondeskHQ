# 🔌 Moondesk.Edge.Simulator

## Overview
The **Edge Simulator** is a Python-based application that mimics an industrial IoT gateway. It generates realistic sensor telemetry and publishes it to EMQX Cloud via MQTT. It also listens for commands from the cloud to simulate bi-directional communication.

## Purpose
- Simulate realistic industrial sensor data (temperature, pressure, vibration, etc.)
- Publish telemetry to EMQX Cloud at configurable intervals
- Subscribe to command topics and execute actions
- Handle connection failures with automatic reconnection
- Support multiple sensors per device

---

## Architecture

```
┌─────────────────────────────────┐
│  Moondesk.Edge.Simulator        │
│  ┌───────────────────────────┐  │
│  │  Sensor Simulators        │  │
│  │  - Temperature            │  │
│  │  - Pressure               │  │
│  │  - Vibration              │  │
│  │  - Flow Rate              │  │
│  └───────────────────────────┘  │
│              │                   │
│              ▼                   │
│  ┌───────────────────────────┐  │
│  │  MQTT Publisher           │  │
│  │  (paho-mqtt)              │  │
│  └───────────────────────────┘  │
│              │                   │
│              ▼                   │
│  ┌───────────────────────────┐  │
│  │  Command Listener         │  │
│  │  (MQTT Subscriber)        │  │
│  └───────────────────────────┘  │
└──────────────┬──────────────────┘
               │
               ▼
      ┌────────────────┐
      │  EMQX Cloud    │
      └────────────────┘
```

---

## Key Components

### 1. **Sensor Data Simulation**

#### **Realistic Patterns**

Each sensor type has a unique simulation pattern:

**Temperature Sensor:**
```python
def simulate_temperature(base_value=25.0, variance=5.0, time_offset=0):
    """Simulates daily temperature cycles with random noise"""
    # Sine wave for daily cycle
    daily_cycle = base_value + (variance * math.sin(time_offset / 3600))
    # Add random noise
    noise = random.gauss(0, variance * 0.1)
    return round(daily_cycle + noise, 2)
```

**Pressure Sensor:**
```python
def simulate_pressure(base_value=100.0, variance=10.0):
    """Simulates pump pressure with occasional spikes"""
    if random.random() < 0.05:  # 5% chance of spike
        return round(base_value + variance * 2, 2)
    return round(base_value + random.gauss(0, variance * 0.2), 2)
```

**Vibration Sensor:**
```python
def simulate_vibration(base_value=0.5, variance=0.2):
    """Simulates machinery vibration with random bursts"""
    if random.random() < 0.1:  # 10% chance of burst
        return round(base_value + variance * 5, 2)
    return round(base_value + random.gauss(0, variance), 2)
```

**Flow Rate Sensor:**
```python
def simulate_flow_rate(base_value=50.0, variance=5.0):
    """Simulates flow rate with gradual changes"""
    # Step function with gradual transitions
    step = random.choice([-1, 0, 0, 0, 1])  # Mostly stable
    return round(base_value + step * variance, 2)
```

---

### 2. **Configuration**

#### **appsettings.json**
```json
{
  "EMQX": {
    "Host": "broker.emqx.io",
    "Port": 8883,
    "Username": "your_device_username",
    "Password": "your_device_password",
    "UseTLS": true
  },
  "Device": {
    "OrganizationId": "org_2abc123",
    "DeviceId": "device_001",
    "Name": "Hydraulic Pump #1"
  },
  "Sensors": [
    {
      "Id": 1,
      "Name": "Bearing Temperature",
      "Type": "Temperature",
      "Unit": "°C",
      "BaseValue": 25.0,
      "Variance": 5.0,
      "SamplingIntervalMs": 1000,
      "ThresholdLow": 10.0,
      "ThresholdHigh": 80.0
    },
    {
      "Id": 2,
      "Name": "Hydraulic Pressure",
      "Type": "Pressure",
      "Unit": "PSI",
      "BaseValue": 100.0,
      "Variance": 10.0,
      "SamplingIntervalMs": 2000,
      "ThresholdLow": 50.0,
      "ThresholdHigh": 150.0
    },
    {
      "Id": 3,
      "Name": "Motor Vibration",
      "Type": "Vibration",
      "Unit": "mm/s",
      "BaseValue": 0.5,
      "Variance": 0.2,
      "SamplingIntervalMs": 500,
      "ThresholdLow": 0.0,
      "ThresholdHigh": 2.0
    }
  ]
}
```

---

### 3. **MQTT Publishing Logic**

#### **Main Publishing Loop**

```python
import paho.mqtt.client as mqtt
import json
import time
from datetime import datetime

class EdgeSimulator:
    def __init__(self, config):
        self.config = config
        self.client = mqtt.Client()
        self.sensors = config['Sensors']
        self.running = False
        
    def connect(self):
        """Connect to EMQX Cloud"""
        self.client.username_pw_set(
            self.config['EMQX']['Username'],
            self.config['EMQX']['Password']
        )
        
        if self.config['EMQX']['UseTLS']:
            self.client.tls_set()
        
        self.client.on_connect = self.on_connect
        self.client.on_message = self.on_message
        self.client.on_disconnect = self.on_disconnect
        
        self.client.connect(
            self.config['EMQX']['Host'],
            self.config['EMQX']['Port'],
            keepalive=60
        )
        
        self.client.loop_start()
    
    def on_connect(self, client, userdata, flags, rc):
        """Callback when connected"""
        if rc == 0:
            print(f"✅ Connected to EMQX Cloud")
            # Subscribe to command topic
            org_id = self.config['Device']['OrganizationId']
            device_id = self.config['Device']['DeviceId']
            cmd_topic = f"{org_id}/{device_id}/cmd"
            self.client.subscribe(cmd_topic)
            print(f"📡 Subscribed to {cmd_topic}")
        else:
            print(f"❌ Connection failed with code {rc}")
    
    def publish_telemetry(self, sensor, value):
        """Publish sensor reading to MQTT"""
        org_id = self.config['Device']['OrganizationId']
        device_id = self.config['Device']['DeviceId']
        topic = f"{org_id}/{device_id}/telemetry"
        
        payload = {
            "sensorId": sensor['Id'],
            "timestamp": datetime.utcnow().isoformat() + "Z",
            "value": value,
            "quality": "Good"
        }
        
        self.client.publish(topic, json.dumps(payload), qos=1)
        print(f"📤 Published: {sensor['Name']} = {value} {sensor['Unit']}")
    
    def run(self):
        """Main simulation loop"""
        self.running = True
        time_offset = 0
        
        while self.running:
            for sensor in self.sensors:
                # Generate sensor value based on type
                if sensor['Type'] == 'Temperature':
                    value = simulate_temperature(
                        sensor['BaseValue'], 
                        sensor['Variance'], 
                        time_offset
                    )
                elif sensor['Type'] == 'Pressure':
                    value = simulate_pressure(
                        sensor['BaseValue'], 
                        sensor['Variance']
                    )
                elif sensor['Type'] == 'Vibration':
                    value = simulate_vibration(
                        sensor['BaseValue'], 
                        sensor['Variance']
                    )
                else:
                    value = sensor['BaseValue']
                
                # Publish to MQTT
                self.publish_telemetry(sensor, value)
                
                # Sleep for sampling interval
                time.sleep(sensor['SamplingIntervalMs'] / 1000)
            
            time_offset += 1
    
    def stop(self):
        """Stop simulation"""
        self.running = False
        self.client.loop_stop()
        self.client.disconnect()
```

---

### 4. **Command Listener**

#### **Handle Commands from Cloud**

```python
def on_message(self, client, userdata, message):
    """Callback when command received"""
    try:
        command = json.loads(message.payload.decode())
        action = command.get('action')
        
        print(f"📥 Received command: {action}")
        
        if action == 'TURN_ON':
            self.start_sensors()
        elif action == 'TURN_OFF':
            self.stop_sensors()
        elif action == 'SET_INTERVAL':
            sensor_id = command.get('sensorId')
            interval = command.get('intervalMs')
            self.update_interval(sensor_id, interval)
        elif action == 'CALIBRATE':
            sensor_id = command.get('sensorId')
            self.calibrate_sensor(sensor_id)
        else:
            print(f"⚠️ Unknown command: {action}")
    
    except Exception as e:
        print(f"❌ Error processing command: {e}")

def start_sensors(self):
    """Start sensor data generation"""
    print("▶️ Starting sensors...")
    self.running = True

def stop_sensors(self):
    """Stop sensor data generation"""
    print("⏸️ Stopping sensors...")
    self.running = False

def update_interval(self, sensor_id, interval_ms):
    """Update sampling interval for a sensor"""
    for sensor in self.sensors:
        if sensor['Id'] == sensor_id:
            sensor['SamplingIntervalMs'] = interval_ms
            print(f"⏱️ Updated interval for {sensor['Name']}: {interval_ms}ms")
            break

def calibrate_sensor(self, sensor_id):
    """Reset sensor baseline values"""
    for sensor in self.sensors:
        if sensor['Id'] == sensor_id:
            # Reset to configured baseline
            print(f"🔧 Calibrated {sensor['Name']}")
            break
```

---

### 5. **Error Handling & Resilience**

#### **Automatic Reconnection**

```python
def on_disconnect(self, client, userdata, rc):
    """Callback when disconnected"""
    if rc != 0:
        print(f"⚠️ Unexpected disconnection. Reconnecting...")
        retry_count = 0
        max_retries = 10
        
        while retry_count < max_retries:
            try:
                time.sleep(2 ** retry_count)  # Exponential backoff
                self.client.reconnect()
                print("✅ Reconnected successfully")
                break
            except Exception as e:
                retry_count += 1
                print(f"❌ Reconnection attempt {retry_count} failed: {e}")
        
        if retry_count == max_retries:
            print("❌ Max retries reached. Exiting.")
            self.stop()
```

#### **Local Message Buffering**

```python
class BufferedPublisher:
    def __init__(self, client):
        self.client = client
        self.buffer = []
        self.max_buffer_size = 1000
    
    def publish(self, topic, payload, qos=1):
        """Publish with buffering on failure"""
        try:
            result = self.client.publish(topic, payload, qos)
            if result.rc != mqtt.MQTT_ERR_SUCCESS:
                self.buffer_message(topic, payload)
        except Exception as e:
            print(f"❌ Publish failed: {e}")
            self.buffer_message(topic, payload)
    
    def buffer_message(self, topic, payload):
        """Store message for retry"""
        if len(self.buffer) < self.max_buffer_size:
            self.buffer.append({'topic': topic, 'payload': payload})
            print(f"💾 Buffered message (buffer size: {len(self.buffer)})")
    
    def flush_buffer(self):
        """Retry sending buffered messages"""
        while self.buffer:
            msg = self.buffer.pop(0)
            self.client.publish(msg['topic'], msg['payload'])
```

---

## Installation

### Requirements
```bash
pip install paho-mqtt numpy python-dotenv
```

### Dependencies (requirements.txt)
```
paho-mqtt==1.6.1
numpy==1.24.0
python-dotenv==1.0.0
```

---

## Running the Simulator

### Local Development
```bash
cd Moondesk.Edge.Simulator
python Program.py
```

### As a Service (Raspberry Pi)

Create systemd service file: `/etc/systemd/system/moondesk-simulator.service`
```ini
[Unit]
Description=Moondesk Edge Simulator
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/moondesk-simulator
ExecStart=/usr/bin/python3 /home/pi/moondesk-simulator/Program.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable moondesk-simulator
sudo systemctl start moondesk-simulator
sudo systemctl status moondesk-simulator
```

---

## Configuration Examples

### Multiple Devices
Run multiple instances with different config files:
```bash
python Program.py --config device1.json
python Program.py --config device2.json
```

### Environment Variables
Use `.env` file for sensitive data:
```
EMQX_USERNAME=device_001
EMQX_PASSWORD=your_secure_password
ORG_ID=org_2abc123
DEVICE_ID=device_001
```

---

## Testing

### Test MQTT Connection
```bash
# Subscribe to telemetry topic
mosquitto_sub -h broker.emqx.io -p 8883 -t "org_2abc123/device_001/telemetry" --cafile ca.crt -u username -P password

# Publish test command
mosquitto_pub -h broker.emqx.io -p 8883 -t "org_2abc123/device_001/cmd" -m '{"action":"TURN_ON"}' --cafile ca.crt -u username -P password
```

---

## Developer Notes

### Adding New Sensor Types
1. Add simulation function in `SimulateSensorData.cs`
2. Update configuration schema
3. Add case in `run()` method

### Customizing Simulation Patterns
- Modify variance for more/less noise
- Adjust spike probability for pressure sensors
- Change sine wave frequency for temperature cycles

### Debugging
Enable verbose logging:
```python
import logging
logging.basicConfig(level=logging.DEBUG)
```

---

**Project Type**: Python Application  
**Python Version**: 3.9+  
**Dependencies**: paho-mqtt, numpy, python-dotenv
