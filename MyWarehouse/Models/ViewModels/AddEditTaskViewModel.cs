using CommunityToolkit.Mvvm.ComponentModel;
using MyWarehouse.Models.Entities;
using System.Collections.ObjectModel;

namespace MyWarehouse.Models.ViewModels
{
    public partial class AddEditTaskViewModel : BaseViewModel
    {
        private readonly AppDbContext _db;
        private readonly DeliveryTask _editingTask;

        public AddEditTaskViewModel(AppDbContext db, DeliveryTask task = null)
        {
            _db = db;
            _editingTask = task;
            InitializeData();

            if (task != null)
                PopulateFromTask(task);
        }

        #region Properties

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private ObservableCollection<Client> _clients;

        [ObservableProperty]
        private ObservableCollection<DeliveryType> _deliveryTypes;

        [ObservableProperty]
        private ObservableCollection<Location> _locations;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private Client _selectedClient;

        [ObservableProperty]
        private DeliveryType _selectedDeliveryType;

        [ObservableProperty]
        private Location _selectedFromLocation;

        [ObservableProperty]
        private Location _selectedToLocation;

        [ObservableProperty]
        private int _quantity;

        [ObservableProperty]
        private string _quantityText = "";

        [ObservableProperty]
        private bool _isFromLocationVisible;

        [ObservableProperty]
        private bool _isToLocationVisible;

        [ObservableProperty]
        private string _maxQuantityInfo = "Выберите продукт и локации";

        // Свойство для проверки базовой валидности формы
        public bool IsFormValid
        {
            get
            {
                // Базовая проверка обязательных полей
                if (SelectedProduct == null || SelectedClient == null || SelectedDeliveryType == null)
                    return false;

                if (Quantity <= 0)
                    return false;

                // Проверка локаций в зависимости от типа доставки
                return SelectedDeliveryType.IdDeliveryType switch
                {
                    (int)DeliveryOperationType.Moving => SelectedFromLocation != null && SelectedToLocation != null,
                    (int)DeliveryOperationType.Incoming => SelectedToLocation != null,
                    (int)DeliveryOperationType.Outgoing => SelectedFromLocation != null,
                    _ => false,
                };
            }
        }

        #endregion

        #region Methods

        private void InitializeData()
        {
            Products = new ObservableCollection<Product>(_db.CURS_Products.ToList());
            Clients = new ObservableCollection<Client>(_db.CURS_Clients.ToList());
            DeliveryTypes = new ObservableCollection<DeliveryType>(_db.CURS_DeliveryTypes.ToList());
            Locations = new ObservableCollection<Location>(_db.CURS_Locations.ToList());
        }

        private void PopulateFromTask(DeliveryTask task)
        {
            SelectedProduct = Products?.FirstOrDefault(p => p.IdProduct == task.ProductId);
            SelectedClient = Clients?.FirstOrDefault(c => c.IdClient == task.ClientId);
            SelectedDeliveryType = DeliveryTypes?.FirstOrDefault(d => d.IdDeliveryType == task.DeliveryTypeId);
            QuantityText = task.ProductQuantity.ToString();
            SelectedFromLocation = Locations?.FirstOrDefault(l => l.IdLocation == task.FromLocationId);
            SelectedToLocation = Locations?.FirstOrDefault(l => l.IdLocation == task.ToLocationId);
        }

        partial void OnSelectedProductChanged(Product value)
        {
            UpdateMaxQuantityInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedClientChanged(Client value)
        {
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedDeliveryTypeChanged(DeliveryType value)
        {
            UpdateLocationVisibility();
            UpdateMaxQuantityInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedFromLocationChanged(Location value)
        {
            UpdateMaxQuantityInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedToLocationChanged(Location value)
        {
            UpdateMaxQuantityInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnQuantityTextChanged(string value)
        {
            if (int.TryParse(value, out int quantity))
            {
                Quantity = quantity;
            }
            else
            {
                Quantity = 0; // Если не число, устанавливаем 0
            }

            UpdateMaxQuantityInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(IsFormValid));
        }

        private void UpdateLocationVisibility()
        {
            if (SelectedDeliveryType == null)
            {
                IsFromLocationVisible = false;
                IsToLocationVisible = false;
                return;
            }

            switch (SelectedDeliveryType.IdDeliveryType)
            {
                case (int)DeliveryOperationType.Moving:
                    IsFromLocationVisible = true;
                    IsToLocationVisible = true;
                    break;
                case (int)DeliveryOperationType.Incoming:
                    IsFromLocationVisible = false;
                    IsToLocationVisible = true;
                    SelectedFromLocation = null;
                    break;
                case (int)DeliveryOperationType.Outgoing:
                    IsFromLocationVisible = true;
                    IsToLocationVisible = false;
                    SelectedToLocation = null;
                    break;
                default:
                    IsFromLocationVisible = false;
                    IsToLocationVisible = false;
                    break;
            }
        }

        private void UpdateMaxQuantityInfo()
        {
            if (SelectedProduct == null)
            {
                MaxQuantityInfo = "Выберите продукт";
                return;
            }

            if (_db == null)
            {
                MaxQuantityInfo = "База данных не инициализирована";
                return;
            }

            // Для внутренней доставки - показываем доступное количество на исходной локации
            if (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving && SelectedFromLocation != null)
            {
                var stock = _db.CURS_Stocks
                    .FirstOrDefault(s => s.ProductId == SelectedProduct.IdProduct && s.LocationId == SelectedFromLocation.IdLocation);

                if (stock != null)
                {
                    MaxQuantityInfo = $"Доступно на локации: {stock.ProductQuantity}";
                    return;
                }
                else
                {
                    MaxQuantityInfo = "Товар отсутствует на выбранной локации";
                    return;
                }
            }

            // Для целевой локации (внутренняя доставка и внешняя поставка) - показываем доступное место
            if ((SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                 SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Incoming) &&
                SelectedToLocation != null)
            {
                var stock = _db.CURS_Stocks
                    .FirstOrDefault(s => s.ProductId == SelectedProduct.IdProduct && s.LocationId == SelectedToLocation.IdLocation);

                if (stock != null)
                {
                    // Точный расчет: доступное место = MaxProductQuantity - текущее количество
                    var availableSpace = stock.MaxProductQuantity - stock.ProductQuantity;
                    MaxQuantityInfo = $"Доступно места на локации: {availableSpace} (из {stock.MaxProductQuantity})";
                }
                else
                {
                    // Если товара еще нет на локации
                    MaxQuantityInfo = $"Товара еще нет на локации";
                }
                return;
            }

            MaxQuantityInfo = "Выберите локации для отображения информации";
        }

        #endregion
    }
}
