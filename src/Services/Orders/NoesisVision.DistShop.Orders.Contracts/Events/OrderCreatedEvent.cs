using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Orders.Contracts.Events;

/// <summary>
/// Event raised when a new order is created
/// </summary>
public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(OrderCreatedEvent);
}