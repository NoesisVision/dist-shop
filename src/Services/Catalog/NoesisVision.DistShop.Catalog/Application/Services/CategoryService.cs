using NoesisVision.DistShop.Catalog.Application.DTOs;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;
using NoesisVision.DistShop.Catalog.Domain.Repositories;
using NoesisVision.DistShop.SharedKernel.Repositories;

namespace NoesisVision.DistShop.Catalog.Application.Services;

/// <summary>
/// Service implementation for category operations
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync();
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(Guid parentCategoryId)
    {
        var categories = await _categoryRepository.GetChildCategoriesAsync(parentCategoryId);
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoryPathAsync(Guid categoryId)
    {
        var categories = await _categoryRepository.GetCategoryPathAsync(categoryId);
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(int skip = 0, int take = 50)
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync(skip, take);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto)
    {
        // Validate parent category exists if specified
        if (createCategoryDto.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(createCategoryDto.ParentCategoryId.Value);
            if (parentCategory == null)
                throw new ArgumentException($"Parent category with ID {createCategoryDto.ParentCategoryId} not found");
        }

        // Check if name already exists at the same level
        if (await _categoryRepository.NameExistsAtLevelAsync(createCategoryDto.Name, createCategoryDto.ParentCategoryId))
            throw new ArgumentException($"Category with name '{createCategoryDto.Name}' already exists at this level");

        var category = CategoryAggregate.Create(
            createCategoryDto.Name,
            createCategoryDto.Description,
            createCategoryDto.ParentCategoryId);

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return null;

        // Validate parent category exists if specified and changed
        if (updateCategoryDto.ParentCategoryId.HasValue && 
            category.ParentCategoryId != updateCategoryDto.ParentCategoryId)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(updateCategoryDto.ParentCategoryId.Value);
            if (parentCategory == null)
                throw new ArgumentException($"Parent category with ID {updateCategoryDto.ParentCategoryId} not found");
        }

        // Check if name already exists at the same level (excluding current category)
        if (await _categoryRepository.NameExistsAtLevelAsync(updateCategoryDto.Name, updateCategoryDto.ParentCategoryId, id))
            throw new ArgumentException($"Category with name '{updateCategoryDto.Name}' already exists at this level");

        category.UpdateDetails(updateCategoryDto.Name, updateCategoryDto.Description);
        
        if (category.ParentCategoryId != updateCategoryDto.ParentCategoryId)
        {
            // Get ancestor IDs to prevent circular hierarchy
            var ancestorIds = await GetAncestorIdsAsync(id);
            category.ChangeParent(updateCategoryDto.ParentCategoryId, ancestorIds);
        }

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return false;

        category.Activate();
        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return false;

        category.Deactivate();
        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<IEnumerable<Guid>> GetAncestorIdsAsync(Guid categoryId)
    {
        var descendants = await _categoryRepository.GetDescendantCategoriesAsync(categoryId);
        return descendants.Select(c => c.Id);
    }

    private static CategoryDto MapToDto(CategoryAggregate category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            IsRootCategory = category.IsRootCategory,
            HasChildren = category.HasChildren
        };
    }
}