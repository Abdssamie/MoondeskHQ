using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Moondesk.API.Extensions;
using Moondesk.DataAccess.Configuration;
using Moondesk.DataAccess.Repositories;
using Serilog;

// Configure logging first
var isDevelopment = args.Contains("--environment=Development") || 
                   Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

if (isDevelopment)
{
    LoggingConfiguration.ConfigureDevelopmentLogging();
}
else
{
    LoggingConfiguration.ConfigureProductionLogging();
}

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add services
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Register DataAccess layer
    builder.Services.AddMoondeskDataAccess(connectionString);
    builder.Services.AddRepositories();

    // Add API services
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(Moondesk.API.Controllers.BaseApiController).Assembly)
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        });
    
    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddMvc().AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.EnableAnnotations();
    });
    
    // Add health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "database");
    
    // Add rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                }));
        
        options.AddPolicy("webhook", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 50,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });
    
    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
        Log.Information("Development environment detected - Swagger enabled");
    }

    // Initialize database (skip TimescaleDB for now)
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Moondesk.DataAccess.Data.MoondeskDbContext>();
        await context.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database initialization failed");
    }

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        await next();
    });

    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseCors();
    app.UseClerkAuthentication(app.Configuration);
    app.UseRouting();
    app.UseAuthorization();
    
    // Map controllers and health endpoint
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Moondesk Host application starting on {Environment}...", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
