namespace InventoryStockTracker.Entities;

public enum StockMovementType
{
    In = 1,
    Out = 2
    // Future-proofing: we can easily add 'Adjustment', 'Return', or 'Transfer' later
}