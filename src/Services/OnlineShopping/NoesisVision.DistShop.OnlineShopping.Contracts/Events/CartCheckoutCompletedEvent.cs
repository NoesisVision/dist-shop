using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.OnlineShopping.Contracts.Events;

/// <summary>
/// Event raised when cart checkout is completed successfully
/// </summary>
public record CartCheckoutCompletedEvent(
    Guid CartId,
    Guid CustomerId,
    Guid OrderId,
    decimal TotalAmount,
    string Currency) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(CartCheckoutCompletedEvent);
}