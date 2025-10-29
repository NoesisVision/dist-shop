using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Inventory.Contracts.Events;

/// <summary>
/// Event raised when stock levels fall below the reorder threshold
/// </summary>
public record LowStockAlertEvent(
    Guid ProductId,
    int CurrentQuantity,
    int ReorderLevel,
    int SuggestedReorderQuantity) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(LowStockAlertEvent);
}