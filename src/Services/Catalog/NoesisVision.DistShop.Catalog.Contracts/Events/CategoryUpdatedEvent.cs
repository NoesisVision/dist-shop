using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Catalog.Contracts.Events;

/// <summary>
/// Event raised when a category is updated in the catalog
/// </summary>
public record CategoryUpdatedEvent(
    Guid CategoryId,
    string Name,
    string Description,
    Guid? ParentCategoryId) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(CategoryUpdatedEvent);
}