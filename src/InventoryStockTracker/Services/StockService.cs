using InventoryStockTracker.Data;
using InventoryStockTracker.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryStockTracker.Services;

public class StockService : IStockService
{
    private readonly AppDbContext _context;

    public StockService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetCurrentStockAsync(Guid productId)
    {
        return await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .SumAsync(m => m.Type == StockMovementType.In ? m.Quantity : -m.Quantity);
    }

    public async Task<Dictionary<Guid, int>> GetCurrentStockForProductsAsync(IEnumerable<Guid> productIds)
    {
        var idList = productIds.ToList();

        return await _context.StockMovements
            .Where(m => idList.Contains(m.ProductId))
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Stock = g.Sum(m => m.Type == StockMovementType.In ? m.Quantity : -m.Quantity)
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.Stock);
    }

    public async Task<StockMovementResult> RecordMovementAsync(
        Guid productId, StockMovementType type, int quantity, string? note)
    {
        // Concurrency-safe path lives here - implemented next, once we've
        // walked through the transaction/isolation approach in detail.
        throw new NotImplementedException();
    }
}