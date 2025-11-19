using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.Network;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class ConnectionCredentialRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>
{
    private readonly TimescaleDbTestContainerFixture _fixture;

    public ConnectionCredentialRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(MoondeskDbContext context, string orgId)> SetupTestDataAsync()
    {
        var context = CreateContext();
        
        var org = MockData.CreateOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        
        return (context, org.Id);
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

    private ConnectionCredential CreateCredential(string orgId) => new()
    {
        Name = "Test Connection",
        EndpointUri = "mqtt://localhost:1883",
        OrganizationId = orgId,
        Username = "testuser",
        ClientId = "client123",
        EncryptedPassword = "encrypted",
        EncryptionIV = "iv123",
        Protocol = Protocol.Mqtt,
        IsActive = true
    };

    [Fact]
    public async Task CreateAsync_ShouldCreateCredential()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            var credential = CreateCredential(orgId);

            var result = await repository.CreateAsync(credential);

            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be(credential.Name);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCredential_WhenExists()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            var credential = CreateCredential(orgId);
            context.ConnectionCredentials.Add(credential);
            await context.SaveChangesAsync();

            var result = await repository.GetByIdAsync(credential.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(credential.Id);
        }
    }

    [Fact]
    public async Task GetByOrganizationIdAsync_ShouldReturnCredentialsForOrganization()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            context.ConnectionCredentials.Add(CreateCredential(orgId));
            context.ConnectionCredentials.Add(CreateCredential(orgId));
            await context.SaveChangesAsync();

            var results = await repository.GetByOrganizationIdAsync(orgId);

            var connectionCredentials = results.ToList();
            connectionCredentials.Should().HaveCountGreaterThan(1);
            connectionCredentials.Should().AllSatisfy(c => c.OrganizationId.Should().Be(orgId));
        }
    }

    [Fact]
    public async Task GetActiveByOrganizationIdAsync_ShouldReturnOnlyActiveCredentials()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            var active = CreateCredential(orgId);
            var inactive = CreateCredential(orgId);
            inactive.IsActive = false;
            context.ConnectionCredentials.AddRange(active, inactive);
            await context.SaveChangesAsync();

            var results = await repository.GetActiveAsyncByOrganizationIdAsync(orgId);

            var connectionCredentials = results.ToList();
            connectionCredentials.Should().Contain(c => c.Id == active.Id);
            connectionCredentials.Should().NotContain(c => c.Id == inactive.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyCredential()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            var credential = CreateCredential(orgId);
            context.ConnectionCredentials.Add(credential);
            await context.SaveChangesAsync();

            credential.Name = "Updated Connection";
            credential.IsActive = false;
            await repository.UpdateAsync(credential);

            var updated = await repository.GetByIdAsync(credential.Id);
            updated!.Name.Should().Be("Updated Connection");
            updated.IsActive.Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCredential()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            var credential = CreateCredential(orgId);
            context.ConnectionCredentials.Add(credential);
            await context.SaveChangesAsync();

            await repository.DeleteAsync(credential.Id);

            var result = await repository.GetByIdAsync(credential.Id);
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetByProtocolAsync_ShouldFilterByProtocol()
    {
        var (context, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new ConnectionCredentialRepository(context, NullLogger<ConnectionCredentialRepository>.Instance);
            var credential = CreateCredential(orgId);
            context.ConnectionCredentials.Add(credential);
            await context.SaveChangesAsync();

            var results = await repository.GetByProtocolAsync(Protocol.Mqtt, orgId);

            results.Should().Contain(c => c.Id == credential.Id);
        }
    }
}
