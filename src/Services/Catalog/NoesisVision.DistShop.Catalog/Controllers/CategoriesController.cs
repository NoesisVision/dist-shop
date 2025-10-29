using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.Catalog.Application.DTOs;
using NoesisVision.DistShop.Catalog.Application.Services;
using NoesisVision.DistShop.Catalog.Domain.Exceptions;

namespace NoesisVision.DistShop.Catalog.Controllers;

/// <summary>
/// API controller for category operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active categories with pagination
    /// </summary>
    /// <param name="skip">Number of categories to skip</param>
    /// <param name="take">Number of categories to take (max 100)</param>
    /// <returns>Collection of active categories</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            if (take > 100) take = 100; // Limit maximum page size
            if (skip < 0) skip = 0;

            var categories = await _categoryService.GetActiveCategoriesAsync(skip, take);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Gets a category by its identifier
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>The category if found</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    /// <summary>
    /// Gets all root categories (categories without parent)
    /// </summary>
    /// <returns>Collection of root categories</returns>
    [HttpGet("root")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetRootCategories()
    {
        try
        {
            var categories = await _categoryService.GetRootCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving root categories");
            return StatusCode(500, "An error occurred while retrieving root categories");
        }
    }

    /// <summary>
    /// Gets all child categories of a specific parent category
    /// </summary>
    /// <param name="parentId">The parent category identifier</param>
    /// <returns>Collection of child categories</returns>
    [HttpGet("{parentId:guid}/children")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetChildCategories(Guid parentId)
    {
        try
        {
            var categories = await _categoryService.GetChildCategoriesAsync(parentId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child categories for parent {ParentId}", parentId);
            return StatusCode(500, "An error occurred while retrieving child categories");
        }
    }

    /// <summary>
    /// Gets all categories in a hierarchical path from root to the specified category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>Collection of categories in the path</returns>
    [HttpGet("{id:guid}/path")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoryPath(Guid id)
    {
        try
        {
            var categories = await _categoryService.GetCategoryPathAsync(id);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category path for {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category path");
        }
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="createCategoryDto">Category creation data</param>
    /// <returns>The created category</returns>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.CreateAsync(createCategoryDto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (InvalidCategoryOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid category operation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (CircularCategoryHierarchyException ex)
        {
            _logger.LogWarning(ex, "Circular category hierarchy: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <param name="updateCategoryDto">Category update data</param>
    /// <returns>The updated category</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateAsync(id, updateCategoryDto);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }
        catch (InvalidCategoryOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid category operation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (CircularCategoryHierarchyException ex)
        {
            _logger.LogWarning(ex, "Circular category hierarchy: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the category");
        }
    }

    /// <summary>
    /// Activates a category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> ActivateCategory(Guid id)
    {
        try
        {
            var success = await _categoryService.ActivateAsync(id);
            if (!success)
                return NotFound($"Category with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while activating the category");
        }
    }

    /// <summary>
    /// Deactivates a category
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> DeactivateCategory(Guid id)
    {
        try
        {
            var success = await _categoryService.DeactivateAsync(id);
            if (!success)
                return NotFound($"Category with ID {id} not found");

            return NoContent();
        }
        catch (InvalidCategoryOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid category operation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deactivating the category");
        }
    }
}