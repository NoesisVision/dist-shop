using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Orders.Domain.Aggregates;
using NoesisVision.DistShop.Orders.Infrastructure.Data.Configurations;

namespace NoesisVision.DistShop.Orders.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the Orders service
/// </summary>
public class OrdersDbContext : DbContext, IUnitOfWork
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<OrderAggregate> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new OrderConfiguration());

        // Set default schema
        modelBuilder.HasDefaultSchema("orders");
    }

    public async Task BeginTransactionAsync()
    {
        if (Database.CurrentTransaction == null)
        {
            await Database.BeginTransactionAsync();
        }
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is OrderAggregate)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                // Use reflection to set UpdatedAt if the property exists
                var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                {
                    var field = entry.Entity.GetType().GetField("_updatedAt", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(entry.Entity, DateTime.UtcNow);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}