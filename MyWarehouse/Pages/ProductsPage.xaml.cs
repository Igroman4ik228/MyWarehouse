using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for ProductPage.xaml
    /// </summary>
    public partial class ProductsPage : Page, INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private string _searchText = string.Empty;

        public ObservableCollection<ProductViewModel> Products { get; } = [];
        public ObservableCollection<ProductViewModel> FilteredProducts { get; } = [];

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public ProductsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            DataContext = this;
        }

        private async Task LoadProducts()
        {
            var products = await _db.CURS_Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
                .Include(p => p.Unit)
                .ToListAsync();

            Products.Clear();
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
            FilterProducts();
        }

        private void FilterProducts()
        {
            FilteredProducts.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var product in Products)
                {
                    FilteredProducts.Add(product);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();

                foreach (var product in Products)
                {
                    // Поиск по текстовым полям
                    if (product.Name.ToLower().Contains(searchLower) ||
                        product.SKU.ToLower().Contains(searchLower) ||
                        product.Category.ToLower().Contains(searchLower) ||
                        product.QuantityName.ToLower().Contains(searchLower))
                    {
                        FilteredProducts.Add(product);
                        continue;
                    }

                    // Поиск по числовым полям
                    if (product.QuantityTotal.ToString().Contains(searchLower) ||
                        product.WeightTotal.ToString("0.##", CultureInfo.InvariantCulture).Contains(searchLower) ||
                        product.WeightTotal.ToString("0.##").Contains(searchLower))
                    {
                        FilteredProducts.Add(product);
                        continue;
                    }

                    // Поиск по булевым значениям через ключевые слова
                    if (SearchByBooleanProperties(product, searchLower))
                    {
                        FilteredProducts.Add(product);
                        continue;
                    }
                }
            }
        }

        private bool SearchByBooleanProperties(ProductViewModel product, string searchLower)
        {
            // Ключевые слова для хрупких товаров
            var fragileKeywords = new[] { "хрупк", "бьется", "ломк", "осторожн", "fragile", "breakable" };
            // Ключевые слова для водобоязненных товаров
            var waterSensitiveKeywords = new[] { "вод", "влаг", "мокр", "сыро", "water", "moisture", "wet" };

            // Проверка на отрицание (например, "не хрупкий")
            var negativeKeywords = new[] { "не ", "нет", "not ", "no " };
            var isNegative = negativeKeywords.Any(keyword => searchLower.Contains(keyword));

            // Поиск по хрупкости
            if (fragileKeywords.Any(keyword => searchLower.Contains(keyword)))
            {
                // Если есть отрицание - ищем НЕ хрупкие товары, иначе хрупкие
                return isNegative ? !product.IsFragile : product.IsFragile;
            }

            // Поиск по водобоязненности
            if (waterSensitiveKeywords.Any(keyword => searchLower.Contains(keyword)))
            {
                // Если есть отрицание - ищем НЕ водобоязненные товары, иначе водобоязненные
                return isNegative ? !product.IsWaterSensitive : product.IsWaterSensitive;
            }

            // Поиск по общим булевым ключевым словам
            if (searchLower.Contains("да") || searchLower.Contains("yes") || searchLower.Contains("true"))
            {
                // Ищем товары, у которых есть хотя бы одно булевое свойство true
                return product.IsFragile || product.IsWaterSensitive;
            }

            if (searchLower.Contains("нет") || searchLower.Contains("no") || searchLower.Contains("false"))
            {
                // Ищем товары, у которых оба булевых свойства false
                return !product.IsFragile && !product.IsWaterSensitive;
            }

            return false;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductViewModel product)
            {
                NavigationService.Navigate(new ProductPage(product.Id, _db));
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Products.Clear();
            FilteredProducts.Clear();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddEditProductWindow(_db);
            addProductWindow.Owner = Window.GetWindow(this);

            if (addProductWindow.ShowDialog() == true)
            {
                // Обновляем список товаров
                Products.Clear();
                FilteredProducts.Clear();
                await LoadProducts();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = ((TextBox)sender).Text;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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