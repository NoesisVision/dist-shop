using NoesisVision.DistShop.Catalog.Application.DTOs;

namespace NoesisVision.DistShop.Catalog.Application.Services;

/// <summary>
/// Service interface for category operations
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets a category by its identifier
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>The category if found, null otherwise</returns>
    Task<CategoryDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all root categories (categories without parent)
    /// </summary>
    /// <returns>Collection of root categories</returns>
    Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync();

    /// <summary>
    /// Gets all child categories of a specific parent category
    /// </summary>
    /// <param name="parentCategoryId">The parent category identifier</param>
    /// <returns>Collection of child categories</returns>
    Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(Guid parentCategoryId);

    /// <summary>
    /// Gets all categories in a hierarchical path from root to the specified category
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    /// <returns>Collection of categories in the path</returns>
    Task<IEnumerable<CategoryDto>> GetCategoryPathAsync(Guid categoryId);

    /// <summary>
    /// Gets active categories with pagination
    /// </summary>
    /// <param name="skip">Number of categories to skip</param>
    /// <param name="take">Number of categories to take</param>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(int skip = 0, int take = 50);

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="createCategoryDto">Category creation data</param>
    /// <returns>The created category</returns>
    Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <param name="updateCategoryDto">Category update data</param>
    /// <returns>The updated category if found, null otherwise</returns>
    Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto);

    /// <summary>
    /// Activates a category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>True if category was found and activated, false otherwise</returns>
    Task<bool> ActivateAsync(Guid id);

    /// <summary>
    /// Deactivates a category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>True if category was found and deactivated, false otherwise</returns>
    Task<bool> DeactivateAsync(Guid id);
}