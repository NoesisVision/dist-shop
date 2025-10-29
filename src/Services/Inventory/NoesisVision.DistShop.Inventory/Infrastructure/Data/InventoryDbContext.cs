using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Inventory.Domain.Aggregates;
using NoesisVision.DistShop.Inventory.Infrastructure.Data.Configurations;

namespace NoesisVision.DistShop.Inventory.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the Inventory service
/// </summary>
public class InventoryDbContext : DbContext, IUnitOfWork
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());

        // Set default schema
        modelBuilder.HasDefaultSchema("inventory");
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction == null)
        {
            await Database.BeginTransactionAsync(cancellationToken);
        }
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        var entries = ChangeTracker.Entries<InventoryItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                // Use reflection to set LastUpdated if needed
                var lastUpdatedProperty = entry.Entity.GetType().GetProperty("LastUpdated");
                if (lastUpdatedProperty != null && lastUpdatedProperty.CanWrite)
                {
                    var field = entry.Entity.GetType().GetField("_lastUpdated", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(entry.Entity, DateTime.UtcNow);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}