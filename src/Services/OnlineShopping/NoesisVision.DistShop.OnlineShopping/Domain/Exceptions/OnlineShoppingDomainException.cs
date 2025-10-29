using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

/// <summary>
/// Base exception for online shopping domain
/// </summary>
public abstract class OnlineShoppingDomainException : DomainException
{
    protected OnlineShoppingDomainException(string message) : base(message) { }
    
    protected OnlineShoppingDomainException(string message, Exception innerException) : base(message, innerException) { }
}