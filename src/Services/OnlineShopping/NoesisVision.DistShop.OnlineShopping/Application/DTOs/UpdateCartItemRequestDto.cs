using System.ComponentModel.DataAnnotations;

namespace NoesisVision.DistShop.OnlineShopping.Application.DTOs;

/// <summary>
/// Request DTO for updating cart item quantity
/// </summary>
public class UpdateCartItemRequestDto
{
    [Required]
    [Range(1, 999)]
    public int Quantity { get; set; }
}