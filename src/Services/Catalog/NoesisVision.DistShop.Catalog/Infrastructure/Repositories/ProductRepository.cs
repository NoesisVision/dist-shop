using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;
using NoesisVision.DistShop.Catalog.Domain.Repositories;
using NoesisVision.DistShop.Catalog.Infrastructure.Data;

namespace NoesisVision.DistShop.Catalog.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of IProductRepository
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ProductAggregate?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<ProductAggregate>> GetAllAsync()
    {
        return await _context.Products
            .ToListAsync();
    }

    public async Task AddAsync(ProductAggregate entity)
    {
        await _context.Products.AddAsync(entity);
    }

    public async Task UpdateAsync(ProductAggregate entity)
    {
        _context.Products.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }

    public async Task<ProductAggregate?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<IEnumerable<ProductAggregate>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductAggregate>> GetActiveProductsAsync(int skip = 0, int take = 50)
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null)
    {
        var query = _context.Products.Where(p => p.Sku == sku);
        
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync();
    }
}