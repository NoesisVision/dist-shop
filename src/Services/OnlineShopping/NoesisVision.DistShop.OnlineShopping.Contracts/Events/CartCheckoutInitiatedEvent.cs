using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.OnlineShopping.Contracts.Events;

/// <summary>
/// Event raised when cart checkout is initiated
/// </summary>
public record CartCheckoutInitiatedEvent(
    Guid CartId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    int ItemCount) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(CartCheckoutInitiatedEvent);
}