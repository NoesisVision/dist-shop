using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.Pricing.Contracts.Events;

/// <summary>
/// Event raised when a pricing rule is updated
/// </summary>
public record PricingRuleUpdatedEvent(
    Guid RuleId,
    string Name,
    string StrategyType,
    decimal Value,
    DateTime ValidFrom,
    DateTime ValidTo) : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = nameof(PricingRuleUpdatedEvent);
}