using NoesisVision.DistShop.Pricing.Domain.Aggregates;
using NoesisVision.DistShop.Pricing.Domain.ValueObjects;
using NoesisVision.DistShop.Pricing.Domain.Exceptions;

namespace NoesisVision.DistShop.Pricing.Domain.Services;

/// <summary>
/// Domain service for calculating prices based on pricing rules
/// </summary>
public class PricingEngine
{
    /// <summary>
    /// Calculates the final price for a product based on applicable pricing rules
    /// </summary>
    /// <param name="basePrice">The base price of the product</param>
    /// <param name="productId">The product identifier</param>
    /// <param name="applicableRules">List of applicable pricing rules ordered by priority</param>
    /// <param name="categoryName">Optional product category name</param>
    /// <param name="customerType">Optional customer type</param>
    /// <param name="orderAmount">Optional total order amount</param>
    /// <returns>Calculated price result</returns>
    public PriceCalculationResult CalculatePrice(
        Money basePrice,
        Guid productId,
        IEnumerable<PricingRuleAggregate> applicableRules,
        string? categoryName = null,
        string? customerType = null,
        decimal? orderAmount = null)
    {
        if (basePrice == null)
            throw new ArgumentNullException(nameof(basePrice));

        var currentPrice = basePrice;
        var appliedRules = new List<Guid>();

        // Apply rules in priority order (higher priority first)
        var orderedRules = applicableRules
            .Where(rule => rule.IsApplicableToProduct(productId, categoryName) && 
                          rule.IsApplicableToCustomer(customerType, orderAmount))
            .OrderByDescending(rule => rule.Priority)
            .ToList();

        foreach (var rule in orderedRules)
        {
            try
            {
                var newPrice = ApplyPricingStrategy(currentPrice, rule.Strategy);
                if (newPrice.Amount != currentPrice.Amount)
                {
                    currentPrice = newPrice;
                    appliedRules.Add(rule.Id);
                }
            }
            catch (Exception ex)
            {
                throw new PriceCalculationException($"Failed to apply pricing rule {rule.Id}: {rule.Name}", ex);
            }
        }

        return new PriceCalculationResult(
            basePrice,
            currentPrice,
            appliedRules,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Applies a specific pricing strategy to a price
    /// </summary>
    /// <param name="currentPrice">The current price</param>
    /// <param name="strategy">The pricing strategy to apply</param>
    /// <returns>The new price after applying the strategy</returns>
    private Money ApplyPricingStrategy(Money currentPrice, PricingStrategy strategy)
    {
        return strategy.Type switch
        {
            PricingStrategyType.Fixed => new Money(strategy.Value, currentPrice.Currency),
            
            PricingStrategyType.Percentage => ApplyPercentageStrategy(currentPrice, strategy),
            
            PricingStrategyType.Tiered => ApplyTieredStrategy(currentPrice, strategy),
            
            PricingStrategyType.Promotional => ApplyPromotionalStrategy(currentPrice, strategy),
            
            _ => throw new PriceCalculationException($"Unsupported pricing strategy: {strategy.Type}")
        };
    }

    private Money ApplyPercentageStrategy(Money currentPrice, PricingStrategy strategy)
    {
        var multiplier = strategy.Value / 100m;
        var adjustment = currentPrice.Amount * multiplier;
        
        // Determine if it's a markup or discount based on parameters
        var isDiscount = strategy.Parameters.ContainsKey("isDiscount") && 
                        (bool)strategy.Parameters["isDiscount"];
        
        var newAmount = isDiscount 
            ? currentPrice.Amount - adjustment 
            : currentPrice.Amount + adjustment;

        return new Money(Math.Max(0, newAmount), currentPrice.Currency);
    }

    private Money ApplyTieredStrategy(Money currentPrice, PricingStrategy strategy)
    {
        if (!strategy.Parameters.ContainsKey("tiers"))
            throw new PriceCalculationException("Tiered pricing strategy requires 'tiers' parameter");

        // Simplified tiered pricing - in real implementation, this would be more complex
        var tiers = (Dictionary<string, decimal>)strategy.Parameters["tiers"];
        
        // Find applicable tier based on current price
        var applicableTier = tiers
            .Where(t => decimal.Parse(t.Key) <= currentPrice.Amount)
            .OrderByDescending(t => decimal.Parse(t.Key))
            .FirstOrDefault();

        if (applicableTier.Key != null)
        {
            var discount = applicableTier.Value / 100m;
            var newAmount = currentPrice.Amount * (1 - discount);
            return new Money(newAmount, currentPrice.Currency);
        }

        return currentPrice;
    }

    private Money ApplyPromotionalStrategy(Money currentPrice, PricingStrategy strategy)
    {
        var discountPercentage = strategy.Value / 100m;
        var discountAmount = currentPrice.Amount * discountPercentage;
        var newAmount = currentPrice.Amount - discountAmount;

        return new Money(Math.Max(0, newAmount), currentPrice.Currency);
    }
}

/// <summary>
/// Result of a price calculation operation
/// </summary>
public class PriceCalculationResult
{
    public Money BasePrice { get; }
    public Money FinalPrice { get; }
    public IReadOnlyList<Guid> AppliedRuleIds { get; }
    public DateTime CalculatedAt { get; }

    public PriceCalculationResult(
        Money basePrice,
        Money finalPrice,
        IEnumerable<Guid> appliedRuleIds,
        DateTime calculatedAt)
    {
        BasePrice = basePrice ?? throw new ArgumentNullException(nameof(basePrice));
        FinalPrice = finalPrice ?? throw new ArgumentNullException(nameof(finalPrice));
        AppliedRuleIds = appliedRuleIds?.ToList().AsReadOnly() ?? new List<Guid>().AsReadOnly();
        CalculatedAt = calculatedAt;
    }

    public decimal DiscountAmount => BasePrice.Amount - FinalPrice.Amount;
    public decimal DiscountPercentage => BasePrice.Amount > 0 ? (DiscountAmount / BasePrice.Amount) * 100 : 0;
}