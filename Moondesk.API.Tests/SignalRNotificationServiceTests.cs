using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Moondesk.API.Hubs;
using Moondesk.BackgroundServices.Services;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;
using Xunit;

namespace Moondesk.API.Tests;

public class SignalRNotificationServiceTests
{
    private readonly Mock<IHubContext<SensorDataHub>> _hubContext;
    private readonly Mock<ILogger<SignalRNotificationService>> _logger;
    private readonly Mock<IClientProxy> _clientProxy;
    private readonly SignalRNotificationService _service;

    public SignalRNotificationServiceTests()
    {
        _hubContext = new Mock<IHubContext<SensorDataHub>>();
        _logger = new Mock<ILogger<SignalRNotificationService>>();
        _clientProxy = new Mock<IClientProxy>();

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxy.Object);
        _hubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _service = new SignalRNotificationService(_hubContext.Object, _logger.Object);
    }

    [Fact]
    public async Task SendAlertNotificationAsync_SendsToCorrectGroup()
    {
        // Arrange
        var alert = new Alert
        {
            Id = 1,
            SensorId = 123,
            OrganizationId = "org_test123",
            Severity = AlertSeverity.Critical,
            Message = "Test alert",
            Timestamp = DateTimeOffset.UtcNow,
            TriggerValue = 10.5,
            ThresholdValue = 8.5
        };
        var orgId = "org_test123";

        // Act
        await _service.SendAlertNotificationAsync(alert, orgId);

        // Assert
        _hubContext.Verify(h => h.Clients.Group($"org_{orgId}"), Times.Once);
        _clientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveAlert",
                It.Is<object[]>(o => o.Length == 1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendSensorReadingAsync_SendsToCorrectGroup()
    {
        // Arrange
        var reading = new Reading
        {
            SensorId = 123,
            OrganizationId = "org_test123",
            Timestamp = DateTimeOffset.UtcNow,
            Value = 7.2,
            Quality = ReadingQuality.Good
        };
        var orgId = "org_test123";

        // Act
        await _service.SendSensorReadingAsync(reading, orgId);

        // Assert
        _hubContext.Verify(h => h.Clients.Group($"org_{orgId}"), Times.Once);
        _clientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveReading",
                It.Is<object[]>(o => o.Length == 1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_LogsNotification()
    {
        // Act
        await _service.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

        // Assert
        _logger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email notification")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_LogsNotification()
    {
        // Act
        await _service.SendSmsAsync("+1234567890", "Test message");

        // Assert
        _logger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SMS notification")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
