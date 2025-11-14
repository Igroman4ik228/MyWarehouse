using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;

namespace Tests;

public class AuthServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public AuthServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsUser()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Login = "testuser", Password = hashedPassword };
        context.CURS_Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context);

        // Act
        var result = await authService.AuthenticateAsync("testuser", "password123");

        // Assert
        result.Should().NotBeNull();
        result.Login.Should().Be("testuser");
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("user", null)]
    [InlineData("user", "")]
    public async Task AuthenticateAsync_WithInvalidInput_ReturnsNull(string login, string password)
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);
        var authService = new AuthService(context);

        // Act
        var result = await authService.AuthenticateAsync(login, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var authService = new AuthService(Mock.Of<AppDbContext>());

        // Act
        var result = authService.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }
}
