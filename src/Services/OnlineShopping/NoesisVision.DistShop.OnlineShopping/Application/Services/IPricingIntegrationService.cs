namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Service interface for integrating with Pricing Service
/// </summary>
public interface IPricingIntegrationService
{
    /// <summary>
    /// Gets the current price for a product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="customerId">The customer ID for customer-specific pricing</param>
    /// <param name="quantity">The quantity for volume pricing</param>
    /// <returns>Current unit price</returns>
    Task<decimal> GetProductPriceAsync(Guid productId, Guid customerId, int quantity = 1);
    
    /// <summary>
    /// Gets current prices for multiple products
    /// </summary>
    /// <param name="productIds">The product IDs</param>
    /// <param name="customerId">The customer ID for customer-specific pricing</param>
    /// <returns>Dictionary of product ID to unit price</returns>
    Task<Dictionary<Guid, decimal>> GetProductPricesAsync(IEnumerable<Guid> productIds, Guid customerId);
    
    /// <summary>
    /// Calculates total cart pricing including discounts and promotions
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="items">Cart items for pricing calculation</param>
    /// <returns>Pricing calculation result</returns>
    Task<CartPricingResult> CalculateCartPricingAsync(Guid customerId, IEnumerable<CartItemForPricing> items);
}

/// <summary>
/// Cart item information for pricing calculations
/// </summary>
public class CartItemForPricing
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal CurrentUnitPrice { get; set; }
}

/// <summary>
/// Result of cart pricing calculation
/// </summary>
public class CartPricingResult
{
    public Dictionary<Guid, decimal> ItemPrices { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<string> AppliedPromotions { get; set; } = new();
}