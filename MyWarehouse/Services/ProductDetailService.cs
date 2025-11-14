using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;

namespace MyWarehouse.Services
{
    public interface IProductDetailService
    {
        Task<Product?> GetProductWithDetailsAsync(int productId);
        Task<ICollection<DeliveryTask>> GetProductMovementHistoryAsync(int productId);
    }

    public class ProductDetailService : IProductDetailService
    {
        private readonly AppDbContext _context;

        public ProductDetailService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetProductWithDetailsAsync(int productId)
        {
            return await _context.CURS_Products
                .Where(p => p.IdProduct == productId)
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Location)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<DeliveryTask>> GetProductMovementHistoryAsync(int productId)
        {
            return await _context.CURS_DeliveryTasks
                .Where(dt => dt.ProductId == productId)
                .Include(dt => dt.DeliveryType)
                .Include(dt => dt.TaskStatus)
                .Include(dt => dt.FromLocation)
                .Include(dt => dt.ToLocation)
                .Include(dt => dt.Client)
                .OrderByDescending(dt => dt.CreatedAt)
                .Where(dt => dt.TaskStatusId == (int)DeliveryTaskStatus.Completed)
                .ToListAsync();
        }
    }
}
