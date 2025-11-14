using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;

namespace Tests;

public class TaskProcessingServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public TaskProcessingServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CancelTaskAsync_WithExistingTask_CancelsSuccessfully()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var task = new DeliveryTask
        {
            IdDeliveryTask = 1,
            TaskStatusId = (int)DeliveryTaskStatus.InProgress
        };

        context.CURS_DeliveryTasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskProcessingService(context);

        // Act
        var result = await service.CancelTaskAsync(1);

        // Assert
        result.Should().BeTrue();
        task.TaskStatusId.Should().Be((int)DeliveryTaskStatus.Rejected);
    }

    [Fact]
    public async Task CancelTaskAsync_WithNonExistingTask_ReturnsFalse()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);
        var service = new TaskProcessingService(context);

        // Act
        var result = await service.CancelTaskAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}