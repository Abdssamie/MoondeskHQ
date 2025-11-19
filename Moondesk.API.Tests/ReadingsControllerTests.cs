using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.API.Tests;

public class ReadingsControllerTests
{
    private readonly Mock<IReadingRepository> _mockRepo;
    private readonly ReadingsController _controller;
    private const string TestOrgId = "org_test123";

    public ReadingsControllerTests()
    {
        _mockRepo = new Mock<IReadingRepository>();
        _controller = new ReadingsController(_mockRepo.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Items["OrganizationId"] = TestOrgId;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetReadings_ReturnsRecentReadings_WithDefaultLimit()
    {
        // Arrange
        var readings = new List<Reading>
        {
            new() { SensorId = 1, Value = 25.5, OrganizationId = TestOrgId },
            new() { SensorId = 1, Value = 26.0, OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetRecentReadingsAsync(TestOrgId, 1, 100)).ReturnsAsync(readings);

        // Act
        var result = await _controller.GetReadings(sensor_id: 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReadings = Assert.IsAssignableFrom<IEnumerable<Reading>>(okResult.Value);
        Assert.Equal(2, returnedReadings.Count());
    }

    [Fact]
    public async Task GetReadings_ReturnsRecentReadings_WithCustomLimit()
    {
        // Arrange
        var readings = new List<Reading>
        {
            new() { SensorId = 1, Value = 25.5, OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetRecentReadingsAsync(TestOrgId, 1, 50)).ReturnsAsync(readings);

        // Act
        var result = await _controller.GetReadings(sensor_id: 1, limit: 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockRepo.Verify(r => r.GetRecentReadingsAsync(TestOrgId, 1, 50), Times.Once);
    }

    [Fact]
    public async Task GetBySensor_ReturnsAllReadingsForSensor()
    {
        // Arrange
        var readings = new List<Reading>
        {
            new() { SensorId = 5, Value = 25.5, OrganizationId = TestOrgId },
            new() { SensorId = 5, Value = 26.0, OrganizationId = TestOrgId },
            new() { SensorId = 5, Value = 26.5, OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetReadingsBySensorAsync(5)).ReturnsAsync(readings);

        // Act
        var result = await _controller.GetBySensor(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReadings = Assert.IsAssignableFrom<IEnumerable<Reading>>(okResult.Value);
        Assert.Equal(3, returnedReadings.Count());
    }

    [Fact]
    public async Task GetReadings_ReturnsUnauthorized_WhenNoOrganization()
    {
        // Arrange
        _controller.HttpContext.Items["OrganizationId"] = null;

        // Act
        var result = await _controller.GetReadings(sensor_id: 1);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
