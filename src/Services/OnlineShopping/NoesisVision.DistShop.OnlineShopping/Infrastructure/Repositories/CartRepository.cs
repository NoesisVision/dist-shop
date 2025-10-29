using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.OnlineShopping.Domain.Aggregates;
using NoesisVision.DistShop.OnlineShopping.Domain.Repositories;
using NoesisVision.DistShop.OnlineShopping.Infrastructure.Data;

namespace NoesisVision.DistShop.OnlineShopping.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of cart repository
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly OnlineShoppingDbContext _context;

    public CartRepository(OnlineShoppingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CartAggregate?> GetByIdAsync(Guid id)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<CartAggregate>> GetAllAsync()
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ToListAsync();
    }

    public async Task<CartAggregate?> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<CartAggregate> GetOrCreateByCustomerIdAsync(Guid customerId, string currency = "USD")
    {
        var existingCart = await GetByCustomerIdAsync(customerId);
        
        if (existingCart != null)
            return existingCart;

        var newCart = CartAggregate.Create(customerId, currency);
        await AddAsync(newCart);
        
        return newCart;
    }

    public async Task<IEnumerable<CartAggregate>> GetExpiredCartsAsync(TimeSpan expirationPeriod)
    {
        var cutoffTime = DateTime.UtcNow - expirationPeriod;
        
        return await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.LastActivityAt.HasValue && c.LastActivityAt.Value < cutoffTime)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredCartsAsync(TimeSpan expirationPeriod)
    {
        var cutoffTime = DateTime.UtcNow - expirationPeriod;
        
        var expiredCarts = await _context.Carts
            .Where(c => c.LastActivityAt.HasValue && c.LastActivityAt.Value < cutoffTime)
            .ToListAsync();

        if (expiredCarts.Any())
        {
            _context.Carts.RemoveRange(expiredCarts);
            await _context.SaveChangesAsync();
        }

        return expiredCarts.Count;
    }

    public async Task AddAsync(CartAggregate entity)
    {
        await _context.Carts.AddAsync(entity);
    }

    public Task UpdateAsync(CartAggregate entity)
    {
        _context.Carts.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
        }
    }
}