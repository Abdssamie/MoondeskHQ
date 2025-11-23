using Clerk.BackendAPI.Helpers.Jwks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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
        // Skip authentication for webhook and health endpoints
        if (context.Request.Path.StartsWithSegments("/api/v1/webhooks") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var options = new AuthenticateRequestOptions(
            secretKey: _secretKey,
            authorizedParties: [""]
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
            
            // Set ClaimsPrincipal for authorization
            var identity = new ClaimsIdentity(requestState.Claims.Claims, "Clerk");
            context.User = new ClaimsPrincipal(identity);
            
            _logger.LogDebug("Authenticated user {UserId} from org {OrgId}", userId, orgId);
            await _next(context);
        }
        else
        {
            _logger.LogWarning("Unauthenticated request to {Path}", context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
        }
    }
}
