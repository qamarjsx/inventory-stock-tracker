namespace InventoryStockTracker.Entities;

public class StockMovement
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    // Private parameterless constructor: exists solely for EF Core materialization.
    // EF Core uses this (via reflection) to rehydrate rows already known to be
    // valid from the database. Application code should never call this directly.
    private StockMovement() { }

    // Public constructor: the only way application code can create a NEW movement.
    // Guard clauses here make an invalid StockMovement unrepresentable in memory.
    public StockMovement(Guid productId, StockMovementType type, int quantity, string? note)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        Id = Guid.CreateVersion7();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        Note = note;
        CreatedUtc = DateTime.UtcNow;
    }
}