using NoesisVision.DistShop.Orders.Domain.ValueObjects;

namespace NoesisVision.DistShop.Orders.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid order state transition is attempted
/// </summary>
public class InvalidOrderStateTransitionException : OrderDomainException
{
    public InvalidOrderStateTransitionException(OrderStatus currentStatus, OrderStatus targetStatus)
        : base($"Cannot transition order from {currentStatus} to {targetStatus}")
    {
    }
}