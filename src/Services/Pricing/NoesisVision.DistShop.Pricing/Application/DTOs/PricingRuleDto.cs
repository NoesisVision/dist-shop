namespace NoesisVision.DistShop.Pricing.Application.DTOs;

/// <summary>
/// DTO for pricing rule information
/// </summary>
public class PricingRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StrategyType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public List<string> ApplicableProductCategories { get; set; } = new();
    public List<Guid> ApplicableProductIds { get; set; } = new();
    public decimal? MinimumOrderAmount { get; set; }
    public string? CustomerType { get; set; }
}