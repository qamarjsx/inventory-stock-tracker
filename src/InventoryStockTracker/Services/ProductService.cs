using InventoryStockTracker.Data;
using InventoryStockTracker.Entities;

using Microsoft.EntityFrameworkCore;

namespace InventoryStockTracker.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync(string? searchTerm = null)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Sku, $"%{term}%") ||
                EF.Functions.Like(p.Name, $"%{term}%"));
        }

        return await query.OrderBy(p => p.Sku).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Movements)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProductCreateResult> CreateAsync(
        string sku, string name, string? description, int reorderLevel)
    {
        var normalizedSku = sku.Trim();

        var exists = await _context.Products
            .AnyAsync(p => p.Sku.ToLower() == normalizedSku.ToLower());

        if (exists)
        {
            return new ProductCreateResult(false, $"SKU '{normalizedSku}' is already in use.");
        }

        var product = new Product(normalizedSku, name, description, reorderLevel);
        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Defense in depth: the AnyAsync check above closes almost every
            // window, but two requests submitting the same new SKU at nearly
            // the same instant could both pass that check before either
            // commits. The unique index we configured on Sku is the real,
            // race-condition-proof guarantee - this catch translates its
            // failure into the same friendly message, rather than leaking
            // a raw SQLite/EF exception to the user.
            return new ProductCreateResult(false, $"SKU '{normalizedSku}' is already in use.");
        }

        return new ProductCreateResult(true, null, product);
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string? description, int reorderLevel)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        product.UpdateDetails(name, description, reorderLevel);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        product.Deactivate();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        product.Reactivate();
        await _context.SaveChangesAsync();
        return true;
    }
}