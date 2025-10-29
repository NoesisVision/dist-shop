using System.ComponentModel.DataAnnotations;

namespace NoesisVision.DistShop.OnlineShopping.Application.DTOs;

/// <summary>
/// Request DTO for cart checkout
/// </summary>
public class CheckoutRequestDto
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [StringLength(500)]
    public string? ShippingAddress { get; set; }
    
    [StringLength(100)]
    public string? PaymentMethod { get; set; }
    
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}