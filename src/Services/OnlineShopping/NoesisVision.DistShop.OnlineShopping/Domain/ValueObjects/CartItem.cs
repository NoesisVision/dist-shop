using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

namespace NoesisVision.DistShop.OnlineShopping.Domain.ValueObjects;

/// <summary>
/// Value object representing an item in a shopping cart
/// </summary>
public class CartItem : ValueObject
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }
    public DateTime AddedAt { get; private set; }

    // EF Core constructor
    private CartItem()
    {
        ProductName = null!;
        Currency = null!;
    }

    private CartItem(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string currency)
    {
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        AddedAt = DateTime.UtcNow;
    }

    public static CartItem Create(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string currency = "USD")
    {
        if (productId == Guid.Empty)
            throw new InvalidCartOperationException("Product ID cannot be empty");

        if (string.IsNullOrWhiteSpace(productName))
            throw new InvalidCartOperationException("Product name cannot be null or empty");

        if (quantity <= 0)
            throw new InvalidCartOperationException("Quantity must be greater than zero");

        if (unitPrice < 0)
            throw new InvalidCartOperationException("Unit price cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidCartOperationException("Currency cannot be null or empty");

        return new CartItem(productId, productName.Trim(), quantity, unitPrice, currency.Trim().ToUpperInvariant());
    }

    public decimal TotalPrice => Quantity * UnitPrice;

    public CartItem UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new InvalidCartOperationException("Quantity must be greater than zero");

        return new CartItem(ProductId, ProductName, newQuantity, UnitPrice, Currency);
    }

    public CartItem UpdatePrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new InvalidCartOperationException("Unit price cannot be negative");

        return new CartItem(ProductId, ProductName, Quantity, newUnitPrice, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return Quantity;
        yield return UnitPrice;
        yield return Currency;
    }
}