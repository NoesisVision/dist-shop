namespace NoesisVision.DistShop.Inventory.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid reservation operation is attempted
/// </summary>
public class InvalidReservationException : InventoryDomainException
{
    public Guid ReservationId { get; }

    public InvalidReservationException(Guid reservationId, string reason)
        : base($"Invalid reservation operation for reservation {reservationId}: {reason}")
    {
        ReservationId = reservationId;
    }
}