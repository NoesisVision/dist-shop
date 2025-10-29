using NoesisVision.DistShop.OnlineShopping.Application.DTOs;

namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Service interface for integrating with Orders Service
/// </summary>
public interface IOrderIntegrationService
{
    /// <summary>
    /// Creates an order from cart items
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order information</returns>
    Task<OrderCreationResult> CreateOrderAsync(CreateOrderRequest request);
    
    /// <summary>
    /// Gets order status
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Order status information</returns>
    Task<OrderStatusInfo?> GetOrderStatusAsync(Guid orderId);
}

/// <summary>
/// Request for creating an order from cart
/// </summary>
public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public string Currency { get; set; } = "USD";
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? PaymentMethod { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

/// <summary>
/// Order item for order creation
/// </summary>
public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Result of order creation
/// </summary>
public class OrderCreationResult
{
    public bool Success { get; set; }
    public Guid? OrderId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Order status information
/// </summary>
public class OrderStatusInfo
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}