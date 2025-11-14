using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWarehouse.Models.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoading;
    }
}
