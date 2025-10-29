using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Inventory.Domain.ValueObjects;

/// <summary>
/// Value object representing a stock reservation
/// </summary>
public class StockReservation : ValueObject
{
    public Guid ReservationId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string? Reference { get; private set; }

    private StockReservation() { } // For EF Core

    public StockReservation(int quantity, TimeSpan duration, string? reference = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be positive", nameof(quantity));

        ReservationId = Guid.NewGuid();
        Quantity = quantity;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.Add(duration);
        Reference = reference;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ReservationId;
        yield return Quantity;
        yield return CreatedAt;
        yield return ExpiresAt;
        yield return Reference ?? string.Empty;
    }
}