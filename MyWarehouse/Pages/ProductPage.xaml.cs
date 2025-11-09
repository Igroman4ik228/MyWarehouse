using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private const int AutoRefreshIntervalSeconds = 20;

        private readonly AppDbContext _db;
        private DispatcherTimer? timer;
        private int ProductId { get; }

        public ProductPage(int productId, AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            ProductId = productId;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshProductAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public async Task StartAutoRefreshAsync(int intervalSeconds = AutoRefreshIntervalSeconds)
        {
            await RefreshProductAsync();

            timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(intervalSeconds)
            };
            timer.Tick += async (s, e) => await RefreshProductAsync();
            timer.Start();
        }

        public void StopAutoRefresh()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private async Task RefreshProductAsync()
        {
            try
            {
                var product = await _db.CURS_Products
                    .Where(p => p.IdProduct == ProductId)
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .Include(p => p.Stocks)
                        .ThenInclude(s => s.Location)
                    .FirstOrDefaultAsync();

                if (product == null)
                    return;

                var stocks = product.Stocks ?? Array.Empty<Stock>();

                var newVm = new ProductDetailViewModel
                {
                    Name = product.Name,
                    SKU = product.SKU,
                    Category = product.Category.Name,
                    Weight = product.Weight,
                    Length = product.Length,
                    Width = product.Width,
                    Height = product.Height,
                    IsFragile = product.IsFragile,
                    IsWaterSensitive = product.IsWaterSensitive,
                    Description = product.Description ?? string.Empty,
                };

                foreach (var s in stocks)
                {
                    var loc = s.Location;
                    var locDisplay = loc != null ? $"{loc.Zone} / {loc.Shelf} / {loc.Cell}" : "Неизвестно";
                    var locDetails = loc != null ? $"ID:{loc.IdLocation}" : string.Empty;

                    var constraints = loc != null ?
                        $"{loc.MaxLength:N2}/{loc.MaxWidth:N2}/{loc.MaxHeight:N2}/{loc.MaxWeight:N2}" : "-";

                    newVm.Stocks.Add(new StockViewModel
                    {
                        LocationDisplay = locDisplay,
                        LocationDetails = locDetails,
                        QuantityDisplay = $"{s.ProductQuantity} / {s.MaxProductQuantity}",
                        ConstraintsDisplay = constraints
                    });
                }

                DataContext = newVm;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            await StartAutoRefreshAsync(AutoRefreshIntervalSeconds);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopAutoRefresh();
        }
    }

    public class ProductDetailViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public bool IsFragile { get; set; }
        public bool IsWaterSensitive { get; set; }
        public string Description { get; set; } = string.Empty;

        public ObservableCollection<StockViewModel> Stocks { get; } = [];
    }

    public class StockViewModel
    {
        public string LocationDisplay { get; set; } = string.Empty;
        public string LocationDetails { get; set; } = string.Empty;
        public string QuantityDisplay { get; set; } = string.Empty;
        public string ConstraintsDisplay { get; set; } = string.Empty;
    }
}
