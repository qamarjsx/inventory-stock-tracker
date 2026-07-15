namespace InventoryStockTracker.ViewModels;

public class ProductDetailViewModel
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public int ReorderLevel { get; init; }
    public bool IsActive { get; init; }
    public int CurrentStock { get; init; }
    public bool IsLowStock => CurrentStock <= ReorderLevel;

    public List<StockMovementViewModel> Movements { get; init; } = new();

    // Nested, mutable, and the only part that binds from POST -
    // everything else on this class is display-only.
    public RecordMovementInputViewModel MovementForm { get; set; } = new();
}