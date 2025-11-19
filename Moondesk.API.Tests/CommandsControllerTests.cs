using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.API.Tests;

public class CommandsControllerTests
{
    private readonly Mock<ICommandRepository> _mockRepo;
    private readonly CommandsController _controller;
    private const string TestOrgId = "org_test123";
    private const string TestUserId = "user_test123";

    public CommandsControllerTests()
    {
        _mockRepo = new Mock<ICommandRepository>();
        _controller = new CommandsController(_mockRepo.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = TestUserId;
        httpContext.Items["OrganizationId"] = TestOrgId;
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task GetPending_ReturnsPendingCommands()
    {
        // Arrange
        var commands = new List<Command>
        {
            new() { Id = 1, UserId = TestUserId, OrganizationId = TestOrgId, CommandType = "SET_THRESHOLD" }
        };
        _mockRepo.Setup(r => r.GetPendingCommandsAsync(TestOrgId)).ReturnsAsync(commands);

        // Act
        var result = await _controller.GetPending();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCommands = Assert.IsAssignableFrom<IEnumerable<Command>>(okResult.Value);
        Assert.Single(returnedCommands);
    }

    [Fact]
    public async Task GetBySensor_ReturnsCommandsForSensor()
    {
        // Arrange
        var commands = new List<Command>
        {
            new() { Id = 1, SensorId = 5, UserId = TestUserId, OrganizationId = TestOrgId, CommandType = "CALIBRATE" }
        };
        _mockRepo.Setup(r => r.GetBySensorIdAsync(5)).ReturnsAsync(commands);

        // Act
        var result = await _controller.GetBySensor(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCommands = Assert.IsAssignableFrom<IEnumerable<Command>>(okResult.Value);
        Assert.Single(returnedCommands);
    }

    [Fact]
    public async Task Create_SetsOrganizationAndUserId()
    {
        // Arrange
        var command = new Command { SensorId = 1, CommandType = "TURN_ON", UserId = "", OrganizationId = "" };
        var created = new Command { Id = 1, SensorId = 1, CommandType = "TURN_ON", UserId = TestUserId, OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Command>())).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        _mockRepo.Verify(r => r.AddAsync(It.Is<Command>(c => 
            c.OrganizationId == TestOrgId && c.UserId == TestUserId)), Times.Once);
    }
}
