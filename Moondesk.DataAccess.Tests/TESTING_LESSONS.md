# Testing Lessons Learned

## Key Principles for Integration Testing with TimescaleDB

### 1. Composite Primary Keys Require Unique Combinations
**Problem:** Duplicate key violations when inserting multiple test records.

**Solution:** Ensure each record has a unique combination of primary key fields.

```csharp
// ❌ BAD - Same timestamp creates duplicates
context.Readings.Add(MockData.CreateReading(sensorId, 25.5, orgId));
context.Readings.Add(MockData.CreateReading(sensorId, 26.0, orgId));

// ✅ GOOD - Unique timestamps
var reading1 = MockData.CreateReading(sensorId, 25.5, orgId);
reading1.Timestamp = DateTimeOffset.UtcNow;
var reading2 = MockData.CreateReading(sensorId, 26.0, orgId);
reading2.Timestamp = DateTimeOffset.UtcNow.AddSeconds(1);
```

### 2. TimescaleDB Hypertables Need Timestamp in Primary Key
**Lesson:** For time-series data, use `(SensorId, Timestamp)` as composite PK, not auto-incrementing IDs.

**Why:** TimescaleDB partitions data by time chunks and requires the partitioning column in the primary key for efficient queries.

```csharp
// ✅ Correct TimescaleDB pattern
entity.HasKey(e => new { e.SensorId, e.Timestamp });
```

### 3. Test Isolation with Separate Contexts
**Problem:** Tests sharing database state causing interference.

**Solution:** Create fresh context per test using helper method.

```csharp
// ✅ Each test gets isolated data
private async Task<(MoondeskDbContext context, string orgId, Sensor sensor)> SetupTestDataAsync()
{
    var context = CreateContext();
    await context.Database.EnsureCreatedAsync();
    // Setup test data...
    return (context, orgId, sensor);
}

[Fact]
public async Task MyTest()
{
    var (context, orgId, sensor) = await SetupTestDataAsync();
    await using (context)
    {
        // Test logic
    }
}
```

### 4. Foreign Key Integrity
**Problem:** Trying to insert records with non-existent foreign keys.

**Solution:** Always create parent entities before children.

```csharp
// ✅ Correct order
var org = MockData.CreateOrganization();
context.Organizations.Add(org);
await context.SaveChangesAsync();

var asset = MockData.CreateAsset(organizationId: org.Id);
context.Assets.Add(asset);
await context.SaveChangesAsync();

var sensor = MockData.CreateSensor(asset.Id, organizationId: org.Id);
context.Sensors.Add(sensor);
await context.SaveChangesAsync();
```

### 5. MockData Should Be Minimal
**Principle:** Mock data factories should only create objects, not handle database operations.

```csharp
// ✅ GOOD - Simple factory
public static Reading CreateReading(long sensorId, double value, string orgId) => new()
{
    SensorId = sensorId,
    OrganizationId = orgId,
    Timestamp = DateTimeOffset.UtcNow,
    Value = value,
    Protocol = Protocol.Mqtt,
    Quality = ReadingQuality.Good
};

// ❌ BAD - Don't add database logic to MockData
// No TestDataBuilder pattern needed
```

### 6. Use Correct Enum Values
**Problem:** Using wrong enum names causes compilation errors.

```csharp
// ❌ Wrong
Protocol.MQTT
ReadingQuality.Poor

// ✅ Correct
Protocol.Mqtt
ReadingQuality.Bad
```

### 7. Testcontainers for Real Database Testing
**Benefit:** Tests run against actual TimescaleDB, catching real-world issues.

```csharp
public class TimescaleDbTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public TimescaleDbTestContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("timescale/timescaledb:latest-pg17")
            .Build();
    }

    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.StopAsync();
}
```

## Summary
- Use unique timestamps for time-series data
- Follow TimescaleDB composite key patterns
- Isolate test data per test
- Respect foreign key relationships
- Keep mock data simple
- Test against real database with Testcontainers
