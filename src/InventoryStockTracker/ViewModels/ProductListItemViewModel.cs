namespace InventoryStockTracker.ViewModels;

public class ProductListItemViewModel
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int CurrentStock { get; init; }
    public int ReorderLevel { get; init; }
    public bool IsActive { get; init; }
    public bool IsLowStock => CurrentStock <= ReorderLevel;
}

public class ProductListViewModel
{
    public List<ProductListItemViewModel> Products { get; init; } = new();
    public string? SearchTerm { get; init; }
}