using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Inventory.Domain.Aggregates;

namespace NoesisVision.DistShop.Inventory.Domain.Repositories;

/// <summary>
/// Repository interface for inventory item operations
/// </summary>
public interface IInventoryRepository : IRepository<InventoryItem>
{
    /// <summary>
    /// Gets inventory item by product ID
    /// </summary>
    /// <param name="productId">Product ID to search for</param>
    /// <returns>Inventory item if found, null otherwise</returns>
    Task<InventoryItem?> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Gets all inventory items with low stock (at or below reorder level)
    /// </summary>
    /// <returns>Collection of inventory items with low stock</returns>
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();

    /// <summary>
    /// Gets inventory items for multiple product IDs
    /// </summary>
    /// <param name="productIds">Collection of product IDs</param>
    /// <returns>Collection of inventory items</returns>
    Task<IEnumerable<InventoryItem>> GetByProductIdsAsync(IEnumerable<Guid> productIds);
}