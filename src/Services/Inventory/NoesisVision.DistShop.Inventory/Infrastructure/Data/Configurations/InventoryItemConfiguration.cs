using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoesisVision.DistShop.Inventory.Domain.Aggregates;
using NoesisVision.DistShop.Inventory.Domain.ValueObjects;

namespace NoesisVision.DistShop.Inventory.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for InventoryItem aggregate
/// </summary>
public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        // Table configuration
        builder.ToTable("InventoryItems");
        
        // Primary key
        builder.HasKey(x => x.Id);
        
        // Properties
        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.AvailableQuantity)
            .IsRequired();

        builder.Property(x => x.ReorderLevel)
            .IsRequired();

        builder.Property(x => x.MaxStockLevel)
            .IsRequired();

        builder.Property(x => x.LastUpdated)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.ProductId)
            .IsUnique()
            .HasDatabaseName("IX_InventoryItems_ProductId");

        builder.HasIndex(x => x.LastUpdated)
            .HasDatabaseName("IX_InventoryItems_LastUpdated");

        // Configure owned collection for reservations
        builder.OwnsMany(x => x.Reservations, reservationBuilder =>
        {
            reservationBuilder.ToTable("StockReservations");
            
            reservationBuilder.WithOwner()
                .HasForeignKey("InventoryItemId");

            reservationBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();

            reservationBuilder.HasKey("Id");

            reservationBuilder.Property(r => r.ReservationId)
                .IsRequired();

            reservationBuilder.Property(r => r.Quantity)
                .IsRequired();

            reservationBuilder.Property(r => r.CreatedAt)
                .IsRequired();

            reservationBuilder.Property(r => r.ExpiresAt)
                .IsRequired();

            reservationBuilder.Property(r => r.Reference)
                .HasMaxLength(500);

            reservationBuilder.HasIndex(r => r.ReservationId)
                .IsUnique()
                .HasDatabaseName("IX_StockReservations_ReservationId");

            reservationBuilder.HasIndex(r => r.ExpiresAt)
                .HasDatabaseName("IX_StockReservations_ExpiresAt");
        });

        // Ignore domain events (they're handled separately)
        builder.Ignore(x => x.DomainEvents);
        
        // Computed properties
        builder.Ignore(x => x.ReservedQuantity);
        builder.Ignore(x => x.TotalQuantity);
    }
}