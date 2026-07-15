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
        // Defensive validation. The ViewModel will also enforce this via data
        // annotations before we ever reach this method — but this service has
        // to be safe to call from anywhere (a future API endpoint, a test,
        // another controller), not just from a caller that remembered to
        // validate first. Same philosophy as the guard clauses in the
        // StockMovement constructor.
        if (quantity <= 0)
        {
            return new StockMovementResult(false, "Quantity must be a positive number.");
        }

        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            return new StockMovementResult(false, "Product not found.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // The re-check: read current stock INSIDE the transaction, immediately
        // before deciding. This is the step that closes the check-then-act gap
        // we walked through earlier — reading this before opening the
        // transaction would put the read back outside the safety net.
        var currentStock = await GetCurrentStockAsync(productId);

        if (type == StockMovementType.Out && quantity > currentStock)
        {
            // Nothing was written, so there's nothing to roll back — letting
            // `transaction` fall out of scope via `await using` disposes it
            // (and implicitly rolls back) cleanly.
            return new StockMovementResult(
                false,
                $"Cannot record this movement: only {currentStock} unit(s) currently in stock.",
                currentStock);
        }

        var movement = new StockMovement(productId, type, quantity, note);
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var newStock = type == StockMovementType.In
            ? currentStock + quantity
            : currentStock - quantity;

        return new StockMovementResult(true, null, newStock);
    }
}