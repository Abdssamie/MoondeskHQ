using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models;

namespace Moondesk.API.Tests;

public class OrganizationsControllerTests
{
    private readonly Mock<IOrganizationRepository> _mockRepo;
    private readonly OrganizationsController _controller;
    private const string TestOrgId = "org_test123";

    public OrganizationsControllerTests()
    {
        _mockRepo = new Mock<IOrganizationRepository>();
        _controller = new OrganizationsController(_mockRepo.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Items["OrganizationId"] = TestOrgId;
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task GetCurrent_ReturnsOrganization_WhenExists()
    {
        // Arrange
        var org = new Organization 
        { 
            Id = TestOrgId, 
            Name = "Test Organization",
            OwnerId = "owner123"
        };
        _mockRepo.Setup(r => r.GetByIdAsync(TestOrgId)).ReturnsAsync(org);

        // Act
        var result = await _controller.GetCurrent();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedOrg = Assert.IsType<Organization>(okResult.Value);
        Assert.Equal(TestOrgId, returnedOrg.Id);
        Assert.Equal("Test Organization", returnedOrg.Name);
    }

    [Fact]
    public async Task GetCurrent_ReturnsNotFound_WhenDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(TestOrgId)).ReturnsAsync((Organization?)null);

        // Act
        var result = await _controller.GetCurrent();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetCurrent_ReturnsUnauthorized_WhenNoOrganization()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.Items["OrganizationId"] = null;

        // Act
        var result = await _controller.GetCurrent();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
