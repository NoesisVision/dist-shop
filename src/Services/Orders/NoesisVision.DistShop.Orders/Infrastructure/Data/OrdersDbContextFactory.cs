using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NoesisVision.DistShop.Orders.Infrastructure.Data;

/// <summary>
/// Factory for creating OrdersDbContext instances during design time (migrations)
/// </summary>
public class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();
        
        // Use a default connection string for migrations
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistShop.Orders;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new OrdersDbContext(optionsBuilder.Options);
    }
}