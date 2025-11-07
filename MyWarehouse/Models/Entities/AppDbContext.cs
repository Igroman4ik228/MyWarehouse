using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MyWarehouse.Models.Entities;
public class AppDbContext : DbContext
{
    public DbSet<Stock> CURS_Stocks { get; set; }
    public DbSet<Product> CURS_Products { get; set; }
    public DbSet<Category> CURS_Categories { get; set; }
    public DbSet<Unit> CURS_Units { get; set; }
    public DbSet<Location> CURS_Locations { get; set; }
    public DbSet<Client> CURS_Clients { get; set; }
    public DbSet<DeliveryTask> CURS_DeliveryTasks { get; set; }
    public DbSet<User> CURS_Users { get; set; }
    public DbSet<Role> CURS_Roles { get; set; }
    public DbSet<DeliveryType> CURS_DeliveryTypes { get; set; }
    public DbSet<TaskStatus> CURS_TaskStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.dev.json", optional: false)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product relationships
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Unit)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Stock relationships
        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Product)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Location)
            .WithMany(l => l.Stocks)
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // DeliveryTask relationships
        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.Product)
            .WithMany(p => p.DeliveryTasks)
            .HasForeignKey(dt => dt.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.Client)
            .WithMany(c => c.DeliveryTasks)
            .HasForeignKey(dt => dt.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.CreatedUser)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(dt => dt.CreatedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.ExecutorUser)
            .WithMany(u => u.ExecutedTasks)
            .HasForeignKey(dt => dt.ExecutorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.TaskStatus)
            .WithMany(ts => ts.DeliveryTasks)
            .HasForeignKey(dt => dt.TaskStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.DeliveryType)
            .WithMany(dt => dt.DeliveryTasks)
            .HasForeignKey(dt => dt.DeliveryTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.FromLocation)
            .WithMany(l => l.FromDeliveryTasks)
            .HasForeignKey(dt => dt.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryTask>()
            .HasOne(dt => dt.ToLocation)
            .WithMany(l => l.ToDeliveryTasks)
            .HasForeignKey(dt => dt.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // User relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Добавляем индексы для улучшения производительности
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.CategoryId);

        modelBuilder.Entity<Stock>()
            .HasIndex(s => s.ProductId);

        modelBuilder.Entity<Stock>()
            .HasIndex(s => s.LocationId);

        modelBuilder.Entity<DeliveryTask>()
            .HasIndex(dt => dt.CreatedAt);

        modelBuilder.Entity<DeliveryTask>()
            .HasIndex(dt => dt.TaskStatusId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        // Настройка значений по умолчанию
        modelBuilder.Entity<DeliveryTask>()
           .Property(dt => dt.CreatedAt)
           .HasDefaultValueSql("GETUTCDATE()"); // Для SQL Server
    }
}