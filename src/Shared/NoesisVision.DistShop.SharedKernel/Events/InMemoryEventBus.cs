using System.Collections.Concurrent;

namespace NoesisVision.DistShop.SharedKernel.Events;

/// <summary>
/// Simple in-memory implementation of the event bus for demonstration purposes
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<Func<object, Task>>> _handlers = new();
    
    /// <summary>
    /// Publishes an event to all registered handlers
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        var eventType = typeof(T);
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
            return;
            
        var tasks = new List<Task>();
        
        foreach (var handler in handlers)
        {
            tasks.Add(handler(@event));
        }
        
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// Subscribes to events of a specific type
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    /// <param name="handler">The event handler</param>
    public void Subscribe<T>(Func<T, Task> handler) where T : class, IEvent
    {
        var eventType = typeof(T);
        
        _handlers.AddOrUpdate(
            eventType,
            new ConcurrentBag<Func<object, Task>> { evt => handler((T)evt) },
            (key, existing) =>
            {
                existing.Add(evt => handler((T)evt));
                return existing;
            });
    }
    
    /// <summary>
    /// Unsubscribes from events of a specific type
    /// Note: This is a simplified implementation for demonstration
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    /// <param name="handler">The event handler to remove</param>
    public void Unsubscribe<T>(Func<T, Task> handler) where T : class, IEvent
    {
        // Note: ConcurrentBag doesn't support removal, so this is a simplified implementation
        // In a production system, you'd use a different data structure or approach
        var eventType = typeof(T);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            // For demonstration purposes, we'll recreate the bag without the handler
            // This is not efficient but shows the concept
            var newHandlers = new ConcurrentBag<Func<object, Task>>();
            
            foreach (var existingHandler in handlers)
            {
                // This is a simplified comparison - in practice you'd need a more sophisticated approach
                // to identify and remove specific handlers
                if (!ReferenceEquals(existingHandler, handler))
                {
                    newHandlers.Add(existingHandler);
                }
            }
            
            _handlers.TryUpdate(eventType, newHandlers, handlers);
        }
    }
}