using InventoryStockTracker.Services;
using InventoryStockTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventoryStockTracker.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IStockService _stockService;

    public ProductsController(IProductService productService, IStockService stockService)
    {
        _productService = productService;
        _stockService = stockService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var products = await _productService.GetAllAsync(search);
        var stockByProduct = await _stockService.GetCurrentStockForProductsAsync(
            products.Select(p => p.Id));

        var viewModel = new ProductListViewModel
        {
            SearchTerm = search,
            Products = products.Select(p => new ProductListItemViewModel
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                ReorderLevel = p.ReorderLevel,
                IsActive = p.IsActive,
                CurrentStock = stockByProduct.GetValueOrDefault(p.Id, 0)
            }).ToList()
        };

        return View(viewModel);
    }
}