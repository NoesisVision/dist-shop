namespace NoesisVision.DistShop.Orders.Domain.ValueObjects;

/// <summary>
/// Enumeration representing the various states of an order
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but not yet confirmed
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Order has been confirmed and is ready for processing
    /// </summary>
    Confirmed = 1,
    
    /// <summary>
    /// Order is being processed (inventory reserved, payment processing)
    /// </summary>
    Processing = 2,
    
    /// <summary>
    /// Order has been shipped to the customer
    /// </summary>
    Shipped = 3,
    
    /// <summary>
    /// Order has been delivered to the customer
    /// </summary>
    Delivered = 4,
    
    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled = 5
}