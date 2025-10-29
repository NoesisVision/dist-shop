using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object representing a product name with validation
/// </summary>
public class ProductName : ValueObject
{
    public string Value { get; private set; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name cannot be null or empty", nameof(value));

        if (value.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(value));

        return new ProductName(value.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(ProductName productName) => productName.Value;
    public static explicit operator ProductName(string value) => Create(value);
}