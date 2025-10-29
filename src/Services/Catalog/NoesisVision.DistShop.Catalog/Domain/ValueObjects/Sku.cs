using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object representing a Stock Keeping Unit (SKU) with validation
/// </summary>
public class Sku : ValueObject
{
    public string Value { get; private set; }

    private Sku(string value)
    {
        Value = value;
    }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be null or empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters", nameof(value));

        // SKU should contain only alphanumeric characters and hyphens
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Za-z0-9\-]+$"))
            throw new ArgumentException("SKU can only contain alphanumeric characters and hyphens", nameof(value));

        return new Sku(value.ToUpperInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Sku sku) => sku.Value;
    public static explicit operator Sku(string value) => Create(value);
}