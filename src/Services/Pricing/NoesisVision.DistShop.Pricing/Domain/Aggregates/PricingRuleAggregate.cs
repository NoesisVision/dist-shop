using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.Pricing.Domain.ValueObjects;
using NoesisVision.DistShop.Pricing.Domain.Exceptions;
using NoesisVision.DistShop.Pricing.Contracts.Events;

namespace NoesisVision.DistShop.Pricing.Domain.Aggregates;

/// <summary>
/// Pricing rule aggregate root managing pricing rules and business logic
/// </summary>
public class PricingRuleAggregate : AggregateRoot
{
    private string _name;
    private string _description;
    private PricingStrategy _strategy;
    private DateTime _validFrom;
    private DateTime _validTo;
    private bool _isActive;
    private int _priority;
    private List<string> _applicableProductCategories;
    private List<Guid> _applicableProductIds;
    private decimal? _minimumOrderAmount;
    private string? _customerType;
    private DateTime _createdAt;
    private DateTime _updatedAt;

    // EF Core constructor
    private PricingRuleAggregate() : base()
    {
        _name = null!;
        _description = null!;
        _strategy = null!;
        _applicableProductCategories = new List<string>();
        _applicableProductIds = new List<Guid>();
    }

    private PricingRuleAggregate(
        string name,
        string description,
        PricingStrategy strategy,
        DateTime validFrom,
        DateTime validTo,
        int priority = 0) : base()
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _description = description ?? throw new ArgumentNullException(nameof(description));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _validFrom = validFrom;
        _validTo = validTo;
        _priority = priority;
        _isActive = true;
        _applicableProductCategories = new List<string>();
        _applicableProductIds = new List<Guid>();
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;

        AddDomainEvent(new PricingRuleCreatedEvent(
            Id,
            _name,
            _strategy.Type.ToString(),
            _strategy.Value,
            _validFrom,
            _validTo));
    }

    public static PricingRuleAggregate Create(
        string name,
        string description,
        PricingStrategy strategy,
        DateTime validFrom,
        DateTime validTo,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPricingRuleException("Pricing rule name cannot be null or empty");

        if (validFrom >= validTo)
            throw new InvalidPricingRuleException("Valid from date must be before valid to date");

        if (validTo <= DateTime.UtcNow)
            throw new InvalidPricingRuleException("Valid to date must be in the future");

        return new PricingRuleAggregate(name.Trim(), description?.Trim() ?? string.Empty, strategy, validFrom, validTo, priority);
    }

    // Properties
    public string Name => _name;
    public string Description => _description;
    public PricingStrategy Strategy => _strategy;
    public DateTime ValidFrom => _validFrom;
    public DateTime ValidTo => _validTo;
    public bool IsActive => _isActive;
    public int Priority => _priority;
    public IReadOnlyList<string> ApplicableProductCategories => _applicableProductCategories.AsReadOnly();
    public IReadOnlyList<Guid> ApplicableProductIds => _applicableProductIds.AsReadOnly();
    public decimal? MinimumOrderAmount => _minimumOrderAmount;
    public string? CustomerType => _customerType;
    public DateTime CreatedAt => _createdAt;
    public DateTime UpdatedAt => _updatedAt;

    // Business methods
    public void UpdateDetails(string name, string description)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update inactive pricing rule");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPricingRuleException("Pricing rule name cannot be null or empty");

        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim() ?? string.Empty;

        if (_name != trimmedName || _description != trimmedDescription)
        {
            _name = trimmedName;
            _description = trimmedDescription;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new PricingRuleUpdatedEvent(
                Id,
                _name,
                _strategy.Type.ToString(),
                _strategy.Value,
                _validFrom,
                _validTo));
        }
    }

    public void UpdateStrategy(PricingStrategy newStrategy)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update strategy of inactive pricing rule");

        if (newStrategy == null)
            throw new ArgumentNullException(nameof(newStrategy));

        if (!_strategy.Equals(newStrategy))
        {
            _strategy = newStrategy;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new PricingRuleUpdatedEvent(
                Id,
                _name,
                _strategy.Type.ToString(),
                _strategy.Value,
                _validFrom,
                _validTo));
        }
    }

    public void UpdateValidityPeriod(DateTime validFrom, DateTime validTo)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update validity period of inactive pricing rule");

        if (validFrom >= validTo)
            throw new InvalidPricingRuleException("Valid from date must be before valid to date");

        if (_validFrom != validFrom || _validTo != validTo)
        {
            _validFrom = validFrom;
            _validTo = validTo;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new PricingRuleUpdatedEvent(
                Id,
                _name,
                _strategy.Type.ToString(),
                _strategy.Value,
                _validFrom,
                _validTo));
        }
    }

    public void SetApplicableProducts(List<Guid> productIds)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update product applicability of inactive pricing rule");

        _applicableProductIds = productIds ?? new List<Guid>();
        _updatedAt = DateTime.UtcNow;
    }

    public void SetApplicableCategories(List<string> categories)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update category applicability of inactive pricing rule");

        _applicableProductCategories = categories ?? new List<string>();
        _updatedAt = DateTime.UtcNow;
    }

    public void SetMinimumOrderAmount(decimal? minimumAmount)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update minimum order amount of inactive pricing rule");

        if (minimumAmount.HasValue && minimumAmount.Value < 0)
            throw new InvalidPricingRuleException("Minimum order amount cannot be negative");

        _minimumOrderAmount = minimumAmount;
        _updatedAt = DateTime.UtcNow;
    }

    public void SetCustomerType(string? customerType)
    {
        if (!_isActive)
            throw new InvalidPricingRuleException("Cannot update customer type of inactive pricing rule");

        _customerType = customerType?.Trim();
        _updatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _updatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _updatedAt = DateTime.UtcNow;
    }

    public bool IsValidAt(DateTime dateTime)
    {
        return _isActive && dateTime >= _validFrom && dateTime <= _validTo;
    }

    public bool IsApplicableToProduct(Guid productId, string? categoryName = null)
    {
        if (!IsValidAt(DateTime.UtcNow))
            return false;

        // If no specific products or categories are defined, rule applies to all
        if (!_applicableProductIds.Any() && !_applicableProductCategories.Any())
            return true;

        // Check if product ID is specifically included
        if (_applicableProductIds.Contains(productId))
            return true;

        // Check if product category is included
        if (!string.IsNullOrEmpty(categoryName) && _applicableProductCategories.Contains(categoryName))
            return true;

        return false;
    }

    public bool IsApplicableToCustomer(string? customerType, decimal? orderAmount = null)
    {
        if (!IsValidAt(DateTime.UtcNow))
            return false;

        // Check customer type constraint
        if (!string.IsNullOrEmpty(_customerType) && _customerType != customerType)
            return false;

        // Check minimum order amount constraint
        if (_minimumOrderAmount.HasValue && (!orderAmount.HasValue || orderAmount.Value < _minimumOrderAmount.Value))
            return false;

        return true;
    }
}