using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoesisVision.DistShop.Orders.Domain.Aggregates;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;
using System.Text.Json;

namespace NoesisVision.DistShop.Orders.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for OrderAggregate
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        builder.ToTable("Orders");

        // Primary key
        builder.HasKey(o => o.Id);

        // Properties
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property("_customerId")
            .HasColumnName("CustomerId")
            .IsRequired();

        builder.Property("_status")
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property("_totalAmount")
            .HasColumnName("TotalAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property("_currency")
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("USD");

        builder.Property("_createdAt")
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property("_updatedAt")
            .HasColumnName("UpdatedAt")
            .IsRequired();

        builder.Property("_cancellationReason")
            .HasColumnName("CancellationReason")
            .HasMaxLength(500);

        // Configure OrderItems as owned entities
        builder.OwnsMany(typeof(OrderItem), "_items", itemBuilder =>
        {
            itemBuilder.ToTable("OrderItems", "orders");
            itemBuilder.WithOwner().HasForeignKey("OrderId");
            itemBuilder.Property<int>("Id");
            itemBuilder.HasKey("OrderId", "Id");
            
            itemBuilder.Property("ProductId")
                .IsRequired();
                
            itemBuilder.Property("ProductName")
                .HasMaxLength(200)
                .IsRequired();
                
            itemBuilder.Property("ProductSku")
                .HasMaxLength(50)
                .IsRequired();
                
            itemBuilder.Property("Quantity")
                .IsRequired();
                
            itemBuilder.Property("UnitPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            itemBuilder.Property("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex("_customerId")
            .HasDatabaseName("IX_Orders_CustomerId");

        builder.HasIndex("_status")
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex("_createdAt")
            .HasDatabaseName("IX_Orders_CreatedAt");

        builder.HasIndex(new[] { "_customerId", "_status" })
            .HasDatabaseName("IX_Orders_CustomerId_Status");

        // Ignore domain events (they are not persisted)
        builder.Ignore(o => o.DomainEvents);
    }
}