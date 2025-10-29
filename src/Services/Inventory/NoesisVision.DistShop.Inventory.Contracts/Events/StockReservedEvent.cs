using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Inventory.Contracts.Events;

/// <summary>
/// Event raised when stock is reserved for a product
/// </summary>
public record StockReservedEvent(
    Guid ProductId,
    int Quantity,
    Guid ReservationId,
    DateTime ExpiresAt) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(StockReservedEvent);
}