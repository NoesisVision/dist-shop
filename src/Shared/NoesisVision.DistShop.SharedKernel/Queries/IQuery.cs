namespace NoesisVision.DistShop.SharedKernel.Queries;

/// <summary>
/// Base interface for all queries in the system
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<TResult>
{
    /// <summary>
    /// Unique identifier for this query instance
    /// </summary>
    Guid QueryId { get; }
}