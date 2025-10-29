using NoesisVision.DistShop.OnlineShopping.Application.DTOs;

namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Service interface for cart operations
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Gets a cart by customer ID
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Cart DTO if found, null otherwise</returns>
    Task<CartDto?> GetCartByCustomerIdAsync(Guid customerId);
    
    /// <summary>
    /// Gets or creates a cart for a customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="currency">The currency for new cart</param>
    /// <returns>Cart DTO</returns>
    Task<CartDto> GetOrCreateCartAsync(Guid customerId, string currency = "USD");
    
    /// <summary>
    /// Adds a product to the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="request">Add to cart request</param>
    /// <returns>Updated cart DTO</returns>
    Task<CartDto> AddToCartAsync(Guid customerId, AddToCartRequestDto request);
    
    /// <summary>
    /// Removes a product from the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="productId">The product ID to remove</param>
    /// <returns>Updated cart DTO</returns>
    Task<CartDto> RemoveFromCartAsync(Guid customerId, Guid productId);
    
    /// <summary>
    /// Updates the quantity of a cart item
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="productId">The product ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated cart DTO</returns>
    Task<CartDto> UpdateCartItemAsync(Guid customerId, Guid productId, UpdateCartItemRequestDto request);
    
    /// <summary>
    /// Clears all items from the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Empty cart DTO</returns>
    Task<CartDto> ClearCartAsync(Guid customerId);
    
    /// <summary>
    /// Updates product prices in the cart
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>Updated cart DTO</returns>
    Task<CartDto> RefreshCartPricesAsync(Guid customerId);
}