namespace NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid cart operation is attempted
/// </summary>
public class InvalidCartOperationException : OnlineShoppingDomainException
{
    public InvalidCartOperationException(string operation)
        : base($"Invalid cart operation: {operation}") { }
        
    public InvalidCartOperationException(string operation, Exception innerException)
        : base($"Invalid cart operation: {operation}", innerException) { }
}