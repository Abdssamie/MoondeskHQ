using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moondesk.API.Controllers;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.API.Tests;

public class SensorsControllerTests
{
    private readonly Mock<ISensorRepository> _mockRepo;
    private readonly SensorsController _controller;
    private const string TestOrgId = "org_test123";

    public SensorsControllerTests()
    {
        _mockRepo = new Mock<ISensorRepository>();
        _controller = new SensorsController(_mockRepo.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Items["OrganizationId"] = TestOrgId;
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task GetAll_ReturnsAllSensors_WhenNoFilter()
    {
        // Arrange
        var sensors = new List<Sensor>
        {
            new() { Id = 1, Name = "Temp Sensor", OrganizationId = TestOrgId },
            new() { Id = 2, Name = "Pressure Sensor", OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sensors);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSensors = Assert.IsAssignableFrom<IEnumerable<Sensor>>(okResult.Value);
        Assert.Equal(2, returnedSensors.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsFilteredSensors_WhenAssetIdProvided()
    {
        // Arrange
        var sensors = new List<Sensor>
        {
            new() { Id = 1, AssetId = 5, Name = "Temp Sensor", OrganizationId = TestOrgId }
        };
        _mockRepo.Setup(r => r.GetByAssetIdAsync(5)).ReturnsAsync(sensors);

        // Act
        var result = await _controller.GetAll(asset_id: 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSensors = Assert.IsAssignableFrom<IEnumerable<Sensor>>(okResult.Value);
        Assert.Single(returnedSensors);
    }

    [Fact]
    public async Task Create_SetsSensorOrganizationId()
    {
        // Arrange
        var sensor = new Sensor { Name = "New Sensor", OrganizationId = "" };
        var created = new Sensor { Id = 1, Name = "New Sensor", OrganizationId = TestOrgId };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Sensor>())).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(sensor);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        _mockRepo.Verify(r => r.AddAsync(It.Is<Sensor>(s => s.OrganizationId == TestOrgId)), Times.Once);
    }
}
