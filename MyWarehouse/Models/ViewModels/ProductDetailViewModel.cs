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

        public Product? Product { get; private set; }
        public ICollection<DeliveryTask>? MovementHistory { get; private set; }

        public bool CanExportCurrentData => Product != null;
        public bool CanExportFullHistory => Product != null && MovementHistory?.Count != 0;

        [RelayCommand]
        public async Task LoadProductAsync()
        {
            try
            {
                IsLoading = true;

                Product = await _productDetailService.GetProductWithDetailsAsync(_productId);
                MovementHistory = await _productDetailService.GetProductMovementHistoryAsync(_productId);

                if (Product == null)
                    return;

                Name = Product.Name;
                Sku = Product.SKU;
                Category = Product.Category.Name;
                Weight = Product.Weight;
                Length = Product.Length;
                Width = Product.Width;
                Height = Product.Height;
                IsFragile = Product.IsFragile;
                IsWaterSensitive = Product.IsWaterSensitive;
                Description = Product.Description ?? "Описание отсутствует";

                UpdateStocksAsync(Product);
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