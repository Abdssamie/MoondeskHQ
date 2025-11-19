using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class ReadingRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>
{
    private readonly TimescaleDbTestContainerFixture _fixture;

    public ReadingRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
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
    public async Task GetReadingsBySensorAsync_ShouldReturnReadingsForSensor()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var reading1 = MockData.CreateReading(sensor.Id, 25.5, orgId);
            reading1.Timestamp = DateTimeOffset.UtcNow;
            var reading2 = MockData.CreateReading(sensor.Id, 26.0, orgId);
            reading2.Timestamp = DateTimeOffset.UtcNow.AddSeconds(1);
            context.Readings.Add(reading1);
            context.Readings.Add(reading2);
            await context.SaveChangesAsync();

            var results = await repository.GetReadingsBySensorAsync(sensor.Id);

            results.Should().HaveCountGreaterThan(1);
            results.Should().AllSatisfy(r => r.SensorId.Should().Be(sensor.Id));
        }
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldReturnLimitedResults()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var baseTime = DateTimeOffset.UtcNow;
            for (int i = 0; i < 5; i++)
            {
                var reading = MockData.CreateReading(sensor.Id, 20 + i, orgId);
                reading.Timestamp = baseTime.AddSeconds(i);
                context.Readings.Add(reading);
            }
            await context.SaveChangesAsync();

            var results = await repository.GetRecentReadingsAsync(orgId, sensor.Id, limit: 3);

            results.Should().HaveCount(3);
        }
    }

    [Fact]
    public async Task BulkInsertReadingsAsync_ShouldInsertMultipleReadings()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var baseTime = DateTimeOffset.UtcNow;
            var readings = Enumerable.Range(1, 10)
                .Select(i =>
                {
                    var reading = MockData.CreateReading(sensor.Id, 20 + i, orgId);
                    reading.Timestamp = baseTime.AddSeconds(i);
                    return reading;
                })
                .ToList();

            await repository.BulkInsertReadingsAsync(readings);

            var results = await repository.GetReadingsBySensorAsync(sensor.Id);
            results.Should().HaveCountGreaterThan(9);
        }
    }

    [Fact]
    public async Task GetReadingsByQualityAsync_ShouldFilterByQuality()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var reading = MockData.CreateReading(sensor.Id, 25.5, orgId);
            context.Readings.Add(reading);
            await context.SaveChangesAsync();

            var results = await repository.GetReadingsByQualityAsync(ReadingQuality.Good);

            results.Should().Contain(r => r.SensorId == reading.SensorId && r.Timestamp == reading.Timestamp);
        }
    }

    [Fact]
    public async Task GetReadingsByProtocolAsync_ShouldFilterByProtocol()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var reading = MockData.CreateReading(sensor.Id, 25.5, orgId);
            context.Readings.Add(reading);
            await context.SaveChangesAsync();

            var results = await repository.GetReadingsByProtocolAsync(Protocol.Mqtt);

            results.Should().Contain(r => r.SensorId == reading.SensorId && r.Timestamp == reading.Timestamp);
        }
    }

    [Fact]
    public async Task UpdateReadingAsync_ShouldModifyReading()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var reading = MockData.CreateReading(sensor.Id, 25.5, orgId);
            context.Readings.Add(reading);
            await context.SaveChangesAsync();
            
            reading.Value = 30.0;
            reading.Quality = ReadingQuality.Bad;
            await repository.UpdateReadingAsync(reading.SensorId, reading.Timestamp, reading);

            var updated = await repository.GetReadingAsync(reading.SensorId, reading.Timestamp);
            updated.Value.Should().Be(30.0);
            updated.Quality.Should().Be(ReadingQuality.Bad);
        }
    }

    [Fact]
    public async Task DeleteReadingAsync_ShouldRemoveReading()
    {
        var (context, orgId, sensor) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ReadingRepository(context, NullLogger<ReadingRepository>.Instance);
            var reading = MockData.CreateReading(sensor.Id, 25.5, orgId);
            context.Readings.Add(reading);
            await context.SaveChangesAsync();

            await repository.DeleteReadingAsync(reading.SensorId, reading.Timestamp);

            var act = async () => await repository.GetReadingAsync(reading.SensorId, reading.Timestamp);
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
