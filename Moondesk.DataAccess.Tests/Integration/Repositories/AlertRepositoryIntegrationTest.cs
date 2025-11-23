using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class AlertRepositoryIntegrationTest : IClassFixture<TimescaleDbTestContainerFixture>
{
    private readonly TimescaleDbTestContainerFixture _fixture;

    public AlertRepositoryIntegrationTest(TimescaleDbTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(MoondeskDbContext context, string orgId, Sensor sensor)> SetupTestDataAsync()
    {
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        
        var org = MockData.CreateOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        
        var asset = MockData.CreateAsset(organizationId: org.Id);
        context.Assets.Add(asset);
        await context.SaveChangesAsync();
        
        var sensor = MockData.CreateSensor(asset.Id, organizationId: org.Id);
        context.Sensors.Add(sensor);
        await context.SaveChangesAsync();
        
        return (context, org.Id, sensor);
    }

    private MoondeskDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MoondeskDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        return new MoondeskDbContext(options);
    }

    [Fact]
    public async Task CreateAlertAsync_ShouldSucceed()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            var alert = MockData.CreateAlert(sensor.Id, orgId);

            var result = await repository.CreateAlertAsync(alert);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.SensorId.Should().Be(sensor.Id);
            result.OrganizationId.Should().Be(orgId);
        }
    }

    [Fact]
    public async Task GetAlertAsync_ShouldReturnAlert_WhenExists()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            var alert = MockData.CreateAlert(sensor.Id, orgId);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var result = await repository.GetAlertAsync(alert.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(alert.Id);
        }
    }

    [Fact]
    public async Task GetAlertsBySensorAsync_ShouldReturnAlertsForSensor()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            context.Alerts.Add(MockData.CreateAlert(sensor.Id, orgId));
            context.Alerts.Add(MockData.CreateAlert(sensor.Id, orgId));
            await context.SaveChangesAsync();

            var results = await repository.GetAlertsBySensorAsync(sensor.Id);

            results = results as Alert[] ?? results.ToArray();
            
            results.Should().NotBeNull();
            results.Should().HaveCountGreaterThan(1);
            results.Should().AllSatisfy(a => a.SensorId.Should().Be(sensor.Id));
        }
    }

    [Fact]
    public async Task GetAlertsByAlertSeverityAsync_ShouldFilterBySeverity()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            var alert = MockData.CreateAlert(sensor.Id, orgId);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var results = await repository.GetAlertsByAlertSeverityAsync(AlertSeverity.Warning);

            results.Should().Contain(a => a.Id == alert.Id);
        }
    }

    [Fact]
    public async Task UpdateAlertAsync_ShouldModifyAlert()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            var alert = MockData.CreateAlert(sensor.Id, orgId);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();
            
            alert.Acknowledged = true;
            alert.AcknowledgedBy = "test-user";
            await repository.UpdateAlertAsync(alert.Id, alert);

            var updated = await repository.GetAlertAsync(alert.Id);
            updated.Should().NotBeNull();
            updated.Acknowledged.Should().BeTrue();
            updated.AcknowledgedBy.Should().Be("test-user");
        }
    }

    [Fact]
    public async Task DeleteAlertAsync_ShouldRemoveAlert()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            var alert = MockData.CreateAlert(sensor.Id, orgId);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            await repository.DeleteAlertAsync(alert.Id);

            var result = await repository.GetAlertAsync(alert.Id);
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetAlertsByProtocolAsync_ShouldFilterByProtocol()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new AlertRepository(context, NullLogger<AlertRepository>.Instance);
            var alert = MockData.CreateAlert(sensor.Id, orgId);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var results = await repository.GetAlertsByProtocolAsync(Protocol.Mqtt);

            results.Should().Contain(a => a.Id == alert.Id);
        }
    }
}
