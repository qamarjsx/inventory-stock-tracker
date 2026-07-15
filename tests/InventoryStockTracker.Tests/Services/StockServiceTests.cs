using InventoryStockTracker.Data;
using InventoryStockTracker.Entities;
using InventoryStockTracker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryStockTracker.Tests.Services;

public class StockServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly StockService _sut;

    public StockServiceTests()
    {
        // A ":memory:" SQLite database only exists while its connection stays
        // open, so we open it once here and hand that same connection to the
        // DbContext. This gives real SQLite transaction behavior (unlike the
        // EF Core InMemory provider, which throws on BeginTransactionAsync),
        // matching what RecordMovementAsync actually relies on in production.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _sut = new StockService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetCurrentStockAsync_ComputesSumOfInMinusOutMovements()
    {
        // Arrange
        var product = new Product("SKU-001", "Widget", null, reorderLevel: 5);
        _context.Products.Add(product);
        _context.StockMovements.Add(new StockMovement(product.Id, StockMovementType.In, 10, null));
        _context.StockMovements.Add(new StockMovement(product.Id, StockMovementType.In, 5, null));
        _context.StockMovements.Add(new StockMovement(product.Id, StockMovementType.Out, 3, null));
        await _context.SaveChangesAsync();

        // Act
        var currentStock = await _sut.GetCurrentStockAsync(product.Id);

        // Assert
        Assert.Equal(12, currentStock); // 10 + 5 - 3
    }

    [Fact]
    public async Task RecordMovementAsync_RejectsOutMovementThatWouldDropStockBelowZero()
    {
        // Arrange - product starts with exactly 5 units in stock
        var product = new Product("SKU-002", "Gadget", null, reorderLevel: 2);
        _context.Products.Add(product);
        _context.StockMovements.Add(new StockMovement(product.Id, StockMovementType.In, 5, null));
        await _context.SaveChangesAsync();

        // Act - attempt to ship more than is available
        var result = await _sut.RecordMovementAsync(
            product.Id, StockMovementType.Out, quantity: 8, note: "Attempted overshipment");

        // Assert - rejected, with the actual available quantity surfaced back
        Assert.False(result.Success);
        Assert.Equal(5, result.AvailableStock);
        Assert.NotNull(result.ErrorMessage);

        // Assert - the rejected movement was never persisted
        var movementCount = await _context.StockMovements.CountAsync(m => m.ProductId == product.Id);
        Assert.Equal(1, movementCount); // just the original seed "In" movement

        var stockAfterRejection = await _sut.GetCurrentStockAsync(product.Id);
        Assert.Equal(5, stockAfterRejection); // unchanged
    }
}