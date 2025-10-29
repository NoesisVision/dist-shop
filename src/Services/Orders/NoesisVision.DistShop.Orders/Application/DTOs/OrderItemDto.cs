namespace NoesisVision.DistShop.Orders.Application.DTOs;

/// <summary>
/// Data transfer object for order items
/// </summary>
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string ProductSku { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
}