using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.SharedKernel.Repositories;

/// <summary>
/// Generic repository interface for aggregate roots
/// </summary>
/// <typeparam name="T">The aggregate root type</typeparam>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Gets an aggregate by its identifier
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <returns>The aggregate if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all aggregates
    /// </summary>
    /// <returns>Collection of all aggregates</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Adds a new aggregate
    /// </summary>
    /// <param name="entity">The aggregate to add</param>
    Task AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing aggregate
    /// </summary>
    /// <param name="entity">The aggregate to update</param>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an aggregate by its identifier
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    Task DeleteAsync(Guid id);
}