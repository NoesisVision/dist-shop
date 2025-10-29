using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Orders.Domain.ValueObjects;

/// <summary>
/// Value object representing an item within an order
/// </summary>
public class OrderItem : ValueObject
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductSku { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    // EF Core constructor
    private OrderItem()
    {
        ProductName = null!;
        ProductSku = null!;
        Currency = null!;
    }

    private OrderItem(
        Guid productId,
        string productName,
        string productSku,
        int quantity,
        decimal unitPrice,
        string currency)
    {
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        ProductSku = productSku ?? throw new ArgumentNullException(nameof(productSku));
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static OrderItem Create(
        Guid productId,
        string productName,
        string productSku,
        int quantity,
        decimal unitPrice,
        string currency = "USD")
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be null or empty", nameof(productName));
        
        if (string.IsNullOrWhiteSpace(productSku))
            throw new ArgumentException("Product SKU cannot be null or empty", nameof(productSku));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        return new OrderItem(
            productId,
            productName.Trim(),
            productSku.Trim(),
            quantity,
            unitPrice,
            currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return ProductSku;
        yield return Quantity;
        yield return UnitPrice;
        yield return Currency;
    }
}