using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Inventory.Contracts.Events;

/// <summary>
/// Event raised when a stock reservation is released
/// </summary>
public record StockReservationReleasedEvent(
    Guid ProductId,
    int Quantity,
    Guid ReservationId,
    string Reason) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(StockReservationReleasedEvent);
}