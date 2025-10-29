using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Pricing.Domain.Aggregates;
using NoesisVision.DistShop.Pricing.Infrastructure.Data.Configurations;

namespace NoesisVision.DistShop.Pricing.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the Pricing service
/// </summary>
public class PricingDbContext : DbContext, IUnitOfWork
{
    private readonly IEventBus _eventBus;

    public PricingDbContext(DbContextOptions<PricingDbContext> options, IEventBus eventBus) 
        : base(options)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public DbSet<PricingRuleAggregate> PricingRules { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new PricingRuleConfiguration());
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events before saving
        var domainEvents = ChangeTracker
            .Entries<PricingRuleAggregate>()
            .Where(x => x.Entity.DomainEvents.Any())
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _eventBus.PublishAsync(domainEvent);
        }

        // Clear domain events
        foreach (var entry in ChangeTracker.Entries<PricingRuleAggregate>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    public async Task BeginTransactionAsync()
    {
        await Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.CurrentTransaction.CommitAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.CurrentTransaction.RollbackAsync();
        }
    }
}