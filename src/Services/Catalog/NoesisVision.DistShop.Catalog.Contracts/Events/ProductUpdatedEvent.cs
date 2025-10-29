using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Catalog.Contracts.Events;

/// <summary>
/// Event raised when a product is updated in the catalog
/// </summary>
public record ProductUpdatedEvent(
    Guid ProductId,
    string Name,
    string Description,
    Guid CategoryId,
    decimal Price,
    string Sku) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(ProductUpdatedEvent);
}