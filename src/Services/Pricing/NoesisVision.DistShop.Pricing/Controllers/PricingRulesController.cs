using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Pricing.Application.DTOs;
using NoesisVision.DistShop.Pricing.Domain.Aggregates;
using NoesisVision.DistShop.Pricing.Domain.Repositories;
using NoesisVision.DistShop.Pricing.Domain.ValueObjects;
using NoesisVision.DistShop.Pricing.Domain.Exceptions;

namespace NoesisVision.DistShop.Pricing.Controllers;

/// <summary>
/// Controller for pricing rule management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PricingRulesController : ControllerBase
{
    private readonly IPricingRuleRepository _pricingRuleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PricingRulesController> _logger;

    public PricingRulesController(
        IPricingRuleRepository pricingRuleRepository,
        IUnitOfWork unitOfWork,
        ILogger<PricingRulesController> logger)
    {
        _pricingRuleRepository = pricingRuleRepository ?? throw new ArgumentNullException(nameof(pricingRuleRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all pricing rules
    /// </summary>
    /// <returns>Collection of pricing rules</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetAllRules()
    {
        try
        {
            var rules = await _pricingRuleRepository.GetAllAsync();
            var ruleDtos = rules.Select(MapToDto);
            return Ok(ruleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all pricing rules");
            return StatusCode(500, "An error occurred while retrieving pricing rules");
        }
    }

    /// <summary>
    /// Gets a pricing rule by ID
    /// </summary>
    /// <param name="id">Rule identifier</param>
    /// <returns>Pricing rule</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PricingRuleDto>> GetRule(Guid id)
    {
        try
        {
            var rule = await _pricingRuleRepository.GetByIdAsync(id);
            return Ok(MapToDto(rule));
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Pricing rule with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pricing rule {RuleId}", id);
            return StatusCode(500, "An error occurred while retrieving the pricing rule");
        }
    }

    /// <summary>
    /// Creates a new pricing rule
    /// </summary>
    /// <param name="request">Create pricing rule request</param>
    /// <returns>Created pricing rule</returns>
    [HttpPost]
    public async Task<ActionResult<PricingRuleDto>> CreateRule([FromBody] CreatePricingRuleRequestDto request)
    {
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Rule name is required");

            if (request.ValidFrom >= request.ValidTo)
                return BadRequest("Valid from date must be before valid to date");

            var strategy = new PricingStrategy(request.StrategyType, request.Value, request.Parameters);
            
            var rule = PricingRuleAggregate.Create(
                request.Name,
                request.Description ?? string.Empty,
                strategy,
                request.ValidFrom,
                request.ValidTo,
                request.Priority);

            if (request.ApplicableProductIds?.Any() == true)
                rule.SetApplicableProducts(request.ApplicableProductIds);

            if (request.ApplicableProductCategories?.Any() == true)
                rule.SetApplicableCategories(request.ApplicableProductCategories);

            if (request.MinimumOrderAmount.HasValue)
                rule.SetMinimumOrderAmount(request.MinimumOrderAmount);

            if (!string.IsNullOrWhiteSpace(request.CustomerType))
                rule.SetCustomerType(request.CustomerType);

            await _pricingRuleRepository.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, MapToDto(rule));
        }
        catch (InvalidPricingRuleException ex)
        {
            _logger.LogWarning(ex, "Invalid pricing rule creation request");
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pricing rule creation request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pricing rule");
            return StatusCode(500, "An error occurred while creating the pricing rule");
        }
    }

    /// <summary>
    /// Updates an existing pricing rule
    /// </summary>
    /// <param name="id">Rule identifier</param>
    /// <param name="request">Update pricing rule request</param>
    /// <returns>Updated pricing rule</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<PricingRuleDto>> UpdateRule(Guid id, [FromBody] UpdatePricingRuleRequestDto request)
    {
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            var rule = await _pricingRuleRepository.GetByIdAsync(id);

            if (!string.IsNullOrWhiteSpace(request.Name) || !string.IsNullOrWhiteSpace(request.Description))
            {
                rule.UpdateDetails(
                    request.Name ?? rule.Name,
                    request.Description ?? rule.Description);
            }

            if (request.ValidFrom.HasValue && request.ValidTo.HasValue)
            {
                rule.UpdateValidityPeriod(request.ValidFrom.Value, request.ValidTo.Value);
            }

            if (request.StrategyType.HasValue && request.Value.HasValue)
            {
                var strategy = new PricingStrategy(request.StrategyType.Value, request.Value.Value, request.Parameters);
                rule.UpdateStrategy(strategy);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(MapToDto(rule));
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Pricing rule with ID {id} not found");
        }
        catch (InvalidPricingRuleException ex)
        {
            _logger.LogWarning(ex, "Invalid pricing rule update request for rule {RuleId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pricing rule {RuleId}", id);
            return StatusCode(500, "An error occurred while updating the pricing rule");
        }
    }

    /// <summary>
    /// Deletes a pricing rule
    /// </summary>
    /// <param name="id">Rule identifier</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRule(Guid id)
    {
        try
        {
            await _pricingRuleRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pricing rule {RuleId}", id);
            return StatusCode(500, "An error occurred while deleting the pricing rule");
        }
    }

    /// <summary>
    /// Gets active pricing rules
    /// </summary>
    /// <param name="validAt">Optional date to check validity (defaults to now)</param>
    /// <returns>Collection of active pricing rules</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetActiveRules([FromQuery] DateTime? validAt = null)
    {
        try
        {
            var checkDate = validAt ?? DateTime.UtcNow;
            var rules = await _pricingRuleRepository.GetActiveRulesAsync(checkDate);
            var ruleDtos = rules.Select(MapToDto);
            return Ok(ruleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active pricing rules");
            return StatusCode(500, "An error occurred while retrieving active pricing rules");
        }
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

/// <summary>
/// DTO for creating pricing rules
/// </summary>
public class CreatePricingRuleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PricingStrategyType StrategyType { get; set; }
    public decimal Value { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int Priority { get; set; }
    public List<string>? ApplicableProductCategories { get; set; }
    public List<Guid>? ApplicableProductIds { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? CustomerType { get; set; }
}

/// <summary>
/// DTO for updating pricing rules
/// </summary>
public class UpdatePricingRuleRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PricingStrategyType? StrategyType { get; set; }
    public decimal? Value { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}