using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;

namespace NoesisVision.DistShop.Catalog.Domain.Repositories;

/// <summary>
/// Repository interface for Category aggregate
/// </summary>
public interface ICategoryRepository : IRepository<CategoryAggregate>
{
    /// <summary>
    /// Gets all root categories (categories without parent)
    /// </summary>
    /// <returns>Collection of root categories</returns>
    Task<IEnumerable<CategoryAggregate>> GetRootCategoriesAsync();
    
    /// <summary>
    /// Gets all child categories of a specific parent category
    /// </summary>
    /// <param name="parentCategoryId">The parent category identifier</param>
    /// <returns>Collection of child categories</returns>
    Task<IEnumerable<CategoryAggregate>> GetChildCategoriesAsync(Guid parentCategoryId);
    
    /// <summary>
    /// Gets all categories in a hierarchical path from root to the specified category
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    /// <returns>Collection of categories in the path</returns>
    Task<IEnumerable<CategoryAggregate>> GetCategoryPathAsync(Guid categoryId);
    
    /// <summary>
    /// Gets all descendant categories of a specific category
    /// </summary>
    /// <param name="categoryId">The parent category identifier</param>
    /// <returns>Collection of descendant categories</returns>
    Task<IEnumerable<CategoryAggregate>> GetDescendantCategoriesAsync(Guid categoryId);
    
    /// <summary>
    /// Gets active categories with pagination
    /// </summary>
    /// <param name="skip">Number of categories to skip</param>
    /// <param name="take">Number of categories to take</param>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<CategoryAggregate>> GetActiveCategoriesAsync(int skip = 0, int take = 50);
    
    /// <summary>
    /// Checks if a category name already exists at the same level
    /// </summary>
    /// <param name="name">The category name to check</param>
    /// <param name="parentCategoryId">The parent category ID (null for root level)</param>
    /// <param name="excludeCategoryId">Category ID to exclude from the check (for updates)</param>
    /// <returns>True if name exists at the same level, false otherwise</returns>
    Task<bool> NameExistsAtLevelAsync(string name, Guid? parentCategoryId, Guid? excludeCategoryId = null);
}