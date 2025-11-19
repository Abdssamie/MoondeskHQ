using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class OrganizationMembershipRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>
{
    private readonly TimescaleDbTestContainerFixture _fixture;

    public OrganizationMembershipRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(MoondeskDbContext context, string userId, string orgId)> SetupTestDataAsync()
    {
        var context = CreateContext();
        
        var user = MockData.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var org = MockData.CreateOrganization();
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        
        return (context, user.Id, org.Id);
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
    public async Task AddAsync_ShouldCreateMembership()
    {
        var (context, userId, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new OrganizationMembershipRepository(context, NullLogger<OrganizationMembershipRepository>.Instance);
            var membership = MockData.CreateMembership(userId, orgId);

            var result = await repository.CreateAsync(membership);

            result.UserId.Should().Be(userId);
            result.OrganizationId.Should().Be(orgId);
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnMembershipsForUser()
    {
        var (context, userId, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new OrganizationMembershipRepository(context, NullLogger<OrganizationMembershipRepository>.Instance);
            var membership = MockData.CreateMembership(userId, orgId);
            context.OrganizationMemberships.Add(membership);
            await context.SaveChangesAsync();

            var results = await repository.GetByUserIdAsync(userId);

            var organizationMemberships = results.ToList();
            organizationMemberships.Should().HaveCountGreaterThan(0);
            organizationMemberships.Should().AllSatisfy(m => m.UserId.Should().Be(userId));
        }
    }

    [Fact]
    public async Task GetByOrganizationIdAsync_ShouldReturnMembershipsForOrganization()
    {
        var (context, userId, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new OrganizationMembershipRepository(context, NullLogger<OrganizationMembershipRepository>.Instance);
            var membership = MockData.CreateMembership(userId, orgId);
            context.OrganizationMemberships.Add(membership);
            await context.SaveChangesAsync();

            var results = await repository.GetByOrganizationIdAsync(orgId);

            var organizationMemberships = results.ToList();
            organizationMemberships.Should().HaveCountGreaterThan(0);
            organizationMemberships.Should().AllSatisfy(m => m.OrganizationId.Should().Be(orgId));
        }
    }

    [Fact]
    public async Task GetByUserAndOrganizationAsync_ShouldReturnMembership_WhenExists()
    {
        var (context, userId, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new OrganizationMembershipRepository(context, NullLogger<OrganizationMembershipRepository>.Instance);
            var membership = MockData.CreateMembership(userId, orgId);
            context.OrganizationMemberships.Add(membership);
            await context.SaveChangesAsync();

            var result = await repository.GetByOrganizationIdAsync(orgId);

            var organizationMemberships = result.ToList();
            organizationMemberships.Should().NotBeNull();
            organizationMemberships.FirstOrDefault()!.UserId.Should().Be(userId);
            organizationMemberships.FirstOrDefault()!.OrganizationId.Should().Be(orgId);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyMembership()
    {
        var (context, userId, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new OrganizationMembershipRepository(context, NullLogger<OrganizationMembershipRepository>.Instance);
            var membership = MockData.CreateMembership(userId, orgId);
            context.OrganizationMemberships.Add(membership);
            await context.SaveChangesAsync();

            membership.Role = UserRole.Admin;
            await repository.UpdateAsync(membership);

            var updated = await repository.GetByOrganizationIdAsync(orgId);
            updated.FirstOrDefault()!.Role.Should().Be(UserRole.Admin);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveMembership()
    {
        var (context, userId, orgId) = await SetupTestDataAsync();
        await using (context)
        {
            var repository = new OrganizationMembershipRepository(context, NullLogger<OrganizationMembershipRepository>.Instance);
            var membership = MockData.CreateMembership(userId, orgId);
            context.OrganizationMemberships.Add(membership);
            await context.SaveChangesAsync();

            await repository.DeleteAsync(userId, orgId);

            var result = await repository.GetByOrganizationIdAsync(orgId);
            result.Should().BeEmpty();
        }
    }
}
