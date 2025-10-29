using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.Inventory.Domain.Aggregates;
using NoesisVision.DistShop.Inventory.Domain.Repositories;
using NoesisVision.DistShop.Inventory.Infrastructure.Data;

namespace NoesisVision.DistShop.Inventory.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of IInventoryRepository
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<InventoryItem?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .ToListAsync();
    }

    public async Task AddAsync(InventoryItem entity)
    {
        await _context.InventoryItems.AddAsync(entity);
    }

    public async Task UpdateAsync(InventoryItem entity)
    {
        _context.InventoryItems.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var inventoryItem = await GetByIdAsync(id);
        if (inventoryItem != null)
        {
            _context.InventoryItems.Remove(inventoryItem);
        }
    }

    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.AvailableQuantity + 
                       i.Reservations.Where(r => r.ExpiresAt > DateTime.UtcNow).Sum(r => r.Quantity) 
                       <= i.ReorderLevel)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetByProductIdsAsync(IEnumerable<Guid> productIds)
    {
        return await _context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync();
    }
}