using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class CommandRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>, IAsyncLifetime
{
    private readonly TimescaleDbTestContainerFixture _fixture;
    private MoondeskDbContext _context = null!;
    private string _orgId = null!;
    private Sensor _sensor = null!;
    private string _userId = null!;

    public CommandRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
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
        
        var asset = MockData.CreateAsset(organizationId: _orgId);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        
        _sensor = MockData.CreateSensor(asset.Id, organizationId: _orgId);
        _context.Sensors.Add(_sensor);
        await _context.SaveChangesAsync();
        
        _userId = Guid.NewGuid().ToString();
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
    public async Task AddAsync_ShouldCreateCommand()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);

        var result = await repository.AddAsync(command);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Status.Should().Be(CommandStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCommand_WhenExists()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(command.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task GetBySensorIdAsync_ShouldReturnCommandsForSensor()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        _context.Commands.Add(MockData.CreateCommand(_sensor.Id, _userId, _orgId));
        _context.Commands.Add(MockData.CreateCommand(_sensor.Id, _userId, _orgId));
        await _context.SaveChangesAsync();

        var results = await repository.GetBySensorIdAsync(_sensor.Id);

        results.Should().HaveCountGreaterThan(1);
        results.Should().AllSatisfy(c => c.SensorId.Should().Be(_sensor.Id));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldFilterByStatus()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();

        var results = await repository.GetByStatusAsync(CommandStatus.Pending);

        results.Should().Contain(c => c.Id == command.Id);
    }

    [Fact]
    public async Task GetPendingCommandsAsync_ShouldReturnPendingCommandsForOrganization()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();

        var results = await repository.GetPendingCommandsAsync(_orgId);

        results.Should().HaveCountGreaterThan(0);
        results.Should().AllSatisfy(c => 
        {
            c.OrganizationId.Should().Be(_orgId);
            c.Status.Should().Be(CommandStatus.Pending);
        });
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyCommand()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();
        
        command.MarkAsExecuting();
        await repository.UpdateAsync(command);

        var updated = await repository.GetByIdAsync(command.Id);
        updated!.Status.Should().Be(CommandStatus.Executing);
        updated.ExecutedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldMarkAsCompleted()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();
        
        command.MarkAsCompleted("Success");
        await repository.UpdateAsync(command);

        var updated = await repository.GetByIdAsync(command.Id);
        updated!.Status.Should().Be(CommandStatus.Completed);
        updated.Response.Should().Be("Success");
        updated.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldMarkAsFailed()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();
        
        command.MarkAsFailed("Connection timeout");
        await repository.UpdateAsync(command);

        var updated = await repository.GetByIdAsync(command.Id);
        updated!.Status.Should().Be(CommandStatus.Failed);
        updated.ErrorMessage.Should().Be("Connection timeout");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCommand()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();

        await repository.DeleteAsync(command.Id);

        var result = await repository.GetByIdAsync(command.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Command_ShouldSupportRetryLogic()
    {
        var repository = new CommandRepository(_context, NullLogger<CommandRepository>.Instance);
        var command = MockData.CreateCommand(_sensor.Id, _userId, _orgId);
        _context.Commands.Add(command);
        await _context.SaveChangesAsync();
        
        command.MarkAsFailed("First attempt failed");
        command.CanRetry().Should().BeTrue();
        command.IncrementRetry();
        await repository.UpdateAsync(command);

        var updated = await repository.GetByIdAsync(command.Id);
        updated!.RetryCount.Should().Be(1);
        updated.Status.Should().Be(CommandStatus.Pending);
    }
}
