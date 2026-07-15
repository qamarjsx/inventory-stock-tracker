using InventoryStockTracker.Entities;

namespace InventoryStockTracker.Services;

public interface IStockService
{
    Task<int> GetCurrentStockAsync(Guid productId);
    Task<Dictionary<Guid, int>> GetCurrentStockForProductsAsync(IEnumerable<Guid> productIds);
    Task<StockMovementResult> RecordMovementAsync(Guid productId, StockMovementType type, int quantity, string? note);
}

public record StockMovementResult(bool Success, string? ErrorMessage, int? AvailableStock = null);