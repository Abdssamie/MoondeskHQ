#!/bin/bash

# Ensure logs directory exists
mkdir -p logs

# Check for Org ID argument
if [ -z "$1" ]; then
  echo "⚠️  WARNING: No Organization ID provided."
  echo "   Usage: ./start-dev.sh <clerk_org_id>"
  echo "   The Simulator will run with a default ID."
  SIM_ARGS=""
else
  ORG_ID=$1
  echo "🔑 Using Organization ID: $ORG_ID"
  SIM_ARGS="--orgId=$ORG_ID"
fi

echo "🚀 Starting Moondesk Development Environment..."

# Start Infrastructure
echo "📦 Starting Docker containers (TimescaleDB + EMQX)..."
docker-compose -f containers/docker-compose.yml up -d

echo "⏳ Waiting 10s for services to be ready..."
sleep 10

# Start API
echo "🌐 Starting API Host..."
dotnet run --project Moondesk.Host --environment Development > logs/api.log 2>&1 &
API_PID=$!
echo "✅ API started (PID $API_PID). Logs: logs/api.log"

# Start Background Worker
echo "⚙️ Starting Background Worker..."
dotnet run --project Moondesk.BackgroundServices --environment Development > logs/worker.log 2>&1 &
WORKER_PID=$!
echo "✅ Worker started (PID $WORKER_PID). Logs: logs/worker.log"

# Start Simulator
echo "📡 Starting Edge Simulator..."
# Pass SIM_ARGS which may contain --orgId
dotnet run --project Moondesk.Edge.Simulator -- $SIM_ARGS > logs/simulator.log 2>&1 &
SIM_PID=$!
echo "✅ Simulator started (PID $SIM_PID). Logs: logs/simulator.log"

echo ""
echo "🎉 Environment is running!"
echo "   - API: http://localhost:5008 (or see logs)"
echo "   - EMQX Dashboard: http://localhost:18083 (admin/public)"
echo "   - MQTT Broker: localhost:1883"
echo ""
echo "To simulate a provisioned device with specific credentials, run manually:"
echo "export ORG_ID=$ORG_ID"
echo "export MQTT_USERNAME=your_device_user"
echo "export MQTT_PASSWORD=your_device_pass"
echo "dotnet run --project Moondesk.Edge.Simulator"
echo ""
echo "Press [ENTER] to stop all services and exit."

read

echo "🛑 Stopping services..."
kill $API_PID
kill $WORKER_PID
kill $SIM_PID
echo "🐳 Stopping containers..."
docker-compose -f containers/docker-compose.yml down
echo "Bye! 👋"
