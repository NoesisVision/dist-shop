using NoesisVision.DistShop.Pricing.Application.DTOs;

namespace NoesisVision.DistShop.Pricing.Application.Services;

/// <summary>
/// Service interface for price calculation operations
/// </summary>
public interface IPriceCalculatorService
{
    /// <summary>
    /// Calculates the price for a single product
    /// </summary>
    /// <param name="request">Price calculation request</param>
    /// <returns>Price calculation result</returns>
    Task<PriceCalculationResultDto> CalculatePriceAsync(PriceCalculationRequestDto request);

    /// <summary>
    /// Calculates prices for multiple products (bulk calculation)
    /// </summary>
    /// <param name="requests">Collection of price calculation requests</param>
    /// <returns>Collection of price calculation results</returns>
    Task<IEnumerable<PriceCalculationResultDto>> CalculateBulkPricesAsync(IEnumerable<PriceCalculationRequestDto> requests);

    /// <summary>
    /// Gets applicable pricing rules for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="categoryName">Optional category name</param>
    /// <param name="customerType">Optional customer type</param>
    /// <returns>Collection of applicable pricing rules</returns>
    Task<IEnumerable<PricingRuleDto>> GetApplicableRulesAsync(
        Guid productId, 
        string? categoryName = null, 
        string? customerType = null);
}