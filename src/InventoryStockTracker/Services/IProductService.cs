using InventoryStockTracker.Entities;

namespace InventoryStockTracker.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync(string? searchTerm = null);
    Task<Product?> GetByIdAsync(Guid id);
    Task<ProductCreateResult> CreateAsync(string sku, string name, string? description, int reorderLevel);
    Task<bool> UpdateAsync(Guid id, string name, string? description, int reorderLevel);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> ReactivateAsync(Guid id);
}

public record ProductCreateResult(bool Success, string? ErrorMessage, Product? Product = null);