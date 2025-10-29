namespace NoesisVision.DistShop.SharedKernel.Domain;

/// <summary>
/// Base exception class for domain-specific exceptions
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}