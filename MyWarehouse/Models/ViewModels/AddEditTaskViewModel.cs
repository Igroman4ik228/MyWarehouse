using MyWarehouse.Models.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MyWarehouse.ViewModels
{
    public class AddEditTaskViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private DeliveryTask _editingTask;

        public AddEditTaskViewModel(AppDbContext db, DeliveryTask task = null)
        {
            _db = db;
            _editingTask = task;
            InitializeData();

            if (task != null)
                PopulateFromTask(task);
        }

        #region Properties

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(nameof(Products)); }
        }

        private ObservableCollection<Client> _clients;
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(nameof(Clients)); }
        }

        private ObservableCollection<DeliveryType> _deliveryTypes;
        public ObservableCollection<DeliveryType> DeliveryTypes
        {
            get => _deliveryTypes;
            set { _deliveryTypes = value; OnPropertyChanged(nameof(DeliveryTypes)); }
        }

        private ObservableCollection<Location> _locations;
        public ObservableCollection<Location> Locations
        {
            get => _locations;
            set { _locations = value; OnPropertyChanged(nameof(Locations)); }
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                UpdateMaxQuantityInfo();
                OnPropertyChanged(nameof(IsFormValid)); // Добавляем уведомление о валидности
            }
        }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        private DeliveryType _selectedDeliveryType;
        public DeliveryType SelectedDeliveryType
        {
            get => _selectedDeliveryType;
            set
            {
                _selectedDeliveryType = value;
                OnPropertyChanged(nameof(SelectedDeliveryType));
                UpdateLocationVisibility();
                UpdateMaxQuantityInfo();
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        private Location _selectedFromLocation;
        public Location SelectedFromLocation
        {
            get => _selectedFromLocation;
            set
            {
                _selectedFromLocation = value;
                OnPropertyChanged(nameof(SelectedFromLocation));
                UpdateMaxQuantityInfo();
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        private Location _selectedToLocation;
        public Location? SelectedToLocation
        {
            get => _selectedToLocation;
            set
            {
                _selectedToLocation = value;
                OnPropertyChanged(nameof(SelectedToLocation));
                UpdateMaxQuantityInfo();
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        private string _quantityText = "";
        public string QuantityText
        {
            get => _quantityText;
            set
            {
                _quantityText = value;
                OnPropertyChanged(nameof(QuantityText));

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
        }

        private bool _isFromLocationVisible;
        public bool IsFromLocationVisible
        {
            get => _isFromLocationVisible;
            set { _isFromLocationVisible = value; OnPropertyChanged(nameof(IsFromLocationVisible)); }
        }

        private bool _isToLocationVisible;
        public bool IsToLocationVisible
        {
            get => _isToLocationVisible;
            set { _isToLocationVisible = value; OnPropertyChanged(nameof(IsToLocationVisible)); }
        }

        private string _maxQuantityInfo = "Выберите продукт и локации";
        public string MaxQuantityInfo
        {
            get => _maxQuantityInfo;
            set { _maxQuantityInfo = value; OnPropertyChanged(nameof(MaxQuantityInfo)); }
        }

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
                    // Внутренняя
                    1 => SelectedFromLocation != null && SelectedToLocation != null,
                    // Внешняя поставка
                    2 => SelectedToLocation != null,
                    // Выгрузка
                    3 => SelectedFromLocation != null,
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
                case 1: // Внутренняя
                    IsFromLocationVisible = true;
                    IsToLocationVisible = true;
                    break;
                case 2: // Внешняя поставка
                    IsFromLocationVisible = false;
                    IsToLocationVisible = true;
                    SelectedFromLocation = null;
                    break;
                case 3: // Выгрузка
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
            if (SelectedDeliveryType?.IdDeliveryType == 1 && SelectedFromLocation != null)
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
            if ((SelectedDeliveryType?.IdDeliveryType == 1 || SelectedDeliveryType?.IdDeliveryType == 2) && SelectedToLocation != null)
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}