# Moondesk API Implementation

## Overview
REST API with Clerk authentication, versioning, and snake_case naming for IoT device management.

## API Conventions
- **Versioning**: URL-based (`/api/v1/...`)
- **Naming**: snake_case for JSON properties, query params, and route segments
- **URLs**: Lowercase
- **Default Version**: v1.0

## Authentication
- **Clerk .NET SDK** (`Clerk.BackendAPI`)
- Middleware extracts `UserId` and `OrganizationId` from JWT claims
- All controllers inherit from `BaseApiController` with auth context

---

## API Endpoints

### AssetsController (`/api/v1/assets`)
Manage IoT assets (equipment) within an organization.

- `GET /api/v1/assets` - List all assets for current organization
- `GET /api/v1/assets/{id}` - Get asset by ID
- `POST /api/v1/assets` - Create new asset
  ```json
  {
    "name": "Hydraulic Pump #1",
    "type": "Pump",
    "location": "Building A",
    "description": "Main hydraulic pump"
  }
  ```
- `PUT /api/v1/assets/{id}` - Update asset
- `DELETE /api/v1/assets/{id}` - Delete asset

**Auth Required**: Organization membership

---

### SensorsController (`/api/v1/sensors`)
Manage sensors attached to assets.

- `GET /api/v1/sensors?asset_id={id}` - List sensors (optionally filtered by asset)
- `GET /api/v1/sensors/{id}` - Get sensor by ID
- `POST /api/v1/sensors` - Create new sensor
  ```json
  {
    "asset_id": 1,
    "name": "Temperature Sensor",
    "type": "Temperature",
    "unit": "°C",
    "threshold_low": 10.0,
    "threshold_high": 80.0,
    "sampling_interval_ms": 1000
  }
  ```
- `PUT /api/v1/sensors/{id}` - Update sensor (including thresholds)
- `DELETE /api/v1/sensors/{id}` - Delete sensor

**Auth Required**: Organization membership

---

### ReadingsController (`/api/v1/readings`)
Query sensor telemetry data.

- `GET /api/v1/readings?sensor_id={id}&limit={n}` - Get recent readings
  - **Query Params**:
    - `sensor_id` (required): Sensor ID
    - `limit` (optional, default: 100): Max readings to return
  - **Response**:
    ```json
    [
      {
        "id": 123,
        "sensor_id": 1,
        "timestamp": "2025-11-19T20:00:00Z",
        "value": 25.5,
        "quality": "Good"
      }
    ]
    ```

- `GET /api/v1/readings/by_sensor/{sensor_id}` - Get all readings for sensor

**Auth Required**: Organization membership

---

### AlertsController (`/api/v1/alerts`)
View and manage threshold violation alerts.

- `GET /api/v1/alerts?acknowledged={bool}` - List alerts
  - **Query Params**:
    - `acknowledged` (optional): Filter by acknowledgment status
  - **Response**:
    ```json
    [
      {
        "id": 1,
        "sensor_id": 5,
        "severity": "Critical",
        "message": "Temperature exceeded threshold",
        "trigger_value": 85.2,
        "threshold_value": 80.0,
        "acknowledged": false,
        "timestamp": "2025-11-19T20:15:00Z"
      }
    ]
    ```

- `GET /api/v1/alerts/{id}` - Get alert by ID
- `GET /api/v1/alerts/by_sensor/{sensor_id}` - Get alerts for specific sensor
- `POST /api/v1/alerts/{id}/acknowledge` - Acknowledge alert
  - Marks alert as reviewed by current user
  - Sets `acknowledged_at` and `acknowledged_by`

**Auth Required**: Organization membership

---

### CommandsController (`/api/v1/commands`)
Send commands to IoT devices.

- `GET /api/v1/commands` - Get pending commands for organization
- `GET /api/v1/commands/by_sensor/{sensor_id}` - Get commands for sensor
- `GET /api/v1/commands/{id}` - Get command by ID
- `POST /api/v1/commands` - Create new command
  ```json
  {
    "sensor_id": 1,
    "command_type": "SET_THRESHOLD",
    "payload": "{\"high\": 90, \"low\": 15}"
  }
  ```
  - Command is automatically assigned to current user and organization
  - Status starts as `Pending`

**Auth Required**: Organization membership

---

### UsersController (`/api/v1/users`)
User management within organization scope.

- `GET /api/v1/users/me` - Get current authenticated user
  ```json
  {
    "id": "user_123",
    "username": "john_doe",
    "email": "john@example.com",
    "first_name": "John",
    "last_name": "Doe",
    "is_onboarded": true
  }
  ```

- `GET /api/v1/users/by_organization` - List users in current organization
  - Returns basic user info for all organization members

**Auth Required**: Authenticated user

---

### OrganizationsController (`/api/v1/organizations`)
Organization information.

- `GET /api/v1/organizations/current` - Get current organization details
  ```json
  {
    "id": "org_123",
    "name": "Acme Industries",
    "owner_id": "user_456",
    "created_at": "2025-01-01T00:00:00Z"
  }
  ```

**Auth Required**: Organization membership

---

### WebhooksController (`/api/webhooks`)
Clerk webhook integration (no authentication required - uses signature verification).

- `POST /api/webhooks/clerk` - Receive Clerk events
  - **Supported Events**:
    - `user.created` - Syncs new user to database
    - `user.updated` - Updates user information
    - `organization.created` - Syncs new organization
    - `organization.updated` - Updates organization info
  - **Security**: Verifies `svix-signature` header using webhook secret

**Configuration Required**: `Clerk:WebhookSecret` in appsettings.json

---

## JSON Response Example
```json
{
  "id": 1,
  "organization_id": "org_123",
  "sensor_name": "Temperature Sensor",
  "current_value": 25.5,
  "last_seen": "2025-11-19T20:00:00Z"
}
```

## Configuration

### appsettings.json
```json
{
  "Clerk": {
    "SecretKey": "sk_test_...",
    "Domain": "your-app.clerk.accounts.dev",
    "WebhookSecret": "whsec_..."
  }
}
```

### Environment Variables (Production)
```bash
Clerk__SecretKey=sk_live_...
Clerk__Domain=your-app.clerk.accounts.dev
Clerk__WebhookSecret=whsec_...
```

## Error Responses

### 401 Unauthorized
```json
{
  "error": "Unauthorized"
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

### 500 Internal Server Error
```json
{
  "error": "Internal Server Error",
  "message": "Error details..."
}
```

## Security
- All endpoints require valid Clerk JWT token (except webhooks)
- Organization isolation enforced via `OrganizationId` claim
- User actions tracked via `UserId` claim
- Webhook signature verification using HMAC-SHA256

## Testing with cURL

### Get Current User
```bash
curl -X GET 'http://localhost:5008/api/v1/users/me' \
  -H 'Authorization: Bearer YOUR_CLERK_TOKEN'
```

### List Assets
```bash
curl -X GET 'http://localhost:5008/api/v1/assets' \
  -H 'Authorization: Bearer YOUR_CLERK_TOKEN'
```

### Create Sensor
```bash
curl -X POST 'http://localhost:5008/api/v1/sensors' \
  -H 'Authorization: Bearer YOUR_CLERK_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    "asset_id": 1,
    "name": "Temp Sensor",
    "type": "Temperature",
    "unit": "°C",
    "threshold_high": 80
  }'
```

### Acknowledge Alert
```bash
curl -X POST 'http://localhost:5008/api/v1/alerts/123/acknowledge' \
  -H 'Authorization: Bearer YOUR_CLERK_TOKEN'
```

## Next Steps
1. Add DTOs for request/response models
2. Implement SignalR hubs for real-time updates
3. Add MQTT ingestion background service
4. Implement command publishing to devices
5. Add pagination for list endpoints
6. Add filtering and sorting options
