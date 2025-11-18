# 🗄️ Moondesk.DataAccess

## Overview
The **DataAccess** layer handles all database interactions using Entity Framework Core and TimescaleDB (PostgreSQL). It implements the repository interfaces defined in the Domain layer.

## Purpose
- Implement repository interfaces from `Moondesk.Domain`
- Configure Entity Framework Core DbContext
- Manage database schema via migrations
- Optimize queries for time-series data
- Enforce multi-tenancy (organization isolation)

---

## Key Responsibilities

### 1. **DbContext Configuration**
Create and configure `MoondeskDbContext` with:
- Entity relationships (Foreign Keys, Navigation Properties)
- Indexes for performance (timestamp, sensor_id, organization_id)
- TimescaleDB hypertable configuration for Readings
- Global query filters for multi-tenancy

### 2. **Repository Implementations**
Implement the interfaces from `Moondesk.Domain`:
- `AssetRepository` - CRUD for assets with organization filtering
- `SensorRepository` - CRUD for sensors with threshold queries
- `ReadingRepository` - High-performance time-series operations
- `AlertRepository` - Alert management with acknowledgment logic

### 3. **Multi-Tenancy**
- Add `OrganizationId` column to all entities
- Apply global query filter: `.Where(e => e.OrganizationId == currentOrgId)`
- Prevent cross-organization data leaks
- Index on OrganizationId for performance

### 4. **TimescaleDB Integration**
- Configure Readings table as hypertable (partitioned by time)
- Set up continuous aggregates for dashboard queries
- Implement data retention policies (e.g., 90 days)
- Use time-bucket functions for aggregations

---

## Database Schema

### Tables

#### **Assets**
```sql
CREATE TABLE Assets (
    Id SERIAL PRIMARY KEY,
    OrganizationId VARCHAR(100) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Type VARCHAR(100),
    Location VARCHAR(200),
    Status INT NOT NULL,
    LastSeen TIMESTAMPTZ,
    Description VARCHAR(500),
    Manufacturer VARCHAR(100),
    ModelNumber VARCHAR(100),
    InstallationDate TIMESTAMPTZ,
    INDEX idx_assets_org (OrganizationId)
);
```

#### **Sensors**
```sql
CREATE TABLE Sensors (
    Id SERIAL PRIMARY KEY,
    AssetId INT NOT NULL REFERENCES Assets(Id) ON DELETE CASCADE,
    OrganizationId VARCHAR(100) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Type INT NOT NULL,
    Unit VARCHAR(20),
    ThresholdLow DOUBLE PRECISION,
    ThresholdHigh DOUBLE PRECISION,
    MinValue DOUBLE PRECISION,
    MaxValue DOUBLE PRECISION,
    SamplingIntervalMs INT DEFAULT 1000,
    IsActive BOOLEAN DEFAULT TRUE,
    Description VARCHAR(500),
    INDEX idx_sensors_asset (AssetId),
    INDEX idx_sensors_org (OrganizationId)
);
```

#### **Readings** (TimescaleDB Hypertable)
```sql
CREATE TABLE Readings (
    Id BIGSERIAL,
    SensorId INT NOT NULL REFERENCES Sensors(Id) ON DELETE CASCADE,
    OrganizationId VARCHAR(100) NOT NULL,
    Timestamp TIMESTAMPTZ NOT NULL,
    Value DOUBLE PRECISION NOT NULL,
    Quality INT DEFAULT 0,
    Notes TEXT,
    PRIMARY KEY (Id, Timestamp)
);

-- Convert to hypertable (partitioned by time)
SELECT create_hypertable('Readings', 'Timestamp');

-- Create indexes
CREATE INDEX idx_readings_sensor_time ON Readings (SensorId, Timestamp DESC);
CREATE INDEX idx_readings_org ON Readings (OrganizationId);
```

#### **Alerts**
```sql
CREATE TABLE Alerts (
    Id SERIAL PRIMARY KEY,
    SensorId INT NOT NULL REFERENCES Sensors(Id) ON DELETE CASCADE,
    OrganizationId VARCHAR(100) NOT NULL,
    Timestamp TIMESTAMPTZ NOT NULL,
    Severity INT NOT NULL,
    Message VARCHAR(500),
    TriggerValue DOUBLE PRECISION,
    ThresholdValue DOUBLE PRECISION,
    Acknowledged BOOLEAN DEFAULT FALSE,
    AcknowledgedAt TIMESTAMPTZ,
    AcknowledgedBy VARCHAR(100),
    Notes VARCHAR(500),
    INDEX idx_alerts_sensor (SensorId),
    INDEX idx_alerts_org (OrganizationId),
    INDEX idx_alerts_severity (Severity, Acknowledged)
);
```

---

## Implementation Guide

### Step 1: Create DbContext

```csharp
public class MoondeskDbContext : DbContext
{
    private readonly string _currentOrganizationId;

    public MoondeskDbContext(
        DbContextOptions<MoondeskDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        // Extract organization ID from JWT claims
        _currentOrganizationId = httpContextAccessor.HttpContext?
            .User.FindFirst("org_id")?.Value ?? string.Empty;
    }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Asset>()
            .HasMany(a => a.Sensors)
            .WithOne(s => s.Asset)
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter for multi-tenancy
        modelBuilder.Entity<Asset>()
            .HasQueryFilter(a => a.OrganizationId == _currentOrganizationId);
        
        modelBuilder.Entity<Sensor>()
            .HasQueryFilter(s => s.OrganizationId == _currentOrganizationId);
        
        modelBuilder.Entity<Reading>()
            .HasQueryFilter(r => r.OrganizationId == _currentOrganizationId);
        
        modelBuilder.Entity<Alert>()
            .HasQueryFilter(a => a.OrganizationId == _currentOrganizationId);

        // Indexes
        modelBuilder.Entity<Reading>()
            .HasIndex(r => new { r.SensorId, r.Timestamp });
    }
}
```

### Step 2: Implement Repositories

```csharp
public class AssetRepository : IAssetRepository
{
    private readonly MoondeskDbContext _context;

    public AssetRepository(MoondeskDbContext context)
    {
        _context = context;
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.Sensors)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _context.Assets
            .Include(a => a.Sensors)
            .ToListAsync();
    }

    public async Task<Asset> AddAsync(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Step 3: Time-Series Queries

```csharp
public class ReadingRepository
{
    private readonly MoondeskDbContext _context;

    // Get readings for a time range
    public async Task<IEnumerable<Reading>> GetReadingsAsync(
        int sensorId, 
        DateTimeOffset start, 
        DateTimeOffset end)
    {
        return await _context.Readings
            .Where(r => r.SensorId == sensorId 
                && r.Timestamp >= start 
                && r.Timestamp <= end)
            .OrderBy(r => r.Timestamp)
            .ToListAsync();
    }

    // Get aggregated data using TimescaleDB time_bucket
    public async Task<IEnumerable<AggregatedReading>> GetAggregatedReadingsAsync(
        int sensorId, 
        DateTimeOffset start, 
        DateTimeOffset end,
        string interval = "1 hour")
    {
        var sql = @"
            SELECT 
                time_bucket(@interval, ""Timestamp"") AS bucket,
                AVG(""Value"") AS avg_value,
                MIN(""Value"") AS min_value,
                MAX(""Value"") AS max_value
            FROM ""Readings""
            WHERE ""SensorId"" = @sensorId 
                AND ""Timestamp"" >= @start 
                AND ""Timestamp"" <= @end
            GROUP BY bucket
            ORDER BY bucket";

        return await _context.Database
            .SqlQueryRaw<AggregatedReading>(sql, 
                new NpgsqlParameter("interval", interval),
                new NpgsqlParameter("sensorId", sensorId),
                new NpgsqlParameter("start", start),
                new NpgsqlParameter("end", end))
            .ToListAsync();
    }

    // Bulk insert for high throughput
    public async Task BulkInsertReadingsAsync(IEnumerable<Reading> readings)
    {
        await _context.Readings.AddRangeAsync(readings);
        await _context.SaveChangesAsync();
    }
}
```

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=moondesk;Username=postgres;Password=yourpassword"
  }
}
```

### Program.cs (in Moondesk.API)
```csharp
builder.Services.AddDbContext<MoondeskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
```

---

## Migrations

### Create Initial Migration
```bash
dotnet ef migrations add InitialCreate --project Moondesk.DataAccess --startup-project Moondesk.API
```

### Apply Migration
```bash
dotnet ef database update --project Moondesk.DataAccess --startup-project Moondesk.API
```

### Add TimescaleDB Hypertable (Manual SQL)
After running migrations, execute:
```sql
SELECT create_hypertable('Readings', 'Timestamp');
```

---

## Performance Optimization

### 1. **Connection Pooling**
Already enabled by default in Npgsql.

### 2. **Batch Inserts**
Use `AddRangeAsync()` for multiple readings:
```csharp
await _context.Readings.AddRangeAsync(readings);
await _context.SaveChangesAsync();
```

### 3. **Async All the Way**
Always use async methods to avoid blocking threads.

### 4. **Indexes**
- Composite index on (SensorId, Timestamp) for time-range queries
- Index on OrganizationId for multi-tenancy filtering

### 5. **Data Retention**
Set up TimescaleDB retention policy:
```sql
SELECT add_retention_policy('Readings', INTERVAL '90 days');
```

---

## Required NuGet Packages

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
```

---

## Developer Notes

### Multi-Tenancy Best Practices
- **Always** set OrganizationId when creating entities
- **Never** bypass query filters (use `IgnoreQueryFilters()` only for admin operations)
- **Test** cross-organization isolation thoroughly

### TimescaleDB Tips
- Use `time_bucket()` for aggregations instead of GROUP BY
- Leverage continuous aggregates for frequently-accessed dashboard data
- Monitor hypertable chunk size (default 7 days is good for IoT)

### Testing
- Use in-memory database for unit tests
- Use Docker container with TimescaleDB for integration tests
- Test query performance with realistic data volumes (millions of rows)

---

**Project Type**: Class Library  
**Target Framework**: .NET 8.0  
**Dependencies**: Moondesk.Domain, Npgsql.EntityFrameworkCore.PostgreSQL
