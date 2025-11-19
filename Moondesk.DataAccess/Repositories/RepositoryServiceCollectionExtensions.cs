using Microsoft.Extensions.DependencyInjection;
using Moondesk.Domain.Interfaces.Repositories;

namespace Moondesk.DataAccess.Repositories;

/// <summary>
/// Extension methods for registering repository services with dependency injection.
/// Follows modern .NET service registration patterns with proper lifetimes.
/// </summary>
public static class RepositoryServiceCollectionExtensions
{
    /// <summary>
    /// Registers all repository implementations with the DI container.
    /// Uses scoped lifetime for proper DbContext management and transaction support.
    /// </summary>
    /// <param name="services">The service collection to register repositories with</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Core entity repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationMembershipRepository, OrganizationMembershipRepository>();
        
        // Network and security repositories
        services.AddScoped<IConnectionCredentialRepository, ConnectionCredentialRepository>();
        
        // IoT data repositories
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IReadingRepository, ReadingRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<ICommandRepository, CommandRepository>();

        return services;
    }
}
