namespace NoesisVision.DistShop.Pricing.Application.DTOs;

/// <summary>
/// DTO for price calculation results
/// </summary>
public class PriceCalculationResultDto
{
    public Guid ProductId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public List<Guid> AppliedRuleIds { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}