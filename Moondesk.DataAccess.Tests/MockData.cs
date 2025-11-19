using Moondesk.Domain.Enums;
using Moondesk.Domain.Models;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Tests;

public static class MockData
{
    public static User CreateUser(string? id = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        Username = "testuser",
        Email = "test@example.com",
        FirstName = "Test",
        LastName = "User",
        IsOnboarded = true,
        CreatedAt = DateTime.UtcNow
    };

    public static Organization CreateOrganization(string? id = null, string? ownerId = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        Name = "Test Org",
        OwnerId = ownerId ?? Guid.NewGuid().ToString(),
        SubscriptionPlan = SubscriptionPlan.Free,
        StorageLimitGB = 5,
        CreatedAt = DateTime.UtcNow
    };

    public static OrganizationMembership CreateMembership(string userId, string orgId) => new()
    {
        UserId = userId,
        OrganizationId = orgId,
        Role = UserRole.Admin,
        JoinedAt = DateTime.UtcNow
    };

    public static Asset CreateAsset(long? id = null, string? organizationId = null) => new()
    {
        Id = id ?? 0,
        OrganizationId = organizationId ?? Guid.NewGuid().ToString(),
        Name = "Test Asset",
        Type = "Pump",
        Location = "Building A",
        Status = AssetStatus.Online,
        LastSeen = DateTimeOffset.UtcNow,
        Description = "Test pump asset",
        Manufacturer = "TestCorp",
        ModelNumber = "TP-100"
    };

    public static Sensor CreateSensor(long assetId, long? id = null, string? organizationId = null) => new()
    {
        Id = id ?? 0,
        AssetId = assetId,
        OrganizationId = organizationId ?? Guid.NewGuid().ToString(),
        Name = "Temperature Sensor",
        Type = SensorType.Temperature,
        Unit = "°C",
        ThresholdLow = 0,
        ThresholdHigh = 100,
        SamplingIntervalMs = 1000,
        IsActive = true,
        Protocol = Protocol.Mqtt
    };

    public static Reading CreateReading(long sensorId, double value = 25.5, string? organizationId = null) => new()
    {
        SensorId = sensorId,
        OrganizationId = organizationId ?? Guid.NewGuid().ToString(),
        Value = value,
        Protocol = Protocol.Mqtt,
        Quality = ReadingQuality.Good
    };

    public static Alert CreateAlert(long sensorId, string? organizationId = null) => new()
    {
        SensorId = sensorId,
        OrganizationId = organizationId ?? Guid.NewGuid().ToString(),
        Timestamp = DateTimeOffset.UtcNow,
        Severity = AlertSeverity.Warning,
        Message = "Temperature threshold exceeded",
        TriggerValue = 105,
        ThresholdValue = 100,
        Acknowledged = false,
        Protocol = Protocol.Mqtt
    };

    public static Command CreateCommand(long sensorId, string userId, string? organizationId = null) => new()
    {
        SensorId = sensorId,
        UserId = userId,
        OrganizationId = organizationId ?? Guid.NewGuid().ToString(),
        CommandType = "SetThreshold",
        Payload = "{\"value\": 100}",
        Status = CommandStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };
}
