using InventoryStockTracker.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryStockTracker.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Quantity)
            .IsRequired();

        builder.Property(m => m.Note)
            .HasMaxLength(500);

        builder.Property(m => m.CreatedUtc)
            .IsRequired();

        // Supports efficient "sum movements for this product" queries -
        // the core read pattern this entire app relies on for computing
        // current stock, and for loading a product's movement history.
        builder.HasIndex(m => m.ProductId);
    }
}