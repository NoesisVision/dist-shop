using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Inventory.Domain.Exceptions;

/// <summary>
/// Base exception for inventory domain-specific errors
/// </summary>
public class InventoryDomainException : DomainException
{
    public InventoryDomainException(string message) : base(message) { }
    
    public InventoryDomainException(string message, Exception innerException) : base(message, innerException) { }
}