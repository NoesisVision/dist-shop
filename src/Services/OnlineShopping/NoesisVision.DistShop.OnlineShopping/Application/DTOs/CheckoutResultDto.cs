namespace NoesisVision.DistShop.OnlineShopping.Application.DTOs;

/// <summary>
/// Result DTO for checkout operation
/// </summary>
public class CheckoutResultDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime CheckoutCompletedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}