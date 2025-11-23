using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Moondesk.API.Hubs;

[Authorize(Policy = "OrgMember")]
public class SensorDataHub : Hub
{
    private readonly ILogger<SensorDataHub> _logger;

    public SensorDataHub(ILogger<SensorDataHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var orgId = Context.User?.FindFirst("org_id")?.Value;
        if (!string.IsNullOrEmpty(orgId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"org_{orgId}");
            _logger.LogInformation("Client {ConnectionId} joined organization group {OrgId}", 
                Context.ConnectionId, orgId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var orgId = Context.User?.FindFirst("org_id")?.Value;
        if (!string.IsNullOrEmpty(orgId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"org_{orgId}");
            _logger.LogInformation("Client {ConnectionId} left organization group {OrgId}", 
                Context.ConnectionId, orgId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
