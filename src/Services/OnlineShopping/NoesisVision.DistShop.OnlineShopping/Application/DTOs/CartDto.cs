namespace NoesisVision.DistShop.OnlineShopping.Application.DTOs;

/// <summary>
/// Data transfer object for cart information
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Currency { get; set; } = null!;
    public List<CartItemDto> Items { get; set; } = new();
    public int ItemCount { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsEmpty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}