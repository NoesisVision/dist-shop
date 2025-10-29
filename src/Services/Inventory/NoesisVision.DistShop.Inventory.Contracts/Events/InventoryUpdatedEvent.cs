using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Inventory.Contracts.Events;

/// <summary>
/// Event raised when inventory levels are updated
/// </summary>
public record InventoryUpdatedEvent(
    Guid ProductId,
    int NewQuantity,
    int PreviousQuantity,
    string Reason) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(InventoryUpdatedEvent);
}