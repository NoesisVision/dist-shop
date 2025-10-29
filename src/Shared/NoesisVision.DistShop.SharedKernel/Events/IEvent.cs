namespace NoesisVision.DistShop.SharedKernel.Events;

/// <summary>
/// Base interface for all events in the system
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Type name of the event for serialization and routing
    /// </summary>
    string EventType { get; }
}