using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoesisVision.DistShop.Catalog.Domain.Aggregates;

namespace NoesisVision.DistShop.Catalog.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for CategoryAggregate
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<CategoryAggregate>
{
    public void Configure(EntityTypeBuilder<CategoryAggregate> builder)
    {
        builder.ToTable("Categories");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property("_name")
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property("_description")
            .HasColumnName("Description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property("_parentCategoryId")
            .HasColumnName("ParentCategoryId")
            .IsRequired(false);

        builder.Property("_isActive")
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property("_createdAt")
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property("_updatedAt")
            .HasColumnName("UpdatedAt")
            .IsRequired();

        // For simplicity, we'll ignore child category IDs in the database for now
        builder.Ignore("_childCategoryIds");

        // Self-referencing relationship for parent-child hierarchy
        builder.HasOne<CategoryAggregate>()
            .WithMany()
            .HasForeignKey("_parentCategoryId")
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex("_parentCategoryId")
            .HasDatabaseName("IX_Categories_ParentCategoryId");

        builder.HasIndex("_isActive")
            .HasDatabaseName("IX_Categories_IsActive");

        builder.HasIndex(new[] { "_name", "_parentCategoryId" })
            .HasDatabaseName("IX_Categories_Name_ParentCategoryId")
            .IsUnique();

        // Ignore domain events (they are not persisted)
        builder.Ignore(c => c.DomainEvents);


    }
}