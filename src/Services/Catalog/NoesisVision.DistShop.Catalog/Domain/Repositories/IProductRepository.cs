using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;

namespace NoesisVision.DistShop.Catalog.Domain.Repositories;

/// <summary>
/// Repository interface for Product aggregate
/// </summary>
public interface IProductRepository : IRepository<ProductAggregate>
{
    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    /// <param name="sku">The product SKU</param>
    /// <returns>The product if found, null otherwise</returns>
    Task<ProductAggregate?> GetBySkuAsync(string sku);
    
    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    /// <returns>Collection of products in the category</returns>
    Task<IEnumerable<ProductAggregate>> GetByCategoryAsync(Guid categoryId);
    
    /// <summary>
    /// Gets active products with pagination
    /// </summary>
    /// <param name="skip">Number of products to skip</param>
    /// <param name="take">Number of products to take</param>
    /// <returns>Collection of active products</returns>
    Task<IEnumerable<ProductAggregate>> GetActiveProductsAsync(int skip = 0, int take = 50);
    
    /// <summary>
    /// Checks if a SKU already exists
    /// </summary>
    /// <param name="sku">The SKU to check</param>
    /// <param name="excludeProductId">Product ID to exclude from the check (for updates)</param>
    /// <returns>True if SKU exists, false otherwise</returns>
    Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null);
}