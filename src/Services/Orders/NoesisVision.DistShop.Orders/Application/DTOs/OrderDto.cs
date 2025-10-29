using NoesisVision.DistShop.Orders.Domain.ValueObjects;

namespace NoesisVision.DistShop.Orders.Application.DTOs;

/// <summary>
/// Data transfer object for orders
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CancellationReason { get; set; }
}