using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;
using NoesisVision.DistShop.Catalog.Infrastructure.Data.Configurations;

namespace NoesisVision.DistShop.Catalog.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the Catalog service
/// </summary>
public class CatalogDbContext : DbContext, IUnitOfWork
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<ProductAggregate> Products { get; set; } = null!;
    public DbSet<CategoryAggregate> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());

        // Set default schema
        modelBuilder.HasDefaultSchema("catalog");
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
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ProductAggregate || e.Entity is CategoryAggregate)
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