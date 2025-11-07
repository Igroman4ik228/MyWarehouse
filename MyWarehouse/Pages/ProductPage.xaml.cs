using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private readonly AppDbContext db = new();

        public ObservableCollection<ProductViewModel> Products { get; } = [];

        public ProductPage()
        {
            InitializeComponent();
            DataContext = this;

            LoadProducts();
        }

        private void LoadProducts()
        {
            var products = db.CURS_Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
                .ToList();

            foreach (var p in products)
            {
                var total = p.Stocks?.Sum(s => s.ProductQuantity) ?? 0;
                Products.Add(new ProductViewModel
                {
                    Id = p.IdProduct,
                    Name = p.Name,
                    SKU = p.SKU,
                    Category = p.Category?.Name ?? string.Empty,
                    Weight = p.Weight,
                    StockTotal = total
                });
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (ProductsControl.ItemsSource is IEnumerable items)
                {
                    foreach (var it in items)
                    {
                        if (it is ProductViewModel pc && pc.Id == id)
                        {
                            MessageBox.Show($"Просмотр товара: {pc.Name}\nSKU: {pc.SKU}\nНа складе: {pc.StockTotal}", "Просмотр", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            break;
                        }
                    }
                }
            }
        }
        public class ProductViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SKU { get; set; }
            public string Category { get; set; }
            public decimal Weight { get; set; }
            public int StockTotal { get; set; }
        }
    }
}
