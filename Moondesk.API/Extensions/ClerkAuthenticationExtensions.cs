using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Moondesk.API.Middleware;

namespace Moondesk.API.Extensions;

public static class ClerkAuthenticationExtensions
{
    public static IApplicationBuilder UseClerkAuthentication(this IApplicationBuilder app, IConfiguration configuration)
    {
        var secretKey = configuration["Clerk:SecretKey"] 
            ?? throw new InvalidOperationException("Clerk:SecretKey not configured");
            
        return app.UseMiddleware<ClerkAuthenticationMiddleware>(secretKey);
    }
}
