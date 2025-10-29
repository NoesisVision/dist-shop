using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.Orders.Domain.Aggregates;
using NoesisVision.DistShop.Orders.Domain.Repositories;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;
using NoesisVision.DistShop.Orders.Infrastructure.Data;

namespace NoesisVision.DistShop.Orders.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of the Order repository
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OrderAggregate?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<OrderAggregate>> GetAllAsync()
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(OrderAggregate entity)
    {
        await _context.Orders.AddAsync(entity);
    }

    public async Task UpdateAsync(OrderAggregate entity)
    {
        _context.Orders.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
        }
    }

    public async Task<IEnumerable<OrderAggregate>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderAggregate>> GetByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderAggregate>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Orders
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderAggregate>> GetOrdersAsync(int skip = 0, int take = 50)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<IEnumerable<OrderAggregate>> GetActiveOrdersByCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId && 
                       o.Status != OrderStatus.Delivered && 
                       o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}