using InventoryStockTracker.Entities;

namespace InventoryStockTracker.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (context.Products.Any())
        {
            return; // Already seeded - don't duplicate on every restart.
        }

        var widget = new Product("WID-001", "Standard Widget", "General-purpose widget", reorderLevel: 10);
        var gadget = new Product("GAD-002", "Premium Gadget", "High-margin gadget", reorderLevel: 5);
        var bolt = new Product("BLT-003", "Hex Bolt (M6)", null, reorderLevel: 50);
        var sensor = new Product("SEN-004", "Temperature Sensor", "Used in cold storage monitoring", reorderLevel: 15);

        context.Products.AddRange(widget, gadget, bolt, sensor);

        // Movements recorded via the domain constructor - never inserted as
        // raw data, so the same validation rules apply here as anywhere else.
        context.StockMovements.AddRange(
            new StockMovement(widget.Id, StockMovementType.In, 100, "Initial stock receipt"),
            new StockMovement(widget.Id, StockMovementType.Out, 85, "Bulk order fulfilled"),
            // widget: 100 - 85 = 15 (above reorder level of 10)

            new StockMovement(gadget.Id, StockMovementType.In, 20, "Initial stock receipt"),
            new StockMovement(gadget.Id, StockMovementType.Out, 17, "Customer orders"),
            // gadget: 20 - 17 = 3 (BELOW reorder level of 5 - will show "Low stock")

            new StockMovement(bolt.Id, StockMovementType.In, 500, "Bulk supplier delivery"),
            // bolt: 500 - 0 = 500 (well above reorder level)

            new StockMovement(sensor.Id, StockMovementType.In, 30, "Initial stock receipt"),
            new StockMovement(sensor.Id, StockMovementType.Out, 30, "Shipped to cold storage facility")
            // sensor: 30 - 30 = 0 (AT reorder level of 15 - will show "Low stock")
        );

        context.SaveChanges();
    }
}