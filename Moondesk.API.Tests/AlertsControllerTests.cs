using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.API.Tests;

public class AlertsControllerTests
{
    private readonly Mock<IAlertRepository> _mockRepo;
    private readonly AlertsController _controller;
    private const string TestOrgId = "org_test123";
    private const string TestUserId = "user_test123";

    public AlertsControllerTests()
    {
        _mockRepo = new Mock<IAlertRepository>();
        _controller = new AlertsController(_mockRepo.Object);
        
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["UserId"] = TestUserId,
                ["OrganizationId"] = TestOrgId
            }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAll_ReturnsAllAlerts_WhenNoFilter()
    {
        // Arrange
        var alerts = new List<Alert>
        {
            new() { Id = 1, Acknowledged = false, OrganizationId = TestOrgId },
            new() { Id = 2, Acknowledged = true, OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetAlertsAsync()).ReturnsAsync(alerts);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAlerts = Assert.IsType<IEnumerable<Alert>>(okResult.Value, exactMatch: false);
        Assert.Equal(2, returnedAlerts.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsFilteredAlerts_WhenAcknowledgedFilter()
    {
        // Arrange
        var alerts = new List<Alert>
        {
            new() { Id = 1, Acknowledged = false, OrganizationId = TestOrgId },
            new() { Id = 2, Acknowledged = true, OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetAlertsAsync()).ReturnsAsync(alerts);

        // Act
        var result = await _controller.GetAll(acknowledged: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAlerts = Assert.IsType<IEnumerable<Alert>>(okResult.Value, exactMatch: false);
        var collection = returnedAlerts as Alert[] ?? returnedAlerts.ToArray();
        Assert.Single(collection);
        Assert.False(collection.First().Acknowledged);
    }

    [Fact]
    public async Task GetBySensor_ReturnsAlertsForSensor()
    {
        // Arrange
        var alerts = new List<Alert>
        {
            new() { Id = 1, SensorId = 5, OrganizationId = TestOrgId },
            new() { Id = 1, SensorId = 0, OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetAlertsBySensorAsync(5)).ReturnsAsync(alerts.GetRange(0,1));

    // Act
        var result = await _controller.GetBySensor(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAlerts = Assert.IsType<IEnumerable<Alert>>(okResult.Value, exactMatch: false);
        Assert.Single(returnedAlerts);
    }

    [Fact]
    public async Task Acknowledge_UpdatesAlert_WhenExists()
    {
        // Arrange
        var alert = new Alert { Id = 1, Acknowledged = false, OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.GetAlertAsync(1)).ReturnsAsync(alert);
        _mockRepo.Setup(r => r.UpdateAlertAsync(1, It.IsAny<Alert>())).ReturnsAsync(alert);

        // Act
        var result = await _controller.Acknowledge(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.True(alert.Acknowledged);
        Assert.NotNull(alert.AcknowledgedAt);
        Assert.Equal(TestUserId, alert.AcknowledgedBy);
    }

    [Fact]
    public async Task Acknowledge_ReturnsNotFound_WhenAlertDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAlertAsync(999)).ReturnsAsync(null as Alert);

        // Act
        var result = await _controller.Acknowledge(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
