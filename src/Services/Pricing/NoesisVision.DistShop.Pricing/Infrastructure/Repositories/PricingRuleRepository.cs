using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.Pricing.Domain.Aggregates;
using NoesisVision.DistShop.Pricing.Domain.Repositories;
using NoesisVision.DistShop.Pricing.Infrastructure.Data;

namespace NoesisVision.DistShop.Pricing.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of pricing rule repository
/// </summary>
public class PricingRuleRepository : IPricingRuleRepository
{
    private readonly PricingDbContext _context;

    public PricingRuleRepository(PricingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PricingRuleAggregate?> GetByIdAsync(Guid id)
    {
        return await _context.PricingRules.FindAsync(id);
    }

    public async Task<IEnumerable<PricingRuleAggregate>> GetAllAsync()
    {
        return await _context.PricingRules.ToListAsync();
    }

    public async Task AddAsync(PricingRuleAggregate entity)
    {
        await _context.PricingRules.AddAsync(entity);
    }

    public Task UpdateAsync(PricingRuleAggregate entity)
    {
        _context.PricingRules.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var rule = await _context.PricingRules.FindAsync(id);
        if (rule != null)
        {
            _context.PricingRules.Remove(rule);
        }
    }

    public async Task<IEnumerable<PricingRuleAggregate>> GetActiveRulesAsync(DateTime validAt)
    {
        return await _context.PricingRules
            .Where(r => r.IsActive && r.ValidFrom <= validAt && r.ValidTo >= validAt)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<PricingRuleAggregate>> GetApplicableRulesForProductAsync(
        Guid productId, 
        string? categoryName = null, 
        DateTime? validAt = null)
    {
        var checkDate = validAt ?? DateTime.UtcNow;
        
        var query = _context.PricingRules
            .Where(r => r.IsActive && r.ValidFrom <= checkDate && r.ValidTo >= checkDate);

        // Filter by product applicability
        query = query.Where(r => 
            // Rule applies to all products (no specific products or categories)
            (!r.ApplicableProductIds.Any() && !r.ApplicableProductCategories.Any()) ||
            // Rule applies to specific product
            r.ApplicableProductIds.Contains(productId) ||
            // Rule applies to product category
            (categoryName != null && r.ApplicableProductCategories.Contains(categoryName)));

        return await query
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<PricingRuleAggregate>> GetRulesByCustomerTypeAsync(
        string customerType, 
        DateTime? validAt = null)
    {
        var checkDate = validAt ?? DateTime.UtcNow;
        
        return await _context.PricingRules
            .Where(r => r.IsActive && 
                       r.ValidFrom <= checkDate && 
                       r.ValidTo >= checkDate &&
                       (r.CustomerType == null || r.CustomerType == customerType))
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<PricingRuleAggregate>> GetRulesByNameAsync(string name)
    {
        return await _context.PricingRules
            .Where(r => r.Name.Contains(name))
            .OrderBy(r => r.Name)
            .ToListAsync();
    }
}