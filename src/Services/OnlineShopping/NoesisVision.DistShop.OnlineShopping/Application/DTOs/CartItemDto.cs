namespace NoesisVision.DistShop.OnlineShopping.Application.DTOs;

/// <summary>
/// Data transfer object for cart item information
/// </summary>
public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public DateTime AddedAt { get; set; }
}