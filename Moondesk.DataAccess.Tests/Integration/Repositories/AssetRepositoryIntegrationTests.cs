using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moondesk.DataAccess.Data;
using Moondesk.DataAccess.Repositories;
using Moondesk.DataAccess.Tests.Fixtures;
using Moondesk.Domain.Enums;

namespace Moondesk.DataAccess.Tests.Integration.Repositories;

public class AssetRepositoryIntegrationTests : IClassFixture<TimescaleDbTestContainerFixture>, IAsyncLifetime
{
    private readonly TimescaleDbTestContainerFixture _fixture;
    private MoondeskDbContext _context = null!;
    private string _orgId = null!;

    public AssetRepositoryIntegrationTests(TimescaleDbTestContainerFixture fixture)
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
    public async Task GetByIdAsync_ShouldReturnAsset_WhenExists()
    {
        var repository = new AssetRepository(_context, NullLogger<AssetRepository>.Instance);
        var asset = MockData.CreateAsset(organizationId: _orgId);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(asset.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(asset.Id);
        result.Name.Should().Be(asset.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var repository = new AssetRepository(_context, NullLogger<AssetRepository>.Instance);

        var result = await repository.GetByIdAsync(99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldCreateAsset()
    {
        var repository = new AssetRepository(_context, NullLogger<AssetRepository>.Instance);
        var asset = MockData.CreateAsset(organizationId: _orgId);

        var result = await repository.AddAsync(asset);

        result.Id.Should().BeGreaterThan(0);
        result.OrganizationId.Should().Be(_orgId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyAsset()
    {
        var repository = new AssetRepository(_context, NullLogger<AssetRepository>.Instance);
        var asset = MockData.CreateAsset(organizationId: _orgId);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        
        asset.Name = "Updated Asset";
        asset.Status = AssetStatus.Offline;
        await repository.UpdateAsync(asset);

        var updated = await repository.GetByIdAsync(asset.Id);
        updated!.Name.Should().Be("Updated Asset");
        updated.Status.Should().Be(AssetStatus.Offline);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveAsset()
    {
        var repository = new AssetRepository(_context, NullLogger<AssetRepository>.Instance);
        var asset = MockData.CreateAsset(organizationId: _orgId);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        await repository.DeleteAsync(asset.Id);

        var result = await repository.GetByIdAsync(asset.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAssets()
    {
        var repository = new AssetRepository(_context, NullLogger<AssetRepository>.Instance);
        _context.Assets.Add(MockData.CreateAsset(organizationId: _orgId));
        _context.Assets.Add(MockData.CreateAsset(organizationId: _orgId));
        await _context.SaveChangesAsync();

        var results = await repository.GetAllAsync();

        results.Should().HaveCountGreaterThan(1);
    }
}
