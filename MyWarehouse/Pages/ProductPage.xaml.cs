using Microsoft.Win32;
using MyWarehouse.Models.ViewModels;
using MyWarehouse.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MyWarehouse.Pages
{
    public partial class ProductPage : Page
    {
        private const int AutoRefreshIntervalSeconds = 20;
        private readonly ProductDetailViewModel _viewModel;
        private readonly IPdfExportService _pdfExportService;
        private DispatcherTimer? _timer;

        public ProductPage(int productId, IProductDetailService productDetailService, IPdfExportService pdfExportService)
        {
            InitializeComponent();
            _pdfExportService = pdfExportService;
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

            _timer = new DispatcherTimer(DispatcherPriority.Background)
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

        private void ExportPdfButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Показываем контекстное меню при клике на кнопку
            if (ExportContextMenu != null)
            {
                ExportContextMenu.PlacementTarget = ExportPdfButton;
                ExportContextMenu.IsOpen = true;
            }
        }

        private void ExportCurrentDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportContextMenu.IsOpen = false; // Закрываем меню
            ExportCurrentData();
        }

        private void ExportFullHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportContextMenu.IsOpen = false; // Закрываем меню
            ExportFullHistory();
        }

        private void ExportCurrentData()
        {
            if (_viewModel.Product == null)
            {
                System.Windows.MessageBox.Show("Нет данных о товаре для экспорта.", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Товар_{_viewModel.Product.SKU}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _pdfExportService.ExportProductCurrentState(_viewModel.Product, saveFileDialog.FileName);
                    System.Windows.MessageBox.Show("Текущие данные товара успешно экспортированы в PDF.", "Успех",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExportFullHistory()
        {
            if (_viewModel.Product == null || _viewModel.MovementHistory == null)
            {
                System.Windows.MessageBox.Show("Нет данных для экспорта истории.", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"История_товара_{_viewModel.Product.SKU}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _pdfExportService.ExportProductFullHistory(_viewModel.Product, _viewModel.MovementHistory, saveFileDialog.FileName);
                    System.Windows.MessageBox.Show("Полная история товара успешно экспортирована в PDF.", "Успех",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}