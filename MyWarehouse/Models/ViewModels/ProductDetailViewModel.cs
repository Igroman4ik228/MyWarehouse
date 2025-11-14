// ViewModels/ProductDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace MyWarehouse.Models.ViewModels
{
    public partial class ProductDetailViewModel(int productId, IProductDetailService productDetailService) : BaseViewModel
    {
        private readonly IProductDetailService _productDetailService = productDetailService;
        private readonly int _productId = productId;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _sku = string.Empty;

        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private decimal _weight;

        [ObservableProperty]
        private decimal _length;

        [ObservableProperty]
        private decimal _width;

        [ObservableProperty]
        private decimal _height;

        [ObservableProperty]
        private bool _isFragile;

        [ObservableProperty]
        private bool _isWaterSensitive;

        [ObservableProperty]
        private string _description = string.Empty;

        public ObservableCollection<StockViewModel> Stocks { get; } = [];

        [RelayCommand]
        public async Task LoadProductAsync()
        {
            try
            {
                IsLoading = true;

                var product = await _productDetailService.GetProductWithDetailsAsync(_productId);

                if (product == null)
                    return;

                Name = product.Name;
                Sku = product.SKU;
                Category = product.Category.Name;
                Weight = product.Weight;
                Length = product.Length;
                Width = product.Width;
                Height = product.Height;
                IsFragile = product.IsFragile;
                IsWaterSensitive = product.IsWaterSensitive;
                Description = product.Description ?? string.Empty;

                UpdateStocksAsync(product);
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                MessageBox.Show("Ошибка", $"Не удалось загрузить данные продукта: {ex.Message}", MessageBoxButton.OK);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateStocksAsync(Product product)
        {
            Stocks.Clear();

            var stocks = product.Stocks ?? [];
            foreach (var stock in stocks)
            {
                var location = stock.Location;
                var locationDisplay = location != null
                    ? $"{location.Zone} / {location.Shelf} / {location.Cell}"
                    : "Неизвестно";

                var locationDetails = location != null
                    ? $"ID:{location.IdLocation}"
                    : string.Empty;

                var constraints = location != null
                    ? $"{location.MaxLength:N2}/{location.MaxWidth:N2}/{location.MaxHeight:N2}/{location.MaxWeight:N2}"
                    : "-";

                Stocks.Add(new StockViewModel
                {
                    LocationDisplay = locationDisplay,
                    LocationDetails = locationDetails,
                    QuantityDisplay = $"{stock.ProductQuantity} / {stock.MaxProductQuantity}",
                    ConstraintsDisplay = constraints
                });
            }
        }
    }
}