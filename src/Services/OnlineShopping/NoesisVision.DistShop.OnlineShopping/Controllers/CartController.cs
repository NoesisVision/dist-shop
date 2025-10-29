using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.OnlineShopping.Application.DTOs;
using NoesisVision.DistShop.OnlineShopping.Application.Services;
using NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

namespace NoesisVision.DistShop.OnlineShopping.Controllers;

/// <summary>
/// REST API controller for cart operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the cart for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Cart information</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> GetCart(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            
            if (cart == null)
                return NotFound($"Cart not found for customer {customerId}");

            return Ok(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the cart");
        }
    }

    /// <summary>
    /// Gets or creates a cart for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="currency">The currency (optional, defaults to USD)</param>
    /// <returns>Cart information</returns>
    [HttpPost("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> GetOrCreateCart(Guid customerId, [FromQuery] string currency = "USD")
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var existingCart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (existingCart != null)
                return Ok(existingCart);

            var cart = await _cartService.GetOrCreateCartAsync(customerId, currency);
            return CreatedAtAction(nameof(GetCart), new { customerId }, cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating cart for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the cart");
        }
    }

    /// <summary>
    /// Adds a product to the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="request">Add to cart request</param>
    /// <returns>Updated cart information</returns>
    [HttpPost("customer/{customerId:guid}/items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> AddToCart(Guid customerId, [FromBody] AddToCartRequestDto request)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cart = await _cartService.AddToCartAsync(customerId, request);
            return Ok(cart);
        }
        catch (CartNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cart not found for customer {CustomerId}", customerId);
            return NotFound(ex.Message);
        }
        catch (InvalidCartOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid cart operation for customer {CustomerId}", customerId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to cart for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the product to cart");
        }
    }

    /// <summary>
    /// Updates the quantity of a cart item
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="productId">The product ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated cart information</returns>
    [HttpPut("customer/{customerId:guid}/items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid customerId, Guid productId, [FromBody] UpdateCartItemRequestDto request)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            if (productId == Guid.Empty)
                return BadRequest("Product ID cannot be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cart = await _cartService.UpdateCartItemAsync(customerId, productId, request);
            return Ok(cart);
        }
        catch (CartNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cart not found for customer {CustomerId}", customerId);
            return NotFound(ex.Message);
        }
        catch (InvalidCartOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid cart operation for customer {CustomerId}, product {ProductId}", customerId, productId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item for customer {CustomerId}, product {ProductId}", customerId, productId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the cart item");
        }
    }

    /// <summary>
    /// Removes a product from the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="productId">The product ID</param>
    /// <returns>Updated cart information</returns>
    [HttpDelete("customer/{customerId:guid}/items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> RemoveFromCart(Guid customerId, Guid productId)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            if (productId == Guid.Empty)
                return BadRequest("Product ID cannot be empty");

            var cart = await _cartService.RemoveFromCartAsync(customerId, productId);
            return Ok(cart);
        }
        catch (CartNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cart not found for customer {CustomerId}", customerId);
            return NotFound(ex.Message);
        }
        catch (InvalidCartOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid cart operation for customer {CustomerId}, product {ProductId}", customerId, productId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product from cart for customer {CustomerId}, product {ProductId}", customerId, productId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the product from cart");
        }
    }

    /// <summary>
    /// Clears all items from the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Empty cart information</returns>
    [HttpDelete("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> ClearCart(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var cart = await _cartService.ClearCartAsync(customerId);
            return Ok(cart);
        }
        catch (CartNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cart not found for customer {CustomerId}", customerId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while clearing the cart");
        }
    }

    /// <summary>
    /// Refreshes cart prices with current pricing
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Updated cart information</returns>
    [HttpPost("customer/{customerId:guid}/refresh-prices")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> RefreshCartPrices(Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty)
                return BadRequest("Customer ID cannot be empty");

            var cart = await _cartService.RefreshCartPricesAsync(customerId);
            return Ok(cart);
        }
        catch (CartNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cart not found for customer {CustomerId}", customerId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cart prices for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while refreshing cart prices");
        }
    }
}