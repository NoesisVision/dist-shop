namespace NoesisVision.DistShop.SharedKernel.Commands;

/// <summary>
/// Base interface for all commands in the system
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Unique identifier for this command instance
    /// </summary>
    Guid CommandId { get; }
    
    /// <summary>
    /// Timestamp when the command was issued
    /// </summary>
    DateTime IssuedAt { get; }
}