using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Orders.Contracts.Events;

/// <summary>
/// Event raised when an order's status changes
/// </summary>
public record OrderStatusChangedEvent(
    Guid OrderId,
    string PreviousStatus,
    string NewStatus,
    DateTime ChangedAt,
    string? Reason = null) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(OrderStatusChangedEvent);
}