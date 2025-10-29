using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NoesisVision.DistShop.Catalog.Infrastructure.Data;

/// <summary>
/// Design-time factory for CatalogDbContext to support EF migrations
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        
        // Use a default connection string for migrations
        // In production, this would come from configuration
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistShop.Catalog;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new CatalogDbContext(optionsBuilder.Options);
    }
}