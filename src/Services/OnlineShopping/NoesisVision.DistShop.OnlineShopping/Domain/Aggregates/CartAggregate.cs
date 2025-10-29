using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.OnlineShopping.Domain.ValueObjects;
using NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;
using NoesisVision.DistShop.OnlineShopping.Contracts.Events;

namespace NoesisVision.DistShop.OnlineShopping.Domain.Aggregates;

/// <summary>
/// Cart aggregate root managing shopping cart state and operations
/// </summary>
public class CartAggregate : AggregateRoot
{
    private readonly List<CartItem> _items = new();
    private Guid _customerId;
    private string _currency;
    private DateTime _createdAt;
    private DateTime _updatedAt;
    private DateTime? _lastActivityAt;

    // Business rules
    private const int MaxItemsPerCart = 100;
    private const int MaxQuantityPerItem = 999;

    // EF Core constructor
    private CartAggregate() : base()
    {
        _currency = null!;
    }

    private CartAggregate(Guid customerId, string currency) : base()
    {
        _customerId = customerId;
        _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;
        _lastActivityAt = DateTime.UtcNow;
    }

    public static CartAggregate Create(Guid customerId, string currency = "USD")
    {
        if (customerId == Guid.Empty)
            throw new InvalidCartOperationException("Customer ID cannot be empty");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidCartOperationException("Currency cannot be null or empty");

        return new CartAggregate(customerId, currency.Trim().ToUpperInvariant());
    }

    // Properties
    public Guid CustomerId => _customerId;
    public string Currency => _currency;
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
    public DateTime CreatedAt => _createdAt;
    public DateTime UpdatedAt => _updatedAt;
    public DateTime? LastActivityAt => _lastActivityAt;

    // Calculated properties
    public int ItemCount => _items.Count;
    public int TotalQuantity => _items.Sum(item => item.Quantity);
    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);
    public bool IsEmpty => !_items.Any();

    // Cart operations
    public void AddProduct(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        ValidateCartNotFull();
        ValidateQuantity(quantity);
        ValidatePrice(unitPrice);

        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId);
        
        if (existingItem != null)
        {
            // Update existing item quantity
            var newQuantity = existingItem.Quantity + quantity;
            ValidateItemQuantity(newQuantity);
            
            var oldQuantity = existingItem.Quantity;
            var updatedItem = existingItem.UpdateQuantity(newQuantity);
            
            // Update price if different (price might have changed)
            if (existingItem.UnitPrice != unitPrice)
            {
                updatedItem = updatedItem.UpdatePrice(unitPrice);
            }
            
            _items.Remove(existingItem);
            _items.Add(updatedItem);
            
            UpdateActivity();
            
            AddDomainEvent(new CartQuantityUpdatedEvent(
                Id,
                _customerId,
                productId,
                oldQuantity,
                newQuantity));
        }
        else
        {
            // Add new item
            var cartItem = CartItem.Create(productId, productName, quantity, unitPrice, _currency);
            _items.Add(cartItem);
            
            UpdateActivity();
            
            AddDomainEvent(new ProductAddedToCartEvent(
                Id,
                _customerId,
                productId,
                productName,
                quantity,
                unitPrice,
                _currency));
        }
    }

    public void RemoveProduct(Guid productId)
    {
        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId);
        if (existingItem == null)
            throw new InvalidCartOperationException($"Product {productId} not found in cart");

        _items.Remove(existingItem);
        UpdateActivity();

        AddDomainEvent(new ProductRemovedFromCartEvent(
            Id,
            _customerId,
            productId,
            existingItem.Quantity));
    }

    public void UpdateProductQuantity(Guid productId, int newQuantity)
    {
        ValidateQuantity(newQuantity);
        ValidateItemQuantity(newQuantity);

        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId);
        if (existingItem == null)
            throw new InvalidCartOperationException($"Product {productId} not found in cart");

        var oldQuantity = existingItem.Quantity;
        var updatedItem = existingItem.UpdateQuantity(newQuantity);
        
        _items.Remove(existingItem);
        _items.Add(updatedItem);
        
        UpdateActivity();

        AddDomainEvent(new CartQuantityUpdatedEvent(
            Id,
            _customerId,
            productId,
            oldQuantity,
            newQuantity));
    }

    public void UpdateProductPrice(Guid productId, decimal newUnitPrice)
    {
        ValidatePrice(newUnitPrice);

        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId);
        if (existingItem == null)
            throw new InvalidCartOperationException($"Product {productId} not found in cart");

        var updatedItem = existingItem.UpdatePrice(newUnitPrice);
        
        _items.Remove(existingItem);
        _items.Add(updatedItem);
        
        UpdateActivity();
    }

    public void Clear()
    {
        if (IsEmpty)
            return;

        var itemsRemoved = _items.Count;
        _items.Clear();
        UpdateActivity();

        AddDomainEvent(new CartClearedEvent(
            Id,
            _customerId,
            itemsRemoved));
    }

    public void InitiateCheckout()
    {
        if (IsEmpty)
            throw new InvalidCartOperationException("Cannot checkout an empty cart");

        UpdateActivity();

        AddDomainEvent(new CartCheckoutInitiatedEvent(
            Id,
            _customerId,
            TotalAmount,
            _currency,
            ItemCount));
    }

    public void CompleteCheckout(Guid orderId)
    {
        if (IsEmpty)
            throw new InvalidCartOperationException("Cannot complete checkout for an empty cart");

        if (orderId == Guid.Empty)
            throw new InvalidCartOperationException("Order ID cannot be empty");

        var totalAmount = TotalAmount;
        
        // Clear cart after successful checkout
        _items.Clear();
        UpdateActivity();

        AddDomainEvent(new CartCheckoutCompletedEvent(
            Id,
            _customerId,
            orderId,
            totalAmount,
            _currency));
    }

    // Business logic methods
    public bool HasProduct(Guid productId)
    {
        return _items.Any(item => item.ProductId == productId);
    }

    public CartItem? GetItem(Guid productId)
    {
        return _items.FirstOrDefault(item => item.ProductId == productId);
    }

    public bool IsExpired(TimeSpan expirationPeriod)
    {
        return _lastActivityAt.HasValue && 
               DateTime.UtcNow - _lastActivityAt.Value > expirationPeriod;
    }

    public bool CanAddMoreItems()
    {
        return _items.Count < MaxItemsPerCart;
    }

    // Private helper methods
    private void ValidateCartNotFull()
    {
        if (_items.Count >= MaxItemsPerCart)
            throw new InvalidCartOperationException($"Cart cannot contain more than {MaxItemsPerCart} different items");
    }

    private void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidCartOperationException("Quantity must be greater than zero");
    }

    private void ValidateItemQuantity(int quantity)
    {
        if (quantity > MaxQuantityPerItem)
            throw new InvalidCartOperationException($"Item quantity cannot exceed {MaxQuantityPerItem}");
    }

    private void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new InvalidCartOperationException("Price cannot be negative");
    }

    private void UpdateActivity()
    {
        _updatedAt = DateTime.UtcNow;
        _lastActivityAt = DateTime.UtcNow;
    }
}