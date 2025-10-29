namespace NoesisVision.DistShop.Pricing.Application.DTOs;

/// <summary>
/// DTO for price calculation requests
/// </summary>
public class PriceCalculationRequestDto
{
    public Guid ProductId { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CategoryName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerType { get; set; }
    public decimal? OrderAmount { get; set; }
    public int Quantity { get; set; } = 1;
}