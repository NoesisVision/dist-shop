using System.ComponentModel.DataAnnotations;

namespace NoesisVision.DistShop.OnlineShopping.Application.DTOs;

/// <summary>
/// Request DTO for adding products to cart
/// </summary>
public class AddToCartRequestDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string ProductName { get; set; } = null!;
    
    [Required]
    [Range(1, 999)]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";
}