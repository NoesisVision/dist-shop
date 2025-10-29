using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Pricing.Application.DTOs;
using NoesisVision.DistShop.Pricing.Domain.Aggregates;
using NoesisVision.DistShop.Pricing.Domain.Repositories;
using NoesisVision.DistShop.Pricing.Domain.Services;
using NoesisVision.DistShop.Pricing.Domain.ValueObjects;
using NoesisVision.DistShop.Pricing.Contracts.Events;

namespace NoesisVision.DistShop.Pricing.Application.Services;

/// <summary>
/// Application service for price calculation operations
/// </summary>
public class PriceCalculatorService : IPriceCalculatorService
{
    private readonly IPricingRuleRepository _pricingRuleRepository;
    private readonly PricingEngine _pricingEngine;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;

    public PriceCalculatorService(
        IPricingRuleRepository pricingRuleRepository,
        PricingEngine pricingEngine,
        IEventBus eventBus,
        IUnitOfWork unitOfWork)
    {
        _pricingRuleRepository = pricingRuleRepository ?? throw new ArgumentNullException(nameof(pricingRuleRepository));
        _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PriceCalculationResultDto> CalculatePriceAsync(PriceCalculationRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var basePrice = new Money(request.BasePrice, request.Currency);
        
        var applicableRules = await _pricingRuleRepository.GetApplicableRulesForProductAsync(
            request.ProductId,
            request.CategoryName);

        // Filter rules by customer type and order amount
        var filteredRules = applicableRules.Where(rule => 
            rule.IsApplicableToCustomer(request.CustomerType, request.OrderAmount));

        var calculationResult = _pricingEngine.CalculatePrice(
            basePrice,
            request.ProductId,
            filteredRules,
            request.CategoryName,
            request.CustomerType,
            request.OrderAmount);

        var resultDto = new PriceCalculationResultDto
        {
            ProductId = request.ProductId,
            BasePrice = calculationResult.BasePrice.Amount,
            FinalPrice = calculationResult.FinalPrice.Amount,
            Currency = calculationResult.FinalPrice.Currency,
            DiscountAmount = calculationResult.DiscountAmount,
            DiscountPercentage = calculationResult.DiscountPercentage,
            AppliedRuleIds = calculationResult.AppliedRuleIds.ToList(),
            CalculatedAt = calculationResult.CalculatedAt,
            Quantity = request.Quantity,
            TotalPrice = calculationResult.FinalPrice.Amount * request.Quantity
        };

        // Publish price calculated event
        var priceCalculatedEvent = new PriceCalculatedEvent(
            request.ProductId,
            calculationResult.BasePrice.Amount,
            calculationResult.FinalPrice.Amount,
            calculationResult.FinalPrice.Currency,
            request.CustomerId,
            calculationResult.AppliedRuleIds.ToList());

        await _eventBus.PublishAsync(priceCalculatedEvent);

        return resultDto;
    }

    public async Task<IEnumerable<PriceCalculationResultDto>> CalculateBulkPricesAsync(IEnumerable<PriceCalculationRequestDto> requests)
    {
        if (requests == null)
            throw new ArgumentNullException(nameof(requests));

        var results = new List<PriceCalculationResultDto>();

        foreach (var request in requests)
        {
            var result = await CalculatePriceAsync(request);
            results.Add(result);
        }

        return results;
    }

    public async Task<IEnumerable<PricingRuleDto>> GetApplicableRulesAsync(
        Guid productId, 
        string? categoryName = null, 
        string? customerType = null)
    {
        var rules = await _pricingRuleRepository.GetApplicableRulesForProductAsync(productId, categoryName);
        
        var filteredRules = rules.Where(rule => 
            rule.IsApplicableToCustomer(customerType));

        return filteredRules.Select(MapToDto);
    }

    private static PricingRuleDto MapToDto(PricingRuleAggregate rule)
    {
        return new PricingRuleDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Description = rule.Description,
            StrategyType = rule.Strategy.Type.ToString(),
            Value = rule.Strategy.Value,
            ValidFrom = rule.ValidFrom,
            ValidTo = rule.ValidTo,
            IsActive = rule.IsActive,
            Priority = rule.Priority,
            ApplicableProductCategories = rule.ApplicableProductCategories.ToList(),
            ApplicableProductIds = rule.ApplicableProductIds.ToList(),
            MinimumOrderAmount = rule.MinimumOrderAmount,
            CustomerType = rule.CustomerType
        };
    }
}