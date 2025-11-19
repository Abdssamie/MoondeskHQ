using Clerk.BackendAPI.Helpers.Jwks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Moondesk.API.Middleware;

public class ClerkAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClerkAuthenticationMiddleware> _logger;
    private readonly string _secretKey;

    public ClerkAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ClerkAuthenticationMiddleware> logger,
        string secretKey)
    {
        _next = next;
        _logger = logger;
        _secretKey = secretKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var options = new AuthenticateRequestOptions(
            secretKey: _secretKey,
            authorizedParties: new[] { "" }
        );

        var requestState = await AuthenticateRequest.AuthenticateRequestAsync(
            context.Request,
            options
        );

        if (requestState is { IsAuthenticated: true, Claims: not null })
        {
            var userId = requestState.Claims.FindFirst("sub")?.Value;
            var orgId = requestState.Claims.FindFirst("org_id")?.Value;
            
            context.Items["UserId"] = userId;
            context.Items["OrganizationId"] = orgId;
            
            _logger.LogDebug("Authenticated user {UserId} from org {OrgId}", userId, orgId);
        }

        await _next(context);
    }
}
