using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class SensorRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>, IAsyncLifetime
{
    private readonly TimescaleDbTestContainerFixture _fixture;
    private MoondeskDbContext _context = null!;
    private string _orgId = null!;
    private Asset _asset = null!;

    public SensorRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = CreateContext();
        await _context.Database.EnsureCreatedAsync();
        
        var org = MockData.CreateOrganization();
        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();
        _orgId = org.Id;
        
        _asset = MockData.CreateAsset(organizationId: _orgId);
        _context.Assets.Add(_asset);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        
        await _context.DisposeAsync();
    }

    private MoondeskDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MoondeskDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        return new MoondeskDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSensor_WhenExists()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        var sensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(sensor.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sensor.Id);
        result.Name.Should().Be(sensor.Name);
    }

    [Fact]
    public async Task GetByAssetIdAsync_ShouldReturnSensorsForAsset()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        _context.Sensors.Add(MockData.CreateSensor(_asset.Id, organizationId: _orgId));
        _context.Sensors.Add(MockData.CreateSensor(_asset.Id, organizationId: _orgId));
        await _context.SaveChangesAsync();

        var results = await repository.GetByAssetIdAsync(_asset.Id);

        var enumerable = results.ToList();
        enumerable.Should().HaveCountGreaterThan(1);
        enumerable.Should().AllSatisfy(s => s.AssetId.Should().Be(_asset.Id));
    }

    [Fact]
    public async Task AddAsync_ShouldCreateSensor()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        var sensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);

        var result = await repository.AddAsync(sensor);

        result.Id.Should().BeGreaterThan(0);
        result.AssetId.Should().Be(_asset.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifySensor()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        var sensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        sensor.Name = "Updated Sensor";
        sensor.IsActive = false;
        await repository.UpdateAsync(sensor);

        var updated = await repository.GetByIdAsync(sensor.Id);
        updated!.Name.Should().Be("Updated Sensor");
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSensor()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        var sensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        await repository.DeleteAsync(sensor.Id);

        var result = await repository.GetByIdAsync(sensor.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSensorsAsync_ShouldReturnOnlyActiveSensors()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        var activeSensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);
        var inactiveSensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);
        inactiveSensor.IsActive = false;
        _context.Sensors.AddRange(activeSensor, inactiveSensor);
        await _context.SaveChangesAsync();

        var results = await repository.GetActiveSensorsAsync();

        var enumerable = results.ToList();
        enumerable.Should().Contain(s => s.Id == activeSensor.Id);
        enumerable.Should().NotContain(s => s.Id == inactiveSensor.Id);
    }

    [Fact]
    public async Task GetSensorsByTypeAsync_ShouldFilterByType()
    {
        var repository = new SensorRepository(_context, NullLogger<SensorRepository>.Instance);
        var sensor = MockData.CreateSensor(_asset.Id, organizationId: _orgId);
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        var results = await repository.GetSensorsByTypeAsync(SensorType.Temperature);

        var enumerable = results.ToList();
        enumerable.Should().Contain(s => s.Id == sensor.Id);
        enumerable.Should().AllSatisfy(s => s.Type.Should().Be(SensorType.Temperature));
    }
}
