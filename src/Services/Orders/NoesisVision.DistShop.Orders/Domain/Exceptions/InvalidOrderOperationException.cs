namespace NoesisVision.DistShop.Orders.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid operation is attempted on an order
/// </summary>
public class InvalidOrderOperationException : OrderDomainException
{
    public InvalidOrderOperationException(string message) : base(message)
    {
    }
}