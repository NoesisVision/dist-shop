using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.OnlineShopping.Application.DTOs;
using NoesisVision.DistShop.OnlineShopping.Application.Services;
using NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

namespace NoesisVision.DistShop.OnlineShopping.Controllers;

/// <summary>
/// REST API controller for checkout operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ICheckoutService checkoutService, ILogger<CheckoutController> logger)
    {
        _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiates checkout process for a customer's cart
    /// </summary>
    /// <param name="request">Checkout request</param>
    /// <returns>Checkout result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CheckoutResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CheckoutResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckoutResultDto>> Checkout([FromBody] CheckoutRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.CustomerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var result = await _checkoutService.CheckoutAsync(request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Checkout failed for customer {CustomerId}: {ErrorMessage}", 
                    request.CustomerId, result.ErrorMessage);
                return BadRequest(result);
            }

            _logger.LogInformation("Checkout completed successfully for customer {CustomerId}, order {OrderId}", 
                request.CustomerId, result.OrderId);
            
            return Ok(result);
        }
        catch (CartNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cart not found during checkout for customer {CustomerId}", request.CustomerId);
            return NotFound(new CheckoutResultDto
            {
                CustomerId = request.CustomerId,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        catch (InvalidCartOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid cart operation during checkout for customer {CustomerId}", request.CustomerId);
            return BadRequest(new CheckoutResultDto
            {
                CustomerId = request.CustomerId,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during checkout for customer {CustomerId}", request.CustomerId);
            return StatusCode(StatusCodes.Status500InternalServerError, new CheckoutResultDto
            {
                CustomerId = request.CustomerId,
                Success = false,
                ErrorMessage = "An error occurred during checkout"
            });
        }
    }

    /// <summary>
    /// Validates if a customer's cart is ready for checkout
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/{customerId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ValidateCheckout(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var isValid = await _checkoutService.ValidateCartForCheckoutAsync(customerId);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating checkout for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while validating checkout");
        }
    }

    /// <summary>
    /// Gets checkout validation details for a customer's cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Detailed validation information</returns>
    [HttpGet("validate/{customerId:guid}/details")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetCheckoutValidationDetails(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var isValid = await _checkoutService.ValidateCartForCheckoutAsync(customerId);
            
            var validationDetails = new
            {
                CustomerId = customerId,
                IsValid = isValid,
                ValidationChecks = new
                {
                    HasCart = true, // Would be determined by actual validation
                    CartNotEmpty = true, // Would be determined by actual validation
                    InventoryAvailable = true, // Would be determined by inventory service integration
                    PricingValid = true, // Would be determined by pricing service integration
                    CustomerEligible = true // Would be determined by customer service integration
                },
                ValidationTimestamp = DateTime.UtcNow
            };

            return Ok(validationDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checkout validation details for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting validation details");
        }
    }
}