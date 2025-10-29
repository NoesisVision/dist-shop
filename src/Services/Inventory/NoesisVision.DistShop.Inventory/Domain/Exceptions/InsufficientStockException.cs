namespace NoesisVision.DistShop.Inventory.Domain.Exceptions;

/// <summary>
/// Exception thrown when there is insufficient stock for a requested operation
/// </summary>
public class InsufficientStockException : InventoryDomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(Guid productId, int requestedQuantity, int availableQuantity)
        : base($"Insufficient stock for product {productId}. Requested: {requestedQuantity}, Available: {availableQuantity}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}