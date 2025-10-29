using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.OnlineShopping.Domain.Aggregates;
using NoesisVision.DistShop.OnlineShopping.Domain.ValueObjects;
using NoesisVision.DistShop.OnlineShopping.Infrastructure.Data.Configurations;

namespace NoesisVision.DistShop.OnlineShopping.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for Online Shopping Service
/// </summary>
public class OnlineShoppingDbContext : DbContext
{
    public OnlineShoppingDbContext(DbContextOptions<OnlineShoppingDbContext> options) : base(options)
    {
    }

    public DbSet<CartAggregate> Carts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        
        // Set default schema
        modelBuilder.HasDefaultSchema("OnlineShopping");
    }
}