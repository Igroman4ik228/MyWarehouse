using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;

namespace Tests;

public class ProductServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public ProductServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetProductsWithDetailsAsync_ReturnsAllProductsWithDetails()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var category = new Category { IdCategory = 1, Name = "Electronics" };
        var unit = new Unit { IdUnit = 1, Name = "Piece" };

        var products = new List<Product>
            {
                new Product { IdProduct = 1, Name = "Laptop", Category = category, Unit = unit, SKU = "SKU1", Weight = 2.0m },
                new Product { IdProduct = 2, Name = "Mouse", Category = category, Unit = unit, SKU = "SKU2", Weight = 0.2m }
            };

        context.CURS_Categories.Add(category);
        context.CURS_Units.Add(unit);
        context.CURS_Products.AddRange(products);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.GetProductsWithDetailsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.Category != null).Should().BeTrue();
        result.All(p => p.Unit != null).Should().BeTrue();
    }

    [Theory]
    [InlineData("Laptop", 1)]
    [InlineData("SKU2", 1)]
    [InlineData("Electronics", 2)]
    [InlineData("Piece", 2)]
    [InlineData("NonExisting", 0)]
    public async Task SearchProductsAsync_WithSearchText_ReturnsFilteredProducts(string searchText, int expectedCount)
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var category = new Category { IdCategory = 1, Name = "Electronics" };
        var unit = new Unit { IdUnit = 1, Name = "Piece" };

        var products = new List<Product>
            {
                new Product { IdProduct = 1, Name = "Laptop", Category = category, Unit = unit, SKU = "SKU1", Weight = 2.0m },
                new Product { IdProduct = 2, Name = "Mouse", Category = category, Unit = unit, SKU = "SKU2", Weight = 0.2m }
            };

        var stocks = new List<Stock>
            {
                new Stock { ProductId = 1, LocationId = 1, ProductQuantity = 5 },
                new Stock { ProductId = 2, LocationId = 1, ProductQuantity = 10 }
            };

        context.CURS_Categories.Add(category);
        context.CURS_Units.Add(unit);
        context.CURS_Products.AddRange(products);
        context.CURS_Stocks.AddRange(stocks);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.SearchProductsAsync(searchText);

        // Assert
        result.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task SearchProductsAsync_WithEmptySearchText_ReturnsAllProducts()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var category = new Category { IdCategory = 1, Name = "Electronics" };
        var unit = new Unit { IdUnit = 1, Name = "Piece" };
        var products = new List<Product>
            {
                new Product { IdProduct = 1, Name = "Laptop", Category = category, Unit = unit },
                new Product { IdProduct = 2, Name = "Mouse", Category = category, Unit = unit }
            };

        context.CURS_Categories.Add(category);
        context.CURS_Units.Add(unit);
        context.CURS_Products.AddRange(products);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.SearchProductsAsync("");

        // Assert
        result.Should().HaveCount(2);
    }
}