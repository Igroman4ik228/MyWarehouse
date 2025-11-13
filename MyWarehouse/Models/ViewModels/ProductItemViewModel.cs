using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace MyWarehouse.Models.ViewModels
{
    public partial class ProductItemViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _sku = string.Empty;

        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private decimal _weightTotal;

        [ObservableProperty]
        private int _quantityTotal;

        [ObservableProperty]
        private string _quantityName = string.Empty;

        [ObservableProperty]
        private bool _isFragile;

        [ObservableProperty]
        private bool _isWaterSensitive;

        public string DisplayWeight => $"{WeightTotal:N2} кг";
        public string DisplayQuantity => $"{QuantityTotal} {QuantityName}";

        public string FragileTooltip => IsFragile ? "Хрупкий товар" : "Не хрупкий";
        public string WaterSensitiveTooltip => IsWaterSensitive ? "Боится воды" : "Водостойкий";
    }
}