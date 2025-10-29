using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.Inventory.Domain.ValueObjects;
using NoesisVision.DistShop.Inventory.Domain.Exceptions;
using NoesisVision.DistShop.Inventory.Contracts.Events;

namespace NoesisVision.DistShop.Inventory.Domain.Aggregates;

/// <summary>
/// Aggregate root for inventory item with stock management business rules
/// </summary>
public class InventoryItem : AggregateRoot
{
    private readonly List<StockReservation> _reservations = new();

    public Guid ProductId { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ReorderLevel { get; private set; }
    public int MaxStockLevel { get; private set; }
    public DateTime LastUpdated { get; private set; }

    /// <summary>
    /// Gets the total reserved quantity from all active reservations
    /// </summary>
    public int ReservedQuantity => _reservations
        .Where(r => !r.IsExpired)
        .Sum(r => r.Quantity);

    /// <summary>
    /// Gets the total quantity including available and reserved stock
    /// </summary>
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;

    /// <summary>
    /// Gets read-only collection of active reservations
    /// </summary>
    public IReadOnlyCollection<StockReservation> Reservations => _reservations
        .Where(r => !r.IsExpired)
        .ToList()
        .AsReadOnly();

    private InventoryItem() { } // For EF Core

    public InventoryItem(Guid productId, int initialQuantity, int reorderLevel, int maxStockLevel)
    {
        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative", nameof(initialQuantity));
        if (reorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative", nameof(reorderLevel));
        if (maxStockLevel <= 0)
            throw new ArgumentException("Max stock level must be positive", nameof(maxStockLevel));
        if (reorderLevel >= maxStockLevel)
            throw new ArgumentException("Reorder level must be less than max stock level", nameof(reorderLevel));

        Id = Guid.NewGuid();
        ProductId = productId;
        AvailableQuantity = initialQuantity;
        ReorderLevel = reorderLevel;
        MaxStockLevel = maxStockLevel;
        LastUpdated = DateTime.UtcNow;

        // Check for low stock on creation
        CheckLowStockAlert();
    }

    /// <summary>
    /// Reserves stock for a specific quantity
    /// </summary>
    /// <param name="quantity">Quantity to reserve</param>
    /// <param name="duration">How long the reservation should last</param>
    /// <param name="reference">Optional reference for the reservation</param>
    /// <returns>The created reservation</returns>
    public StockReservation ReserveStock(int quantity, TimeSpan duration, string? reference = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be positive", nameof(quantity));

        // Clean up expired reservations first
        CleanupExpiredReservations();

        if (AvailableQuantity < quantity)
            throw new InsufficientStockException(ProductId, quantity, AvailableQuantity);

        var reservation = new StockReservation(quantity, duration, reference);
        _reservations.Add(reservation);
        
        AvailableQuantity -= quantity;
        LastUpdated = DateTime.UtcNow;

        AddDomainEvent(new StockReservedEvent(ProductId, quantity, reservation.ReservationId, reservation.ExpiresAt));
        
        return reservation;
    }

    /// <summary>
    /// Releases a stock reservation, making the stock available again
    /// </summary>
    /// <param name="reservationId">ID of the reservation to release</param>
    /// <param name="reason">Reason for releasing the reservation</param>
    public void ReleaseReservation(Guid reservationId, string reason = "Manual release")
    {
        var reservation = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
        if (reservation == null)
            throw new InvalidReservationException(reservationId, "Reservation not found");

        _reservations.Remove(reservation);
        AvailableQuantity += reservation.Quantity;
        LastUpdated = DateTime.UtcNow;

        AddDomainEvent(new StockReservationReleasedEvent(ProductId, reservation.Quantity, reservationId, reason));
    }

    /// <summary>
    /// Confirms a reservation by permanently removing the stock
    /// </summary>
    /// <param name="reservationId">ID of the reservation to confirm</param>
    public void ConfirmReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.ReservationId == reservationId);
        if (reservation == null)
            throw new InvalidReservationException(reservationId, "Reservation not found");

        if (reservation.IsExpired)
            throw new InvalidReservationException(reservationId, "Reservation has expired");

        _reservations.Remove(reservation);
        LastUpdated = DateTime.UtcNow;

        // Stock was already deducted when reserved, so no need to adjust AvailableQuantity
        AddDomainEvent(new InventoryUpdatedEvent(ProductId, TotalQuantity, TotalQuantity + reservation.Quantity, "Reservation confirmed"));
    }

    /// <summary>
    /// Adjusts stock levels (for restocking, returns, etc.)
    /// </summary>
    /// <param name="quantityChange">Positive for additions, negative for reductions</param>
    /// <param name="reason">Reason for the adjustment</param>
    public void AdjustStock(int quantityChange, string reason)
    {
        var previousTotal = TotalQuantity;
        var newAvailable = AvailableQuantity + quantityChange;

        if (newAvailable < 0)
            throw new InsufficientStockException(ProductId, Math.Abs(quantityChange), AvailableQuantity);

        if (newAvailable > MaxStockLevel)
            throw new InventoryDomainException($"Stock adjustment would exceed maximum stock level of {MaxStockLevel}");

        AvailableQuantity = newAvailable;
        LastUpdated = DateTime.UtcNow;

        AddDomainEvent(new InventoryUpdatedEvent(ProductId, TotalQuantity, previousTotal, reason));
        
        CheckLowStockAlert();
    }

    /// <summary>
    /// Updates reorder and max stock levels
    /// </summary>
    /// <param name="newReorderLevel">New reorder level</param>
    /// <param name="newMaxStockLevel">New maximum stock level</param>
    public void UpdateStockLevels(int newReorderLevel, int newMaxStockLevel)
    {
        if (newReorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative", nameof(newReorderLevel));
        if (newMaxStockLevel <= 0)
            throw new ArgumentException("Max stock level must be positive", nameof(newMaxStockLevel));
        if (newReorderLevel >= newMaxStockLevel)
            throw new ArgumentException("Reorder level must be less than max stock level");

        ReorderLevel = newReorderLevel;
        MaxStockLevel = newMaxStockLevel;
        LastUpdated = DateTime.UtcNow;

        CheckLowStockAlert();
    }

    /// <summary>
    /// Checks if stock is below reorder level and raises alert if needed
    /// </summary>
    private void CheckLowStockAlert()
    {
        if (TotalQuantity <= ReorderLevel)
        {
            var suggestedReorder = MaxStockLevel - TotalQuantity;
            AddDomainEvent(new LowStockAlertEvent(ProductId, TotalQuantity, ReorderLevel, suggestedReorder));
        }
    }

    /// <summary>
    /// Removes expired reservations and makes their stock available again
    /// </summary>
    private void CleanupExpiredReservations()
    {
        var expiredReservations = _reservations.Where(r => r.IsExpired).ToList();
        
        foreach (var expired in expiredReservations)
        {
            _reservations.Remove(expired);
            AvailableQuantity += expired.Quantity;
            
            AddDomainEvent(new StockReservationReleasedEvent(
                ProductId, 
                expired.Quantity, 
                expired.ReservationId, 
                "Reservation expired"));
        }

        if (expiredReservations.Any())
        {
            LastUpdated = DateTime.UtcNow;
        }
    }
}