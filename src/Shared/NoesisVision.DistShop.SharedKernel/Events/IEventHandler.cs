namespace NoesisVision.DistShop.SharedKernel.Events;

/// <summary>
/// Interface for handling domain events
/// </summary>
/// <typeparam name="T">The type of event to handle</typeparam>
public interface IEventHandler<in T> where T : class, IEvent
{
    /// <summary>
    /// Handles the specified event
    /// </summary>
    /// <param name="event">The event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}