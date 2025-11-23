using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moondesk.BackgroundServices.Services;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Interfaces.Services;
using Moondesk.Domain.Models.IoT;
using Xunit;

namespace Moondesk.BackgroundServices.Tests;

public class MqttIngestionServiceTests
{
    private readonly Mock<ILogger<MqttIngestionService>> _logger;
    private readonly Mock<IConfiguration> _config;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IServiceScope> _serviceScope;
    private readonly Mock<IReadingRepository> _readingRepo;
    private readonly Mock<ISensorRepository> _sensorRepo;
    private readonly Mock<IAlertRepository> _alertRepo;
    private readonly Mock<INotificationService> _notificationService;

    public MqttIngestionServiceTests()
    {
        _logger = new Mock<ILogger<MqttIngestionService>>();
        _config = new Mock<IConfiguration>();
        _serviceProvider = new Mock<IServiceProvider>();
        _serviceScope = new Mock<IServiceScope>();
        _readingRepo = new Mock<IReadingRepository>();
        _sensorRepo = new Mock<ISensorRepository>();
        _alertRepo = new Mock<IAlertRepository>();
        _notificationService = new Mock<INotificationService>();

        // Setup configuration
        _config.Setup(c => c["MQTT:Host"]).Returns("localhost");
        _config.Setup(c => c["MQTT:Port"]).Returns("1883");
        _config.Setup(c => c["MQTT:Username"]).Returns("");
        _config.Setup(c => c["MQTT:Password"]).Returns("");

        // Setup service provider
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(_serviceScope.Object);
        _serviceScope.Setup(s => s.ServiceProvider).Returns(_serviceProvider.Object);
        
        _serviceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory.Object);
        _serviceProvider.Setup(p => p.GetService(typeof(IReadingRepository)))
            .Returns(_readingRepo.Object);
        _serviceProvider.Setup(p => p.GetService(typeof(ISensorRepository)))
            .Returns(_sensorRepo.Object);
        _serviceProvider.Setup(p => p.GetService(typeof(IAlertRepository)))
            .Returns(_alertRepo.Object);
        _serviceProvider.Setup(p => p.GetService(typeof(INotificationService)))
            .Returns(_notificationService.Object);
    }

    [Fact]
    public void Constructor_InitializesWithConfiguration()
    {
        // Act
        var service = new MqttIngestionService(_logger.Object, _config.Object, _serviceProvider.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void DetermineSeverity_ReturnsEmergency_WhenOver50Percent()
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "Test Sensor",
            OrganizationId = "org_test",
            ThresholdHigh = 10.0,
            ThresholdLow = 0.0
        };
        var value = 16.0; // 60% over threshold

        // Act
        var service = new MqttIngestionService(_logger.Object, _config.Object, _serviceProvider.Object);
        var severity = InvokeDetermineSeverity(service, value, sensor);

        // Assert
        Assert.Equal(AlertSeverity.Emergency, severity);
    }

    [Fact]
    public void DetermineSeverity_ReturnsCritical_WhenOver25Percent()
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "Test Sensor",
            OrganizationId = "org_test",
            ThresholdHigh = 10.0,
            ThresholdLow = 0.0
        };
        var value = 13.0; // 30% over threshold

        // Act
        var service = new MqttIngestionService(_logger.Object, _config.Object, _serviceProvider.Object);
        var severity = InvokeDetermineSeverity(service, value, sensor);

        // Assert
        Assert.Equal(AlertSeverity.Critical, severity);
    }

    [Fact]
    public void DetermineSeverity_ReturnsWarning_WhenOver10Percent()
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "Test Sensor",
            OrganizationId = "org_test",
            ThresholdHigh = 10.0,
            ThresholdLow = 0.0
        };
        var value = 11.5; // 15% over threshold

        // Act
        var service = new MqttIngestionService(_logger.Object, _config.Object, _serviceProvider.Object);
        var severity = InvokeDetermineSeverity(service, value, sensor);

        // Assert
        Assert.Equal(AlertSeverity.Warning, severity);
    }

    [Fact]
    public void DetermineSeverity_ReturnsInfo_WhenUnder10Percent()
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "Test Sensor",
            OrganizationId = "org_test",
            ThresholdHigh = 10.0,
            ThresholdLow = 0.0
        };
        var value = 10.5; // 5% over threshold

        // Act
        var service = new MqttIngestionService(_logger.Object, _config.Object, _serviceProvider.Object);
        var severity = InvokeDetermineSeverity(service, value, sensor);

        // Assert
        Assert.Equal(AlertSeverity.Info, severity);
    }

    // Helper method to invoke private DetermineSeverity method via reflection
    private AlertSeverity InvokeDetermineSeverity(MqttIngestionService service, double value, Sensor sensor)
    {
        var method = typeof(MqttIngestionService).GetMethod(
            "DetermineSeverity",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        return (AlertSeverity)method!.Invoke(service, new object[] { value, sensor })!;
    }
}
