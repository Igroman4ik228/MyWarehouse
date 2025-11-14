using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;

namespace Tests;

public class ProductDetailServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public ProductDetailServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetProductWithDetailsAsync_WithExistingProduct_ReturnsProductWithDetails()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var category = new Category { IdCategory = 1, Name = "Electronics" };
        var unit = new Unit { IdUnit = 1, Name = "Piece" };
        var location = new Location { IdLocation = 1, Zone = "Warehouse A" };

        var product = new Product
        {
            IdProduct = 1,
            Name = "Laptop",
            Category = category,
            Unit = unit
        };

        var stock = new Stock
        {
            ProductId = 1,
            LocationId = 1,
            Location = location,
            ProductQuantity = 10
        };

        context.CURS_Categories.Add(category);
        context.CURS_Units.Add(unit);
        context.CURS_Locations.Add(location);
        context.CURS_Products.Add(product);
        context.CURS_Stocks.Add(stock);
        await context.SaveChangesAsync();

        var service = new ProductDetailService(context);

        // Act
        var result = await service.GetProductWithDetailsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.IdProduct.Should().Be(1);
        result.Category.Should().NotBeNull();
        result.Unit.Should().NotBeNull();
        result.Stocks.Should().HaveCount(1);
        result.Stocks.First().Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductWithDetailsAsync_WithNonExistingProduct_ReturnsNull()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);
        var service = new ProductDetailService(context);

        // Act
        var result = await service.GetProductWithDetailsAsync(999);

        // Assert
        result.Should().BeNull();
    }
}