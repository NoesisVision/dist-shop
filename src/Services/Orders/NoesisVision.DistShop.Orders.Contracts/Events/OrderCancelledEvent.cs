using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Orders.Contracts.Events;

/// <summary>
/// Event raised when an order is cancelled
/// </summary>
public record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerId,
    string Reason,
    DateTime CancelledAt) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(OrderCancelledEvent);
}