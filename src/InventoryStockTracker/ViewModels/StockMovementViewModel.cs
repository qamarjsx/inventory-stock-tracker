using InventoryStockTracker.Entities;

namespace InventoryStockTracker.ViewModels;

public class StockMovementViewModel
{
    public StockMovementType Type { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedUtc { get; init; }
}