using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NoesisVision.DistShop.OnlineShopping.Infrastructure.Data;

/// <summary>
/// Design-time factory for OnlineShoppingDbContext
/// </summary>
public class OnlineShoppingDbContextFactory : IDesignTimeDbContextFactory<OnlineShoppingDbContext>
{
    public OnlineShoppingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OnlineShoppingDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistShop.OnlineShopping;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new OnlineShoppingDbContext(optionsBuilder.Options);
    }
}