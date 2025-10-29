using NoesisVision.DistShop.Orders.Domain.Aggregates;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;
using NoesisVision.DistShop.Orders.Application.DTOs;

namespace NoesisVision.DistShop.Orders.Application.Services;

/// <summary>
/// Service interface for order operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="items">The order items</param>
    /// <param name="currency">The currency (default: USD)</param>
    /// <returns>The created order</returns>
    Task<OrderAggregate> CreateOrderAsync(Guid customerId, IEnumerable<OrderItemDto> items, string currency = "USD");
    
    /// <summary>
    /// Gets an order by its identifier
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <returns>The order if found, null otherwise</returns>
    Task<OrderAggregate?> GetOrderByIdAsync(Guid orderId);
    
    /// <summary>
    /// Gets all orders for a customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <returns>Collection of orders for the customer</returns>
    Task<IEnumerable<OrderAggregate>> GetOrdersByCustomerAsync(Guid customerId);
    
    /// <summary>
    /// Confirms an order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <returns>The updated order</returns>
    Task<OrderAggregate> ConfirmOrderAsync(Guid orderId);
    
    /// <summary>
    /// Starts processing an order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <returns>The updated order</returns>
    Task<OrderAggregate> StartProcessingOrderAsync(Guid orderId);
    
    /// <summary>
    /// Marks an order as shipped
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <returns>The updated order</returns>
    Task<OrderAggregate> MarkOrderAsShippedAsync(Guid orderId);
    
    /// <summary>
    /// Marks an order as delivered
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <returns>The updated order</returns>
    Task<OrderAggregate> MarkOrderAsDeliveredAsync(Guid orderId);
    
    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="reason">The cancellation reason</param>
    /// <returns>The updated order</returns>
    Task<OrderAggregate> CancelOrderAsync(Guid orderId, string reason);
    
    /// <summary>
    /// Gets orders by status
    /// </summary>
    /// <param name="status">The order status</param>
    /// <returns>Collection of orders with the specified status</returns>
    Task<IEnumerable<OrderAggregate>> GetOrdersByStatusAsync(OrderStatus status);
}