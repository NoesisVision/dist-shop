using NoesisVision.DistShop.SharedKernel.Events;

namespace NoesisVision.DistShop.SharedKernel.Domain;

/// <summary>
/// Base class for domain aggregates with domain event support
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IEvent> _domainEvents = new();
    
    protected AggregateRoot() : base() { }
    
    protected AggregateRoot(Guid id) : base(id) { }
    
    /// <summary>
    /// Gets the collection of domain events raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void AddDomainEvent(IEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    /// <summary>
    /// Clears all domain events from this aggregate
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}