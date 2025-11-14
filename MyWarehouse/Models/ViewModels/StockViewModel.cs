using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWarehouse.Models.ViewModels
{
    public partial class StockViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _locationDisplay = string.Empty;

        [ObservableProperty]
        private string _locationDetails = string.Empty;

        [ObservableProperty]
        private string _quantityDisplay = string.Empty;

        [ObservableProperty]
        private string _constraintsDisplay = string.Empty;
    }
}