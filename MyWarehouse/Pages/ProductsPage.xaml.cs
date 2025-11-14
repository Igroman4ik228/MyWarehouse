using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Models.Entities;
using MyWarehouse.Models.ViewModels;
using MyWarehouse.Services;
using MyWarehouse.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MyWarehouse.Pages
{
    public partial class ProductsPage : Page
    {
        private readonly AppDbContext _context;
        private readonly IProductDetailService _productDetailService;
        private readonly IPdfExportService _pdfExportService;
        private readonly ProductsViewModel _viewModel;

        public ProductsPage(AppDbContext context, IProductService productService, IProductDetailService productDetailService, IPdfExportService pdfExportService)
        {
            InitializeComponent();

            _context = context;
            _productDetailService = productDetailService;
            _pdfExportService = pdfExportService;
            _viewModel = new ProductsViewModel(productService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadProductsCommand.ExecuteAsync(null);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Products.Clear();
            _viewModel.FilteredProducts.Clear();
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductItemViewModel product)
            {
                NavigationService.Navigate(new ProductPage(product.Id, _productDetailService, _pdfExportService));
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddEditProductWindow(_context)
            {
                Owner = Window.GetWindow(this)
            };

            if (addProductWindow.ShowDialog() == true)
            {
                // Обновляем список товаров
                _ = _viewModel.LoadProductsCommand.ExecuteAsync(null);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = ((TextBox)sender).Text;
        }
    }
}