using MyWarehouse.Models.Entities;
using MyWarehouse.Models.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyWarehouse.Windows
{
    public partial class AddEditTaskWindow : Window
    {
        private readonly AddEditTaskViewModel _viewModel;

        public int SelectedProductId => _viewModel.SelectedProduct?.IdProduct ?? 0;
        public int SelectedClientId => _viewModel.SelectedClient?.IdClient ?? 0;
        public int SelectedDeliveryTypeId => _viewModel.SelectedDeliveryType?.IdDeliveryType ?? 0;
        public int? SelectedFromLocationId => _viewModel.SelectedFromLocation?.IdLocation;
        public int? SelectedToLocationId => _viewModel.SelectedToLocation?.IdLocation;
        public int Quantity => _viewModel.Quantity;

        public AddEditTaskWindow(AppDbContext db)
        {
            InitializeComponent();
            _viewModel = new AddEditTaskViewModel(db);
            DataContext = _viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}