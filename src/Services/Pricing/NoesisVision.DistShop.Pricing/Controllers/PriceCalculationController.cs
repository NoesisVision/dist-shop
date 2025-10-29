using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.Pricing.Application.DTOs;
using NoesisVision.DistShop.Pricing.Application.Services;

namespace NoesisVision.DistShop.Pricing.Controllers;

/// <summary>
/// Controller for price calculation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PriceCalculationController : ControllerBase
{
    private readonly IPriceCalculatorService _priceCalculatorService;
    private readonly ILogger<PriceCalculationController> _logger;

    public PriceCalculationController(
        IPriceCalculatorService priceCalculatorService,
        ILogger<PriceCalculationController> logger)
    {
        _priceCalculatorService = priceCalculatorService ?? throw new ArgumentNullException(nameof(priceCalculatorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the price for a single product
    /// </summary>
    /// <param name="request">Price calculation request</param>
    /// <returns>Price calculation result</returns>
    [HttpPost("calculate")]
    public async Task<ActionResult<PriceCalculationResultDto>> CalculatePrice([FromBody] PriceCalculationRequestDto request)
    {
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            if (request.ProductId == Guid.Empty)
                return BadRequest("Product ID is required");

            if (request.BasePrice < 0)
                return BadRequest("Base price cannot be negative");

            var result = await _priceCalculatorService.CalculatePriceAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid price calculation request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for product {ProductId}", request?.ProductId);
            return StatusCode(500, "An error occurred while calculating the price");
        }
    }

    /// <summary>
    /// Calculates prices for multiple products in bulk
    /// </summary>
    /// <param name="requests">Collection of price calculation requests</param>
    /// <returns>Collection of price calculation results</returns>
    [HttpPost("calculate/bulk")]
    public async Task<ActionResult<IEnumerable<PriceCalculationResultDto>>> CalculateBulkPrices([FromBody] IEnumerable<PriceCalculationRequestDto> requests)
    {
        try
        {
            if (requests == null || !requests.Any())
                return BadRequest("At least one request is required");

            var invalidRequests = requests.Where(r => r.ProductId == Guid.Empty || r.BasePrice < 0).ToList();
            if (invalidRequests.Any())
                return BadRequest("All requests must have valid product IDs and non-negative base prices");

            var results = await _priceCalculatorService.CalculateBulkPricesAsync(requests);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid bulk price calculation request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating bulk prices");
            return StatusCode(500, "An error occurred while calculating bulk prices");
        }
    }

    /// <summary>
    /// Gets applicable pricing rules for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="categoryName">Optional category name</param>
    /// <param name="customerType">Optional customer type</param>
    /// <returns>Collection of applicable pricing rules</returns>
    [HttpGet("rules/applicable")]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetApplicableRules(
        [FromQuery] Guid productId,
        [FromQuery] string? categoryName = null,
        [FromQuery] string? customerType = null)
    {
        try
        {
            if (productId == Guid.Empty)
                return BadRequest("Product ID is required");

            var rules = await _priceCalculatorService.GetApplicableRulesAsync(productId, categoryName, customerType);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicable rules for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while retrieving applicable rules");
        }
    }
}