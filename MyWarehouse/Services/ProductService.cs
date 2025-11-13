using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;

namespace MyWarehouse.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsWithDetailsAsync();
        Task<List<Product>> SearchProductsAsync(string searchText);
    }

    public class ProductService(AppDbContext context) : IProductService
    {
        private readonly AppDbContext _context = context;

        public async Task<List<Product>> GetProductsWithDetailsAsync()
        {
            return await _context.CURS_Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
                .Include(p => p.Unit)
                .ToListAsync();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchText)
        {
            var products = await GetProductsWithDetailsAsync();

            if (string.IsNullOrWhiteSpace(searchText))
                return products;

            var searchLower = searchText.ToLower();

            return products.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.SKU.ToLower().Contains(searchLower) ||
                p.Category.Name.ToLower().Contains(searchLower) ||
                p.Unit.Name.ToLower().Contains(searchLower) ||
                (p.Stocks?.Sum(s => s.ProductQuantity) ?? 0).ToString().Contains(searchLower) ||
                ((p.Weight * (p.Stocks?.Sum(s => s.ProductQuantity) ?? 0)).ToString("0.##").Contains(searchLower))
            ).ToList();
        }
    }
}
