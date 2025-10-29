namespace NoesisVision.DistShop.SharedKernel.Events;

/// <summary>
/// Event bus abstraction for publishing and subscribing to events
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all subscribers
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
    
    /// <summary>
    /// Subscribes to events of a specific type
    /// </summary>
    /// <typeparam name="T">The event type to subscribe to</typeparam>
    /// <param name="handler">The event handler</param>
    void Subscribe<T>(Func<T, Task> handler) where T : class, IEvent;
    
    /// <summary>
    /// Unsubscribes from events of a specific type
    /// </summary>
    /// <typeparam name="T">The event type to unsubscribe from</typeparam>
    /// <param name="handler">The event handler to remove</param>
    void Unsubscribe<T>(Func<T, Task> handler) where T : class, IEvent;
}