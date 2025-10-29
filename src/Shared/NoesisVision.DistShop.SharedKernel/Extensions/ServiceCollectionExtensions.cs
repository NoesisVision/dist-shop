using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;

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
    
    /// <summary>
    /// Adds shared kernel services with DbContext support
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSharedKernel<TDbContext>(this IServiceCollection services) 
        where TDbContext : DbContext
    {
        // Register the in-memory event bus as a singleton
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        
        // Register Unit of Work with the specific DbContext
        services.AddScoped<IUnitOfWork>(provider => 
            new UnitOfWork<TDbContext>(provider.GetRequiredService<TDbContext>()));
        
        return services;
    }
}