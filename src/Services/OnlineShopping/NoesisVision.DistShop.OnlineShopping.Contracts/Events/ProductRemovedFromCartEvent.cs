using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.OnlineShopping.Contracts.Events;

/// <summary>
/// Event raised when a product is removed from a cart
/// </summary>
public record ProductRemovedFromCartEvent(
    Guid CartId,
    Guid CustomerId,
    Guid ProductId,
    int RemovedQuantity) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(ProductRemovedFromCartEvent);
}