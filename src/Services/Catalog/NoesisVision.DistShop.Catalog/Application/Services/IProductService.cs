using NoesisVision.DistShop.Catalog.Application.DTOs;

namespace NoesisVision.DistShop.Catalog.Application.Services;

/// <summary>
/// Service interface for product operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets a product by its identifier
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>The product if found, null otherwise</returns>
    Task<ProductDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    /// <param name="sku">The product SKU</param>
    /// <returns>The product if found, null otherwise</returns>
    Task<ProductDto?> GetBySkuAsync(string sku);

    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    /// <returns>Collection of products in the category</returns>
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId);

    /// <summary>
    /// Gets active products with pagination
    /// </summary>
    /// <param name="skip">Number of products to skip</param>
    /// <param name="take">Number of products to take</param>
    /// <returns>Collection of active products</returns>
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="createProductDto">Product creation data</param>
    /// <returns>The created product</returns>
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <param name="updateProductDto">Product update data</param>
    /// <returns>The updated product if found, null otherwise</returns>
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateProductDto);

    /// <summary>
    /// Activates a product
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>True if product was found and activated, false otherwise</returns>
    Task<bool> ActivateAsync(Guid id);

    /// <summary>
    /// Deactivates a product
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>True if product was found and deactivated, false otherwise</returns>
    Task<bool> DeactivateAsync(Guid id);
}