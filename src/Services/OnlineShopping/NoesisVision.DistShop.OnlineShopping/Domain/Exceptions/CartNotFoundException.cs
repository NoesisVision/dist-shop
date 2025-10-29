namespace NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

/// <summary>
/// Exception thrown when a cart is not found
/// </summary>
public class CartNotFoundException : OnlineShoppingDomainException
{
    public CartNotFoundException(Guid cartId)
        : base($"Cart with ID {cartId} was not found") { }
        
    public CartNotFoundException(Guid customerId, bool byCustomer)
        : base($"Cart for customer {customerId} was not found") { }
}