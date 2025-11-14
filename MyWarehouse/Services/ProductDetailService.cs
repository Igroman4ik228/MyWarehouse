using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;

namespace MyWarehouse.Services
{
    public interface IProductDetailService
    {
        Task<Product?> GetProductWithDetailsAsync(int productId);
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
    }
}