using InventoryStockTracker.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryStockTracker.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // Sku: required, bounded length, and — critically — the unique
        // constraint enforcing "SKU must be unique" at the database level,
        // not just in application code. A unique index is the real source
        // of truth for uniqueness; app-level checks alone are vulnerable to
        // the same race-condition category we discussed for stock movements.
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.ReorderLevel)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedUtc)
            .IsRequired();

        // Tells EF Core to read/write the private _movements field directly,
        // instead of expecting a public settable collection property.
        builder.Metadata
            .FindNavigation(nameof(Product.Movements))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Movements)
            .WithOne()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}