namespace InventoryStockTracker.Entities;

public class Product
{
    private readonly List<StockMovement> _movements = new();

    public Guid Id { get; private set; }
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public int ReorderLevel { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    // Exposed as read-only to callers; EF Core can still populate the backing
    // field directly. External code can read history but cannot replace or
    // directly mutate the collection (e.g., product.Movements.Clear() won't compile).
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    private Product() { } // EF Core materialization only

    public Product(string sku, string name, string? description, int reorderLevel)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (reorderLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(reorderLevel), "Reorder level cannot be negative.");

        Id = Guid.CreateVersion7();
        Sku = sku.Trim();
        Name = name.Trim();
        Description = description;
        ReorderLevel = reorderLevel;
        IsActive = true;
        CreatedUtc = DateTime.UtcNow;
    }

    // Domain method, not a public setter — this is where our locked-SKU
    // business rule (Assumption #6) actually gets enforced, not just in the UI.
    public void UpdateDetails(string name, string? description, int reorderLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (reorderLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(reorderLevel), "Reorder level cannot be negative.");

        Name = name.Trim();
        Description = description;
        ReorderLevel = reorderLevel;
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}