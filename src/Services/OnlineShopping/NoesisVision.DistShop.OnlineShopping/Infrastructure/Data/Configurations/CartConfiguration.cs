using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoesisVision.DistShop.OnlineShopping.Domain.Aggregates;
using NoesisVision.DistShop.OnlineShopping.Domain.ValueObjects;
using System.Text.Json;

namespace NoesisVision.DistShop.OnlineShopping.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for CartAggregate
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<CartAggregate>
{
    public void Configure(EntityTypeBuilder<CartAggregate> builder)
    {
        builder.ToTable("Carts");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedNever();
            
        builder.Property(c => c.CustomerId)
            .IsRequired();
            
        builder.Property(c => c.Currency)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.Property(c => c.CreatedAt)
            .IsRequired();
            
        builder.Property(c => c.UpdatedAt)
            .IsRequired();
            
        builder.Property(c => c.LastActivityAt);

        // Configure CartItems as owned entities (value objects)
        builder.OwnsMany(c => c.Items, itemBuilder =>
        {
            itemBuilder.ToTable("CartItems");
            
            itemBuilder.WithOwner().HasForeignKey("CartId");
            
            itemBuilder.Property<int>("Id")
                .ValueGeneratedOnAdd();
            itemBuilder.HasKey("Id");
            
            itemBuilder.Property(ci => ci.ProductId)
                .IsRequired();
                
            itemBuilder.Property(ci => ci.ProductName)
                .IsRequired()
                .HasMaxLength(500);
                
            itemBuilder.Property(ci => ci.Quantity)
                .IsRequired();
                
            itemBuilder.Property(ci => ci.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            itemBuilder.Property(ci => ci.Currency)
                .IsRequired()
                .HasMaxLength(3);
                
            itemBuilder.Property(ci => ci.AddedAt)
                .IsRequired();

            // Index for better query performance
            itemBuilder.HasIndex(ci => ci.ProductId);
        });

        // Indexes for better query performance
        builder.HasIndex(c => c.CustomerId)
            .IsUnique();
            
        builder.HasIndex(c => c.LastActivityAt);
        
        // Ignore domain events (they are not persisted)
        builder.Ignore(c => c.DomainEvents);
    }
}