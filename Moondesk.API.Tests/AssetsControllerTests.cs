using System.Security.Claims;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;
using Moondesk.Domain.Models;
using Moondesk.Domain.Enums;

namespace Moondesk.API.Tests;

public class AssetsControllerTests
{
    private readonly Mock<IAssetRepository> _mockRepo;
    private readonly AssetsController _controller;
    private const string TestOrgId = "org_test123";
    private const string TestUserId = "user_test123";

    public AssetsControllerTests()
    {
        _mockRepo = new Mock<IAssetRepository>();
        _controller = new AssetsController(_mockRepo.Object);
        
        // Simulate authenticated request with claims
        var claims = new List<Claim>
        {
            new Claim("sub", TestUserId),
            new Claim("org_id", TestOrgId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        httpContext.Items["UserId"] = TestUserId;
        httpContext.Items["OrganizationId"] = TestOrgId;
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAll_ReturnsAssets_WhenOrganizationExists()
    {
        // Arrange
        var assets = new List<Asset>
        {
            new() { Id = 1, Name = "Pump 1", OrganizationId = TestOrgId },
            new() { Id = 2, Name = "Pump 2", OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(assets);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAssets = Assert.IsAssignableFrom<IEnumerable<Asset>>(okResult.Value);
        Assert.Equal(2, returnedAssets.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WhenNoOrganization()
    {
        // Arrange
        _controller.HttpContext.Items["OrganizationId"] = null;

        // Act
        var result = await _controller.GetAll();

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsAsset_WhenExists()
    {
        // Arrange
        var asset = new Asset { Id = 1, Name = "Pump 1", OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(asset);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var returnedAsset = Assert.IsType<Asset>(okResult.Value);
        Assert.Equal("Pump 1", returnedAsset.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Asset?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAsset()
    {
        // Arrange
        var newAsset = new Asset { Name = "New Pump", OrganizationId = "" };
        var createdAsset = new Asset { Id = 1, Name = "New Pump", OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Asset>())).ReturnsAsync(createdAsset);

        // Act
        var result = await _controller.Create(newAsset);

        // Assert
        var createdResult = Assert.IsType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>(result);
        var returnedAsset = Assert.IsType<Asset>(createdResult.Value);
        Assert.Equal(TestOrgId, returnedAsset.OrganizationId);
        _mockRepo.Verify(r => r.AddAsync(It.Is<Asset>(a => a.OrganizationId == TestOrgId)), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var existingAsset = new Asset { Id = 1, Name = "Old Name", OrganizationId = TestOrgId };
        var updatedAsset = new Asset { Id = 1, Name = "New Name", OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingAsset);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Asset>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updatedAsset);

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Asset>(a => a.OrganizationId == TestOrgId)), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var asset = new Asset { Id = 1, Name = "Pump", OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(asset);
        _mockRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}
