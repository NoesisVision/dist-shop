using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;

namespace NoesisVision.DistShop.Catalog.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ProductAggregate
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<ProductAggregate>
{
    public void Configure(EntityTypeBuilder<ProductAggregate> builder)
    {
        builder.ToTable("Products");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property("_name")
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property("_description")
            .HasColumnName("Description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property("_sku")
            .HasColumnName("Sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property("_price")
            .HasColumnName("Price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property("_currency")
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("USD");

        builder.Property("_categoryId")
            .HasColumnName("CategoryId")
            .IsRequired();

        builder.Property("_isActive")
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property("_createdAt")
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property("_updatedAt")
            .HasColumnName("UpdatedAt")
            .IsRequired();

        // Indexes
        builder.HasIndex("_sku")
            .HasDatabaseName("IX_Products_Sku")
            .IsUnique();

        builder.HasIndex("_categoryId")
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex("_isActive")
            .HasDatabaseName("IX_Products_IsActive");

        // Ignore domain events (they are not persisted)
        builder.Ignore(p => p.DomainEvents);


    }
}