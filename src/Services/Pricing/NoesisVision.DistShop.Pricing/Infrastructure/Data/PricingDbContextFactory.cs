using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Pricing.Infrastructure.Data;

/// <summary>
/// Design-time factory for PricingDbContext (used by EF migrations)
/// </summary>
public class PricingDbContextFactory : IDesignTimeDbContextFactory<PricingDbContext>
{
    public PricingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PricingDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistShop.Pricing;Trusted_Connection=true;MultipleActiveResultSets=true");

        // Create a dummy event bus for design-time
        var dummyEventBus = new InMemoryEventBus();
        
        return new PricingDbContext(optionsBuilder.Options, dummyEventBus);
    }
}