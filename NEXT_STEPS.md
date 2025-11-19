# Next Steps for Moondesk MVP

## Immediate Priority (This Week)

### 1. **Configure Clerk** (Required for MVP)
```bash
# In Moondesk.Host/appsettings.json
{
  "Clerk": {
    "SecretKey": "sk_test_...",  # Get from Clerk Dashboard
    "Domain": "your-app.clerk.accounts.dev",
    "WebhookSecret": "whsec_..."  # For user/org sync
  }
}
```

**Steps:**
- [ ] Create Clerk account at https://clerk.com
- [ ] Create new application
- [ ] Copy Secret Key to appsettings.json
- [ ] Copy Domain to appsettings.json
- [ ] Create webhook endpoint in Clerk Dashboard
  - URL: `https://your-api.com/api/webhooks/clerk`
  - Events: `user.created`, `user.updated`, `organization.created`, `organization.updated`
- [ ] Copy Webhook Secret to appsettings.json

### 2. **Database Setup** (Required)
```bash
# Ensure TimescaleDB is running
docker ps | grep timescale

# If not running, start it
cd containers
docker-compose up -d

# Run migrations
cd ../Moondesk.Host
dotnet ef database update

# Verify tables created
psql -h localhost -p 5433 -U moondeskdb -d moondeskdb -c "\dt"
```

**Verify:**
- [ ] TimescaleDB container running
- [ ] Database migrations applied
- [ ] Tables created (Assets, Sensors, Readings, Alerts, etc.)

### 3. **Test API Locally**
```bash
# Start API
cd Moondesk.Host
dotnet run

# Visit Swagger UI
# Open browser: http://localhost:5008/swagger
```

**Test Endpoints:**
- [ ] GET /health - Should return 200
- [ ] GET /api/v1/organizations/current - Test with Clerk token
- [ ] GET /api/v1/users/me - Test authentication
- [ ] POST /api/v1/assets - Create test asset
- [ ] GET /api/v1/assets - List assets

**Get Clerk Token:**
1. Create test user in Clerk Dashboard
2. Use Clerk's test token or integrate frontend
3. Add to request header: `Authorization: Bearer YOUR_TOKEN`

---

## Phase 2: Core Features (Next 2 Weeks)

### 4. **MQTT Integration**
- [ ] Sign up for EMQX Cloud (free tier)
- [ ] Create background service: `MqttIngestionService`
- [ ] Subscribe to telemetry topics: `{org_id}/{device_id}/telemetry`
- [ ] Implement command publishing to devices
- [ ] Test with Edge Simulator

### 5. **SignalR Real-time Updates**
- [ ] Create `TelemetryHub` in Moondesk.API
- [ ] Broadcast readings to connected clients
- [ ] Push alerts in real-time
- [ ] Test with SignalR client

### 6. **Frontend Development**
- [ ] Initialize Next.js project
- [ ] Install Clerk SDK for Next.js
- [ ] Create authentication flow
- [ ] Build asset management UI
- [ ] Build sensor configuration UI
- [ ] Display real-time readings
- [ ] Show alerts dashboard

---

## Phase 3: Deployment (Week 4)

### 7. **Production Deployment**

**Database:**
- [ ] Deploy TimescaleDB on VPS or managed service
- [ ] Configure connection string
- [ ] Run production migrations
- [ ] Set up automated backups

**API:**
- [ ] Build Docker image for Moondesk.Host
- [ ] Deploy to VPS/cloud (DigitalOcean, AWS, Azure)
- [ ] Configure environment variables
- [ ] Set up reverse proxy (Nginx)
- [ ] Enable HTTPS with Let's Encrypt

**Frontend:**
- [ ] Deploy Next.js to Vercel
- [ ] Configure environment variables
- [ ] Connect to production API

**Monitoring:**
- [ ] Set up health check monitoring
- [ ] Configure Serilog for production logging
- [ ] Set up error tracking (optional: Sentry)

---

## Phase 4: Post-MVP Enhancements

### 8. **Advanced Features**
- [ ] Data aggregation and analytics
- [ ] Historical data export (CSV)
- [ ] Alert notification channels (email, SMS)
- [ ] User roles and permissions
- [ ] Asset maintenance scheduling
- [ ] Mobile app (optional)

### 9. **Performance & Scale**
- [ ] Implement caching (Redis)
- [ ] Add pagination to list endpoints
- [ ] Optimize TimescaleDB queries
- [ ] Set up CDN for frontend
- [ ] Load testing

### 10. **DevOps**
- [ ] Set up CI/CD pipeline (GitHub Actions)
- [ ] Automated testing in pipeline
- [ ] Staging environment
- [ ] Database migration automation
- [ ] Monitoring and alerting

---

## MVP Success Criteria

✅ **Must Have:**
- Users can sign up and log in (Clerk)
- Users can create and manage assets
- Users can add sensors to assets
- Users can view sensor readings
- Users can see and acknowledge alerts
- Organization-based data isolation

⏭️ **Nice to Have (Post-MVP):**
- Real-time data streaming (SignalR)
- MQTT device integration
- Historical data charts
- Email notifications
- Mobile responsive design

---

## Current Status

✅ **Completed:**
- Domain models and entities
- Database schema with TimescaleDB
- Repository pattern implementation
- REST API with 7 controllers
- Clerk authentication middleware
- API versioning and snake_case naming
- Swagger documentation
- 28 functional tests
- Health checks

🔄 **In Progress:**
- Clerk configuration
- Database deployment
- API testing

⏭️ **Next:**
- Frontend development
- MQTT integration
- Real-time features

---

## Quick Start Commands

```bash
# Start database
cd containers && docker-compose up -d

# Run migrations
cd ../Moondesk.Host && dotnet ef database update

# Start API
dotnet run

# Run tests
cd ../Moondesk.API.Tests && dotnet test

# Build for production
cd ../Moondesk.Host && dotnet publish -c Release
```

---

## Resources

- **Clerk Docs:** https://clerk.com/docs
- **TimescaleDB Docs:** https://docs.timescale.com
- **EMQX Cloud:** https://www.emqx.com/en/cloud
- **SignalR Docs:** https://learn.microsoft.com/en-us/aspnet/core/signalr
- **Next.js Docs:** https://nextjs.org/docs

---

**Last Updated:** 2025-11-19
