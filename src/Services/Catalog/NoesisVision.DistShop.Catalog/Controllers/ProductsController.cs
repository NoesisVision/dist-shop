using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.Catalog.Application.DTOs;
using NoesisVision.DistShop.Catalog.Application.Services;
using NoesisVision.DistShop.Catalog.Domain.Exceptions;

namespace NoesisVision.DistShop.Catalog.Controllers;

/// <summary>
/// API controller for product operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active products with pagination
    /// </summary>
    /// <param name="skip">Number of products to skip</param>
    /// <param name="take">Number of products to take (max 100)</param>
    /// <returns>Collection of active products</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            if (take > 100) take = 100; // Limit maximum page size
            if (skip < 0) skip = 0;

            var products = await _productService.GetActiveProductsAsync(skip, take);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    /// <summary>
    /// Gets a product by its identifier
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>The product if found</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    /// <param name="sku">The product SKU</param>
    /// <returns>The product if found</returns>
    [HttpGet("by-sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetProductBySku(string sku)
    {
        try
        {
            var product = await _productService.GetBySkuAsync(sku);
            if (product == null)
                return NotFound($"Product with SKU '{sku}' not found");

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by SKU {Sku}", sku);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    /// <returns>Collection of products in the category</returns>
    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(Guid categoryId)
    {
        try
        {
            var products = await _productService.GetByCategoryAsync(categoryId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="createProductDto">Product creation data</param>
    /// <returns>The created product</returns>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.CreateAsync(createProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (InvalidProductOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid product operation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <param name="updateProductDto">Product update data</param>
    /// <returns>The updated product</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateAsync(id, updateProductDto);
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            return Ok(product);
        }
        catch (InvalidProductOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid product operation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    /// <summary>
    /// Activates a product
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> ActivateProduct(Guid id)
    {
        try
        {
            var success = await _productService.ActivateAsync(id);
            if (!success)
                return NotFound($"Product with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product {ProductId}", id);
            return StatusCode(500, "An error occurred while activating the product");
        }
    }

    /// <summary>
    /// Deactivates a product
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> DeactivateProduct(Guid id)
    {
        try
        {
            var success = await _productService.DeactivateAsync(id);
            if (!success)
                return NotFound($"Product with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product {ProductId}", id);
            return StatusCode(500, "An error occurred while deactivating the product");
        }
    }
}