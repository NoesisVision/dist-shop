using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Pricing.Domain.Exceptions;

/// <summary>
/// Base exception for pricing domain-specific errors
/// </summary>
public class PricingDomainException : DomainException
{
    public PricingDomainException(string message) : base(message)
    {
    }

    public PricingDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when pricing rule operations are invalid
/// </summary>
public class InvalidPricingRuleException : PricingDomainException
{
    public InvalidPricingRuleException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when price calculation fails
/// </summary>
public class PriceCalculationException : PricingDomainException
{
    public PriceCalculationException(string message) : base(message)
    {
    }

    public PriceCalculationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}