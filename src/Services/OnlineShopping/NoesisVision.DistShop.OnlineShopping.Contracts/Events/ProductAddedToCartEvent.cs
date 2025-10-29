using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.OnlineShopping.Contracts.Events;

/// <summary>
/// Event raised when a product is added to a cart
/// </summary>
public record ProductAddedToCartEvent(
    Guid CartId,
    Guid CustomerId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(ProductAddedToCartEvent);
}