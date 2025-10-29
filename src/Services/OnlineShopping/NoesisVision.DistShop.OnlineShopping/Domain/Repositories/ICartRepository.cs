using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.OnlineShopping.Domain.Aggregates;

namespace NoesisVision.DistShop.OnlineShopping.Domain.Repositories;

/// <summary>
/// Repository interface for cart aggregate operations
/// </summary>
public interface ICartRepository : IRepository<CartAggregate>
{
    /// <summary>
    /// Gets a cart by customer ID
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>The cart if found, null otherwise</returns>
    Task<CartAggregate?> GetByCustomerIdAsync(Guid customerId);
    
    /// <summary>
    /// Gets or creates a cart for a customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="currency">The currency for new cart</param>
    /// <returns>Existing or new cart</returns>
    Task<CartAggregate> GetOrCreateByCustomerIdAsync(Guid customerId, string currency = "USD");
    
    /// <summary>
    /// Gets expired carts for cleanup
    /// </summary>
    /// <param name="expirationPeriod">The expiration period</param>
    /// <returns>Collection of expired carts</returns>
    Task<IEnumerable<CartAggregate>> GetExpiredCartsAsync(TimeSpan expirationPeriod);
    
    /// <summary>
    /// Deletes expired carts
    /// </summary>
    /// <param name="expirationPeriod">The expiration period</param>
    /// <returns>Number of deleted carts</returns>
    Task<int> DeleteExpiredCartsAsync(TimeSpan expirationPeriod);
}