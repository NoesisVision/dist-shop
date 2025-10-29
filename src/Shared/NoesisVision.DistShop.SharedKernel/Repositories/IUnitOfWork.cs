namespace NoesisVision.DistShop.SharedKernel.Repositories;

/// <summary>
/// Unit of work pattern interface for managing transactions
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a new transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}