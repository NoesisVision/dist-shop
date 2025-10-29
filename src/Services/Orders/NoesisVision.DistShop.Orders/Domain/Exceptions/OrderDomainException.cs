using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Orders.Domain.Exceptions;

/// <summary>
/// Base exception for order domain-specific errors
/// </summary>
public class OrderDomainException : DomainException
{
    public OrderDomainException(string message) : base(message)
    {
    }

    public OrderDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}