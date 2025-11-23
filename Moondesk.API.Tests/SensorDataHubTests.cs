using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Moondesk.API.Hubs;
using Xunit;

namespace Moondesk.API.Tests;

public class SensorDataHubTests
{
    private readonly Mock<ILogger<SensorDataHub>> _logger;
    private readonly SensorDataHub _hub;
    private readonly Mock<HubCallerContext> _context;
    private readonly Mock<IGroupManager> _groups;

    public SensorDataHubTests()
    {
        _logger = new Mock<ILogger<SensorDataHub>>();
        _hub = new SensorDataHub(_logger.Object);
        _context = new Mock<HubCallerContext>();
        _groups = new Mock<IGroupManager>();

        _hub.Context = _context.Object;
        _hub.Groups = _groups.Object;
    }

    [Fact]
    public async Task OnConnectedAsync_AddsClientToOrganizationGroup()
    {
        // Arrange
        var orgId = "org_test123";
        var connectionId = "connection_123";
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", orgId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _context.Setup(c => c.User).Returns(principal);
        _context.Setup(c => c.ConnectionId).Returns(connectionId);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _groups.Verify(g => g.AddToGroupAsync(connectionId, $"org_{orgId}", default), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_DoesNotAddToGroup_WhenNoOrgId()
    {
        // Arrange
        var connectionId = "connection_123";
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _context.Setup(c => c.User).Returns(principal);
        _context.Setup(c => c.ConnectionId).Returns(connectionId);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _groups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task OnDisconnectedAsync_RemovesClientFromOrganizationGroup()
    {
        // Arrange
        var orgId = "org_test123";
        var connectionId = "connection_123";
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123"),
            new Claim("org_id", orgId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _context.Setup(c => c.User).Returns(principal);
        _context.Setup(c => c.ConnectionId).Returns(connectionId);

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _groups.Verify(g => g.RemoveFromGroupAsync(connectionId, $"org_{orgId}", default), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_DoesNotRemoveFromGroup_WhenNoOrgId()
    {
        // Arrange
        var connectionId = "connection_123";
        var claims = new List<Claim>
        {
            new Claim("sub", "user_123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _context.Setup(c => c.User).Returns(principal);
        _context.Setup(c => c.ConnectionId).Returns(connectionId);

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _groups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }
}
