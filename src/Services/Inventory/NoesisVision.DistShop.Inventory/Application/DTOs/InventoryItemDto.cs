namespace NoesisVision.DistShop.Inventory.Application.DTOs;

/// <summary>
/// Data transfer object for inventory item information
/// </summary>
public class InventoryItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<StockReservationDto> ActiveReservations { get; set; } = new();
}