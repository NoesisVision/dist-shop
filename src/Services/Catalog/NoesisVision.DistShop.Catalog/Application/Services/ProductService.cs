using NoesisVision.DistShop.Catalog.Application.DTOs;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;
using NoesisVision.DistShop.Catalog.Domain.Repositories;
using NoesisVision.DistShop.SharedKernel.Repositories;

namespace NoesisVision.DistShop.Catalog.Application.Services;

/// <summary>
/// Service implementation for product operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku)
    {
        var product = await _productRepository.GetBySkuAsync(sku);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync(int skip = 0, int take = 50)
    {
        var products = await _productRepository.GetActiveProductsAsync(skip, take);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(createProductDto.CategoryId);
        if (category == null)
            throw new ArgumentException($"Category with ID {createProductDto.CategoryId} not found");

        // Check if SKU already exists
        if (await _productRepository.SkuExistsAsync(createProductDto.Sku))
            throw new ArgumentException($"Product with SKU '{createProductDto.Sku}' already exists");

        var product = ProductAggregate.Create(
            createProductDto.Name,
            createProductDto.Description,
            createProductDto.Sku,
            createProductDto.Price,
            createProductDto.CategoryId,
            createProductDto.Currency);

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateProductDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        // Validate category exists if changed
        if (product.CategoryId != updateProductDto.CategoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(updateProductDto.CategoryId);
            if (category == null)
                throw new ArgumentException($"Category with ID {updateProductDto.CategoryId} not found");
        }

        product.UpdateDetails(updateProductDto.Name, updateProductDto.Description);
        product.UpdatePrice(updateProductDto.Price, updateProductDto.Currency);
        
        if (product.CategoryId != updateProductDto.CategoryId)
        {
            product.ChangeCategory(updateProductDto.CategoryId);
        }

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        product.Activate();
        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        product.Deactivate();
        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static ProductDto MapToDto(ProductAggregate product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            Currency = product.Currency,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}