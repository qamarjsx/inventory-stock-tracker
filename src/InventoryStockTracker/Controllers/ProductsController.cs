using InventoryStockTracker.Services;
using InventoryStockTracker.ViewModels;
using InventoryStockTracker.Entities;

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

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var viewModel = await BuildDetailViewModelAsync(product);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordMovement(Guid id, RecordMovementInputViewModel movementForm)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await BuildDetailViewModelAsync(product);
            viewModel.MovementForm = movementForm; // preserve what the user typed
            return View("Details", viewModel);
        }

        var result = await _stockService.RecordMovementAsync(
            id, movementForm.Type, movementForm.Quantity, movementForm.Note);

        if (!result.Success)
        {
            ModelState.AddModelError(
            $"{nameof(ProductDetailViewModel.MovementForm)}.{nameof(RecordMovementInputViewModel.Quantity)}",
            result.ErrorMessage!);
            var viewModel = await BuildDetailViewModelAsync(product);
            viewModel.MovementForm = movementForm;
            return View("Details", viewModel);
        }

        TempData["SuccessMessage"] = "Movement recorded successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<ProductDetailViewModel> BuildDetailViewModelAsync(Product product)
    {
        var currentStock = await _stockService.GetCurrentStockAsync(product.Id);

        return new ProductDetailViewModel
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive,
            CurrentStock = currentStock,
            Movements = product.Movements
                .OrderByDescending(m => m.CreatedUtc)
                .Select(m => new StockMovementViewModel
                {
                    Type = m.Type,
                    Quantity = m.Quantity,
                    Note = m.Note,
                    CreatedUtc = m.CreatedUtc
                })
                .ToList()
        };
    }

    public IActionResult Create()
    {
        return View("Edit", new ProductFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        var result = await _productService.CreateAsync(form.Sku, form.Name, form.Description, form.ReorderLevel);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.Sku), result.ErrorMessage!);
            return View("Edit", form);
        }

        return RedirectToAction(nameof(Details), new { id = result.Product!.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var form = new ProductFormViewModel
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            ReorderLevel = product.ReorderLevel
        };

        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var updated = await _productService.UpdateAsync(id, form.Name, form.Description, form.ReorderLevel);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var deactivated = await _productService.DeactivateAsync(id);
        if (!deactivated) return NotFound();

        TempData["SuccessMessage"] = "Product deactivated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        var reactivated = await _productService.ReactivateAsync(id);
        if (!reactivated) return NotFound();

        TempData["SuccessMessage"] = "Product reactivated.";
        return RedirectToAction(nameof(Details), new { id });
    }
}