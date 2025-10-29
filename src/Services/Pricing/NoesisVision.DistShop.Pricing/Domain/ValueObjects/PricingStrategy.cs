using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Pricing.Domain.ValueObjects;

/// <summary>
/// Value object representing different pricing strategies
/// </summary>
public class PricingStrategy : ValueObject
{
    public PricingStrategyType Type { get; private set; }
    public decimal Value { get; private set; }
    public Dictionary<string, object> Parameters { get; private set; }

    private PricingStrategy()
    {
        Parameters = new Dictionary<string, object>();
    }

    public PricingStrategy(PricingStrategyType type, decimal value, Dictionary<string, object>? parameters = null)
    {
        Type = type;
        Value = value;
        Parameters = parameters ?? new Dictionary<string, object>();

        ValidateStrategy();
    }

    private void ValidateStrategy()
    {
        switch (Type)
        {
            case PricingStrategyType.Fixed:
                if (Value < 0)
                    throw new ArgumentException("Fixed price cannot be negative");
                break;
            
            case PricingStrategyType.Percentage:
                if (Value < 0 || Value > 100)
                    throw new ArgumentException("Percentage must be between 0 and 100");
                break;
            
            case PricingStrategyType.Tiered:
                if (!Parameters.ContainsKey("tiers"))
                    throw new ArgumentException("Tiered pricing requires 'tiers' parameter");
                break;
            
            case PricingStrategyType.Promotional:
                if (Value < 0 || Value > 100)
                    throw new ArgumentException("Promotional discount must be between 0 and 100");
                break;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
        foreach (var param in Parameters.OrderBy(x => x.Key))
        {
            yield return param.Key;
            yield return param.Value;
        }
    }
}

/// <summary>
/// Enumeration of supported pricing strategy types
/// </summary>
public enum PricingStrategyType
{
    Fixed,
    Percentage,
    Tiered,
    Promotional
}