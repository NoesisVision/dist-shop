using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Orders.Domain.Aggregates;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;

namespace NoesisVision.DistShop.Orders.Domain.Repositories;

/// <summary>
/// Repository interface for Order aggregate
/// </summary>
public interface IOrderRepository : IRepository<OrderAggregate>
{
    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <returns>Collection of orders for the customer</returns>
    Task<IEnumerable<OrderAggregate>> GetByCustomerIdAsync(Guid customerId);
    
    /// <summary>
    /// Gets orders by status
    /// </summary>
    /// <param name="status">The order status</param>
    /// <returns>Collection of orders with the specified status</returns>
    Task<IEnumerable<OrderAggregate>> GetByStatusAsync(OrderStatus status);
    
    /// <summary>
    /// Gets orders created within a date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>Collection of orders within the date range</returns>
    Task<IEnumerable<OrderAggregate>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Gets orders with pagination
    /// </summary>
    /// <param name="skip">Number of orders to skip</param>
    /// <param name="take">Number of orders to take</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<OrderAggregate>> GetOrdersAsync(int skip = 0, int take = 50);
    
    /// <summary>
    /// Gets the total count of orders
    /// </summary>
    /// <returns>Total number of orders</returns>
    Task<int> GetTotalCountAsync();
    
    /// <summary>
    /// Gets active orders (not in terminal state) for a customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <returns>Collection of active orders for the customer</returns>
    Task<IEnumerable<OrderAggregate>> GetActiveOrdersByCustomerAsync(Guid customerId);
}