using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.Catalog.Domain.ValueObjects;
using NoesisVision.DistShop.Catalog.Domain.Exceptions;
using NoesisVision.DistShop.Catalog.Contracts.Events;

namespace NoesisVision.DistShop.Catalog.Domain.Aggregates;

/// <summary>
/// Product aggregate root managing product information and business rules
/// </summary>
public class ProductAggregate : AggregateRoot
{
    private string _name;
    private string _description;
    private string _sku;
    private decimal _price;
    private string _currency;
    private Guid _categoryId;
    private bool _isActive;
    private DateTime _createdAt;
    private DateTime _updatedAt;

    // EF Core constructor
    private ProductAggregate() : base() 
    {
        _name = null!;
        _description = null!;
        _sku = null!;
        _currency = null!;
    }

    private ProductAggregate(
        string name,
        string description,
        string sku,
        decimal price,
        string currency,
        Guid categoryId) : base()
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _description = description ?? throw new ArgumentNullException(nameof(description));
        _sku = sku ?? throw new ArgumentNullException(nameof(sku));
        _price = price;
        _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        _categoryId = categoryId;
        _isActive = true;
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductCreatedEvent(
            Id,
            _name,
            _description,
            _categoryId,
            _price,
            _sku));
    }

    public static ProductAggregate Create(
        string name,
        string description,
        string sku,
        decimal price,
        Guid categoryId,
        string currency = "USD")
    {
        if (categoryId == Guid.Empty)
            throw new InvalidProductOperationException("Product must belong to a valid category");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductOperationException("Product name cannot be null or empty");

        if (string.IsNullOrWhiteSpace(sku))
            throw new InvalidProductOperationException("Product SKU cannot be null or empty");

        if (price < 0)
            throw new InvalidProductOperationException("Product price cannot be negative");

        return new ProductAggregate(name.Trim(), description?.Trim() ?? string.Empty, sku.Trim().ToUpperInvariant(), price, currency, categoryId);
    }

    // Properties
    public string Name => _name;
    public string Description => _description;
    public string Sku => _sku;
    public decimal Price => _price;
    public string Currency => _currency;
    public Guid CategoryId => _categoryId;
    public bool IsActive => _isActive;
    public DateTime CreatedAt => _createdAt;
    public DateTime UpdatedAt => _updatedAt;

    // Business methods
    public void UpdateDetails(string name, string description)
    {
        if (!_isActive)
            throw new InvalidProductOperationException("Cannot update inactive product");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductOperationException("Product name cannot be null or empty");

        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim() ?? string.Empty;
        
        if (_name != trimmedName || _description != trimmedDescription)
        {
            _name = trimmedName;
            _description = trimmedDescription;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new ProductUpdatedEvent(
                Id,
                _name,
                _description,
                _categoryId,
                _price,
                _sku));
        }
    }

    public void UpdatePrice(decimal newPrice, string currency = "USD")
    {
        if (!_isActive)
            throw new InvalidProductOperationException("Cannot update price of inactive product");

        if (newPrice < 0)
            throw new InvalidProductOperationException("Product price cannot be negative");
        
        if (_price != newPrice || _currency != currency)
        {
            _price = newPrice;
            _currency = currency;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new ProductUpdatedEvent(
                Id,
                _name,
                _description,
                _categoryId,
                _price,
                _sku));
        }
    }

    public void ChangeCategory(Guid newCategoryId)
    {
        if (!_isActive)
            throw new InvalidProductOperationException("Cannot change category of inactive product");

        if (newCategoryId == Guid.Empty)
            throw new InvalidProductOperationException("Product must belong to a valid category");

        if (_categoryId != newCategoryId)
        {
            _categoryId = newCategoryId;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new ProductUpdatedEvent(
                Id,
                _name,
                _description,
                _categoryId,
                _price,
                _sku));
        }
    }

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _updatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _updatedAt = DateTime.UtcNow;
    }
}