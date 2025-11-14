using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyWarehouse.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace MyWarehouse.Models.ViewModels
{
    public partial class ProductsViewModel(IProductService productService) : BaseViewModel
    {
        private readonly IProductService _productService = productService;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public ObservableCollection<ProductItemViewModel> Products { get; } = [];
        public ObservableCollection<ProductItemViewModel> FilteredProducts { get; } = [];

        [RelayCommand]
        private async Task LoadProductsAsync()
        {

            try
            {
                var products = await _productService.GetProductsWithDetailsAsync();

                Products.Clear();
                foreach (var product in products)
                {
                    var totalQuantity = product.Stocks?.Sum(s => s.ProductQuantity) ?? 0;
                    var totalWeight = product.Weight * totalQuantity;

                    Products.Add(new ProductItemViewModel
                    {
                        Id = product.IdProduct,
                        Name = product.Name,
                        Sku = product.SKU,
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        partial void OnSearchTextChanged(string value)
        {
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
                    if (MatchesSearchCriteria(product, searchLower))
                    {
                        FilteredProducts.Add(product);
                    }
                }
            }
        }

        private bool MatchesSearchCriteria(ProductItemViewModel product, string searchLower)
        {
            // Поиск по текстовым полям
            if (product.Name.ToLower().Contains(searchLower) ||
                product.Sku.ToLower().Contains(searchLower) ||
                product.Category.ToLower().Contains(searchLower) ||
                product.QuantityName.ToLower().Contains(searchLower) ||
                product.QuantityTotal.ToString().Contains(searchLower) ||
                product.WeightTotal.ToString("0.##", CultureInfo.InvariantCulture).Contains(searchLower) ||
                product.WeightTotal.ToString("0.##").Contains(searchLower))
            {
                return true;
            }

            // Поиск по булевым свойствам через ключевые слова
            return SearchByBooleanProperties(product, searchLower);
        }

        private bool SearchByBooleanProperties(ProductItemViewModel product, string searchLower)
        {
            var fragileKeywords = new[] { "хрупк", "бьется", "ломк", "осторожн", "fragile", "breakable" };
            var waterSensitiveKeywords = new[] { "вод", "влаг", "мокр", "сыро", "water", "moisture", "wet" };
            var negativeKeywords = new[] { "не ", "нет", "not ", "no " };

            var isNegative = negativeKeywords.Any(keyword => searchLower.Contains(keyword));

            if (fragileKeywords.Any(keyword => searchLower.Contains(keyword)))
            {
                return isNegative ? !product.IsFragile : product.IsFragile;
            }

            if (waterSensitiveKeywords.Any(keyword => searchLower.Contains(keyword)))
            {
                return isNegative ? !product.IsWaterSensitive : product.IsWaterSensitive;
            }

            if (searchLower.Contains("да") || searchLower.Contains("yes") || searchLower.Contains("true"))
            {
                return product.IsFragile || product.IsWaterSensitive;
            }

            if (searchLower.Contains("нет") || searchLower.Contains("no") || searchLower.Contains("false"))
            {
                return !product.IsFragile && !product.IsWaterSensitive;
            }

            return false;
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }
    }
}