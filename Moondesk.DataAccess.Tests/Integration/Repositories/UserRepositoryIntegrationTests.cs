using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class UserRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>
{
    private readonly TimescaleDbTestContainerFixture _fixture;

    public UserRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private MoondeskDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MoondeskDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        var context = new MoondeskDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AddAsync_ShouldCreateUser()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = MockData.CreateUser();

        var result = await repository.CreateAsync(user);

        result.Id.Should().NotBeNullOrEmpty();
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = MockData.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = MockData.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await repository.GetByEmailAsync(user.Email);

        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = MockData.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await repository.GetByUsernameAsync(user.Username);

        result.Should().NotBeNull();
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyUser()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = MockData.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        user.FirstName = "Updated";
        user.IsOnboarded = false;
        await repository.UpdateAsync(user);

        var updated = await repository.GetByIdAsync(user.Id);
        updated!.FirstName.Should().Be("Updated");
        updated.IsOnboarded.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = MockData.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await repository.DeleteAsync(user.Id);

        var result = await repository.GetByIdAsync(user.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        context.Users.Add(MockData.CreateUser(Guid.NewGuid().ToString()));
        context.Users.Add(MockData.CreateUser(Guid.NewGuid().ToString()));
        await context.SaveChangesAsync();

        var results = await repository.GetAllAsync();

        results.Should().HaveCountGreaterThan(1);
    }
}
