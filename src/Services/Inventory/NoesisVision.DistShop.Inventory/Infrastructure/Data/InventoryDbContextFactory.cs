using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NoesisVision.DistShop.Inventory.Infrastructure.Data;

/// <summary>
/// Factory for creating InventoryDbContext instances during design-time operations like migrations
/// </summary>
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        
        // Use a default connection string for migrations
        // In production, this would come from configuration
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistShop.Inventory;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new InventoryDbContext(optionsBuilder.Options);
    }
}