using NoesisVision.DistShop.Inventory.Domain.Aggregates;
using NoesisVision.DistShop.Inventory.Domain.ValueObjects;

namespace NoesisVision.DistShop.Inventory.Application.Services;

/// <summary>
/// Application service interface for inventory operations
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Creates a new inventory item for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="initialQuantity">Initial stock quantity</param>
    /// <param name="reorderLevel">Reorder threshold level</param>
    /// <param name="maxStockLevel">Maximum stock level</param>
    /// <returns>Created inventory item</returns>
    Task<InventoryItem> CreateInventoryItemAsync(Guid productId, int initialQuantity, int reorderLevel, int maxStockLevel);

    /// <summary>
    /// Gets inventory item by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Inventory item if found</returns>
    Task<InventoryItem?> GetInventoryByProductIdAsync(Guid productId);

    /// <summary>
    /// Reserves stock for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <param name="duration">Reservation duration</param>
    /// <param name="reference">Optional reference</param>
    /// <returns>Stock reservation</returns>
    Task<StockReservation> ReserveStockAsync(Guid productId, int quantity, TimeSpan duration, string? reference = null);

    /// <summary>
    /// Releases a stock reservation
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="reservationId">Reservation ID to release</param>
    /// <param name="reason">Reason for release</param>
    Task ReleaseReservationAsync(Guid productId, Guid reservationId, string reason = "Manual release");

    /// <summary>
    /// Confirms a stock reservation (permanently removes stock)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="reservationId">Reservation ID to confirm</param>
    Task ConfirmReservationAsync(Guid productId, Guid reservationId);

    /// <summary>
    /// Adjusts stock levels for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantityChange">Quantity change (positive for additions, negative for reductions)</param>
    /// <param name="reason">Reason for adjustment</param>
    Task AdjustStockAsync(Guid productId, int quantityChange, string reason);

    /// <summary>
    /// Gets all inventory items with low stock
    /// </summary>
    /// <returns>Collection of low stock items</returns>
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();

    /// <summary>
    /// Updates stock levels configuration for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="reorderLevel">New reorder level</param>
    /// <param name="maxStockLevel">New maximum stock level</param>
    Task UpdateStockLevelsAsync(Guid productId, int reorderLevel, int maxStockLevel);
}