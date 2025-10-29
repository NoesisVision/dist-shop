using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;
using NoesisVision.DistShop.Catalog.Domain.Repositories;
using NoesisVision.DistShop.Catalog.Infrastructure.Data;

namespace NoesisVision.DistShop.Catalog.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of ICategoryRepository
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CategoryAggregate?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<CategoryAggregate>> GetAllAsync()
    {
        return await _context.Categories
            .ToListAsync();
    }

    public async Task AddAsync(CategoryAggregate entity)
    {
        await _context.Categories.AddAsync(entity);
    }

    public async Task UpdateAsync(CategoryAggregate entity)
    {
        _context.Categories.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await GetByIdAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
        }
    }

    public async Task<IEnumerable<CategoryAggregate>> GetRootCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryAggregate>> GetChildCategoriesAsync(Guid parentCategoryId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryAggregate>> GetCategoryPathAsync(Guid categoryId)
    {
        var path = new List<CategoryAggregate>();
        var currentId = categoryId;

        while (currentId != Guid.Empty)
        {
            var category = await GetByIdAsync(currentId);
            if (category == null)
                break;

            path.Insert(0, category); // Insert at beginning to maintain order from root to target
            currentId = category.ParentCategoryId ?? Guid.Empty;
        }

        return path;
    }

    public async Task<IEnumerable<CategoryAggregate>> GetDescendantCategoriesAsync(Guid categoryId)
    {
        var descendants = new List<CategoryAggregate>();
        var toProcess = new Queue<Guid>();
        toProcess.Enqueue(categoryId);

        while (toProcess.Count > 0)
        {
            var currentId = toProcess.Dequeue();
            var children = await GetChildCategoriesAsync(currentId);

            foreach (var child in children)
            {
                descendants.Add(child);
                toProcess.Enqueue(child.Id);
            }
        }

        return descendants;
    }

    public async Task<IEnumerable<CategoryAggregate>> GetActiveCategoriesAsync(int skip = 0, int take = 50)
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAtLevelAsync(string name, Guid? parentCategoryId, Guid? excludeCategoryId = null)
    {
        var query = _context.Categories
            .Where(c => c.Name == name && c.ParentCategoryId == parentCategoryId);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return await query.AnyAsync();
    }
}