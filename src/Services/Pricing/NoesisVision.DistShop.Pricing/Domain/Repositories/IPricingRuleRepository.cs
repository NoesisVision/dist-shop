using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Pricing.Domain.Aggregates;

namespace NoesisVision.DistShop.Pricing.Domain.Repositories;

/// <summary>
/// Repository interface for pricing rule aggregate
/// </summary>
public interface IPricingRuleRepository : IRepository<PricingRuleAggregate>
{
    /// <summary>
    /// Gets all active pricing rules valid at the specified date
    /// </summary>
    /// <param name="validAt">The date to check validity against</param>
    /// <returns>Collection of active pricing rules</returns>
    Task<IEnumerable<PricingRuleAggregate>> GetActiveRulesAsync(DateTime validAt);

    /// <summary>
    /// Gets pricing rules applicable to a specific product
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="categoryName">Optional category name</param>
    /// <param name="validAt">The date to check validity against</param>
    /// <returns>Collection of applicable pricing rules</returns>
    Task<IEnumerable<PricingRuleAggregate>> GetApplicableRulesForProductAsync(
        Guid productId, 
        string? categoryName = null, 
        DateTime? validAt = null);

    /// <summary>
    /// Gets pricing rules by customer type
    /// </summary>
    /// <param name="customerType">The customer type</param>
    /// <param name="validAt">The date to check validity against</param>
    /// <returns>Collection of pricing rules for the customer type</returns>
    Task<IEnumerable<PricingRuleAggregate>> GetRulesByCustomerTypeAsync(
        string customerType, 
        DateTime? validAt = null);

    /// <summary>
    /// Gets pricing rules by name (for searching)
    /// </summary>
    /// <param name="name">The rule name or partial name</param>
    /// <returns>Collection of matching pricing rules</returns>
    Task<IEnumerable<PricingRuleAggregate>> GetRulesByNameAsync(string name);
}