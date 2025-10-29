namespace NoesisVision.DistShop.Inventory.Application.DTOs;

/// <summary>
/// Data transfer object for stock reservation information
/// </summary>
public class StockReservationDto
{
    public Guid ReservationId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Reference { get; set; }
    public bool IsExpired { get; set; }
}