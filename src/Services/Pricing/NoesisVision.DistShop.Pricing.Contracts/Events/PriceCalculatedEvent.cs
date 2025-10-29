using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Pricing.Contracts.Events;

/// <summary>
/// Event raised when a price is calculated for a product
/// </summary>
public record PriceCalculatedEvent(
    Guid ProductId,
    decimal BasePrice,
    decimal FinalPrice,
    string Currency,
    Guid? CustomerId,
    List<Guid> AppliedRuleIds) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(PriceCalculatedEvent);
}