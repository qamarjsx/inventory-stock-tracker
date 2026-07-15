using System.ComponentModel.DataAnnotations;
using InventoryStockTracker.Entities;

namespace InventoryStockTracker.ViewModels;

public class RecordMovementInputViewModel
{
    [Required(ErrorMessage = "Select whether this is stock In or Out.")]
    public StockMovementType Type { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
    public int Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }
}