using NoesisVision.DistShop.OnlineShopping.Application.DTOs;

namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Service interface for checkout operations
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Initiates checkout process for a customer's cart
    /// </summary>
    /// <param name="request">Checkout request</param>
    /// <returns>Checkout result</returns>
    Task<CheckoutResultDto> CheckoutAsync(CheckoutRequestDto request);
    
    /// <summary>
    /// Validates cart for checkout
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>True if cart is valid for checkout</returns>
    Task<bool> ValidateCartForCheckoutAsync(Guid customerId);
}