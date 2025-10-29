namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Service interface for integrating with Inventory Service
/// </summary>
public interface IInventoryIntegrationService
{
    /// <summary>
    /// Checks if a product is available in the requested quantity
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="quantity">The requested quantity</param>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> IsProductAvailableAsync(Guid productId, int quantity);
    
    /// <summary>
    /// Checks availability for multiple products
    /// </summary>
    /// <param name="items">Product IDs and quantities to check</param>
    /// <returns>Dictionary of product ID to availability status</returns>
    Task<Dictionary<Guid, bool>> CheckProductsAvailabilityAsync(IEnumerable<ProductAvailabilityCheck> items);
    
    /// <summary>
    /// Gets current stock level for a product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Current available quantity</returns>
    Task<int> GetProductStockLevelAsync(Guid productId);
    
    /// <summary>
    /// Reserves stock for cart items during checkout
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="items">Items to reserve</param>
    /// <returns>Reservation result</returns>
    Task<StockReservationResult> ReserveStockAsync(Guid customerId, IEnumerable<StockReservationItem> items);
    
    /// <summary>
    /// Releases stock reservation (e.g., when cart is abandoned)
    /// </summary>
    /// <param name="reservationId">The reservation ID</param>
    /// <returns>True if successfully released</returns>
    Task<bool> ReleaseStockReservationAsync(Guid reservationId);
}

/// <summary>
/// Product availability check request
/// </summary>
public class ProductAvailabilityCheck
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Stock reservation item
/// </summary>
public class StockReservationItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Result of stock reservation operation
/// </summary>
public class StockReservationResult
{
    public bool Success { get; set; }
    public Guid? ReservationId { get; set; }
    public Dictionary<Guid, int> ReservedQuantities { get; set; } = new();
    public List<Guid> UnavailableProducts { get; set; } = new();
    public string? ErrorMessage { get; set; }
}