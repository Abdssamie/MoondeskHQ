using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class OrganizationRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>
{
    private readonly TimescaleDbTestContainerFixture _fixture;

    public OrganizationRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
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
    public async Task AddAsync_ShouldCreateOrganization()
    {
        await using var context = CreateContext();
        var repository = new OrganizationRepository(context, NullLogger<OrganizationRepository>.Instance);
        var org = MockData.CreateOrganization();

        var result = await repository.CreateAsync(org);

        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be(org.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrganization_WhenExists()
    {
        await using var context = CreateContext();
        var repository = new OrganizationRepository(context, NullLogger<OrganizationRepository>.Instance);
        var org = MockData.CreateOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(org.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(org.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyOrganization()
    {
        await using var context = CreateContext();
        var repository = new OrganizationRepository(context, NullLogger<OrganizationRepository>.Instance);
        var org = MockData.CreateOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        org.Name = "Updated Org";
        org.SubscriptionPlan = SubscriptionPlan.Enterprise;
        await repository.UpdateAsync(org);

        var updated = await repository.GetByIdAsync(org.Id);
        updated!.Name.Should().Be("Updated Org");
        updated.SubscriptionPlan.Should().Be(SubscriptionPlan.Enterprise);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveOrganization()
    {
        await using var context = CreateContext();
        var repository = new OrganizationRepository(context, NullLogger<OrganizationRepository>.Instance);
        var org = MockData.CreateOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        await repository.DeleteAsync(org.Id);

        var result = await repository.GetByIdAsync(org.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOwnerIdAsync_ShouldReturnOrganizationsForOwner()
    {
        await using var context = CreateContext();
        var repository = new OrganizationRepository(context, NullLogger<OrganizationRepository>.Instance);
        var ownerId = Guid.NewGuid().ToString();
        context.Organizations.Add(MockData.CreateOrganization(ownerId: ownerId));
        context.Organizations.Add(MockData.CreateOrganization(ownerId: ownerId));
        await context.SaveChangesAsync();

        var results = await repository.GetByOwnerIdAsync(ownerId);

        var organizations = results.ToList();
        organizations.Should().HaveCountGreaterThan(1);
        organizations.Should().AllSatisfy(o => o.OwnerId.Should().Be(ownerId));
    }
}
