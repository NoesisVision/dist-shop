using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.OnlineShopping.Contracts.Events;

/// <summary>
/// Event raised when cart item quantity is updated
/// </summary>
public record CartQuantityUpdatedEvent(
    Guid CartId,
    Guid CustomerId,
    Guid ProductId,
    int OldQuantity,
    int NewQuantity) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(CartQuantityUpdatedEvent);
}