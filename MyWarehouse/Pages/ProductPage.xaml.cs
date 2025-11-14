using MyWarehouse.Models.ViewModels;
using MyWarehouse.Services;
using System.Windows.Controls;

namespace MyWarehouse.Pages
{
    public partial class ProductPage : Page
    {
        private const int AutoRefreshIntervalSeconds = 20;
        private readonly ProductDetailViewModel _viewModel;
        private System.Windows.Threading.DispatcherTimer? _timer;

        public ProductPage(int productId, IProductDetailService productDetailService)
        {
            InitializeComponent();
            _viewModel = new ProductDetailViewModel(productId, productDetailService);
            DataContext = _viewModel;
        }

        private async void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.LoadProductCommand.ExecuteAsync(null);
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public async Task StartAutoRefreshAsync(int intervalSeconds = AutoRefreshIntervalSeconds)
        {
            await _viewModel.LoadProductCommand.ExecuteAsync(null);

            _timer = new System.Windows.Threading.DispatcherTimer(
                System.Windows.Threading.DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(intervalSeconds)
            };
            _timer.Tick += async (s, e) => await _viewModel.LoadProductCommand.ExecuteAsync(null);
            _timer.Start();
        }

        public void StopAutoRefresh()
        {
            _timer?.Stop();
            _timer = null;
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await StartAutoRefreshAsync(AutoRefreshIntervalSeconds);
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            StopAutoRefresh();
        }
    }
}