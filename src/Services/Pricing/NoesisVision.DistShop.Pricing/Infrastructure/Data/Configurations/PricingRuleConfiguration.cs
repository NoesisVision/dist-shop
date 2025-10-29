using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoesisVision.DistShop.Pricing.Domain.Aggregates;
using NoesisVision.DistShop.Pricing.Domain.ValueObjects;
using System.Text.Json;

namespace NoesisVision.DistShop.Pricing.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for PricingRuleAggregate
/// </summary>
public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRuleAggregate>
{
    public void Configure(EntityTypeBuilder<PricingRuleAggregate> builder)
    {
        builder.ToTable("PricingRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.ValidFrom)
            .IsRequired();

        builder.Property(x => x.ValidTo)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CustomerType)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Configure PricingStrategy as owned entity
        builder.OwnsOne(x => x.Strategy, strategy =>
        {
            strategy.Property(s => s.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            strategy.Property(s => s.Value)
                .HasPrecision(18, 4)
                .IsRequired();

            strategy.Property(s => s.Parameters)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnType("nvarchar(max)");
        });

        // Configure collections as JSON columns
        builder.Property(x => x.ApplicableProductCategories)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ApplicableProductIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => new { x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.CustomerType);

        // Ignore domain events (they are not persisted)
        builder.Ignore(x => x.DomainEvents);
    }
}