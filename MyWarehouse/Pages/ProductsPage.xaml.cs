using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for ProductPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private readonly AppDbContext db = new();

        public ObservableCollection<ProductViewModel> Products { get; } = [];

        public ProductsPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async Task LoadProducts()
        {
            var products = await db.CURS_Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
                .Include(p => p.Unit)
                .ToListAsync();

            foreach (var product in products)
            {
                var totalQuantity = product.Stocks?.Sum(s => s.ProductQuantity) ?? 0;
                var totalWeight = product.Weight * totalQuantity;

                Products.Add(new ProductViewModel
                {
                    Id = product.IdProduct,
                    Name = product.Name,
                    SKU = product.SKU,
                    Category = product.Category.Name,
                    WeightTotal = totalWeight,
                    QuantityTotal = totalQuantity,
                    QuantityName = product.Unit.Name,
                    IsFragile = product.IsFragile,
                    IsWaterSensitive = product.IsWaterSensitive,
                });
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductViewModel product)
            {
                NavigationService.Navigate(App.ServiceProvider.GetService<ProductPage>());
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
           await LoadProducts();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Products.Clear();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal WeightTotal { get; set; }
        public int QuantityTotal { get; set; }
        public string QuantityName { get; set; } = string.Empty;
        public bool IsFragile { get; set; }
        public bool IsWaterSensitive { get; set; }
    }
}
