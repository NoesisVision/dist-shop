using Microsoft.Extensions.DependencyInjection;
using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.SharedKernel.Extensions;

/// <summary>
/// Extension methods for configuring shared kernel services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds shared kernel services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        // Register the in-memory event bus as a singleton
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        
        return services;
    }
}