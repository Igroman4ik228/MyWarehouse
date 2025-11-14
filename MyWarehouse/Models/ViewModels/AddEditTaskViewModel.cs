using CommunityToolkit.Mvvm.ComponentModel;
using MyWarehouse.Models.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MyWarehouse.Models.ViewModels
{
    public partial class AddEditTaskViewModel : BaseViewModel
    {
        private readonly AppDbContext _db;

        public AddEditTaskViewModel(AppDbContext db)
        {
            _db = db;
            InitializeData();
        }

        #region Properties

        [ObservableProperty]
        private ObservableCollection<Product> _products = [];

        [ObservableProperty]
        private ObservableCollection<Client> _clients = [];

        [ObservableProperty]
        private ObservableCollection<DeliveryType> _deliveryTypes = [];

        [ObservableProperty]
        private ObservableCollection<Location> _locations = [];

        [ObservableProperty]
        private Product? _selectedProduct;

        [ObservableProperty]
        private Client? _selectedClient;

        [ObservableProperty]
        private DeliveryType? _selectedDeliveryType;

        [ObservableProperty]
        private Location? _selectedFromLocation;

        [ObservableProperty]
        private Location? _selectedToLocation;

        [ObservableProperty]
        private int _quantity;

        [ObservableProperty]
        private string _quantityText = string.Empty;

        [ObservableProperty]
        private bool _isFromLocationVisible;

        [ObservableProperty]
        private bool _isToLocationVisible;

        [ObservableProperty]
        private string _maxQuantityInfo = string.Empty;

        [ObservableProperty]
        private string _validationMessage = string.Empty;

        // Дополнительные свойства для валидации
        private int _availableQuantity = 0;
        private int _availableSpace = 0;
        private bool _hasSourceStock = false;
        private bool _hasDestinationStock = false;

        // Свойство для проверки валидности формы
        public bool IsFormValid
        {
            get
            {
                var validationResult = ValidateForm();
                ValidationMessage = validationResult.ErrorMessage;
                return validationResult.IsValid;
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

        // Комплексная валидация формы
        private ValidationResult ValidateForm()
        {
            // Базовая проверка обязательных полей
            if (SelectedProduct == null)
                return new ValidationResult("Выберите продукт");

            if (SelectedClient == null)
                return new ValidationResult("Выберите клиента");

            if (SelectedDeliveryType == null)
                return new ValidationResult("Выберите тип доставки");

            // Проверка количества
            if (Quantity <= 0)
                return new ValidationResult("Количество должно быть больше 0");

            if (Quantity > 1000000) // Разумное ограничение
                return new ValidationResult("Количество слишком большое");

            // Проверка локаций в зависимости от типа доставки
            var locationValidation = ValidateLocations();
            if (!locationValidation.IsValid)
                return locationValidation;

            // Проверка доступности товара и места
            var stockValidation = ValidateStockAvailability();
            if (!stockValidation.IsValid)
                return stockValidation;

            // Дополнительные бизнес-правила
            var businessValidation = ValidateBusinessRules();
            if (!businessValidation.IsValid)
                return businessValidation;

            return ValidationResult.Success;
        }

        private ValidationResult ValidateLocations()
        {
            return SelectedDeliveryType?.IdDeliveryType switch
            {
                (int)DeliveryOperationType.Moving =>
                    SelectedFromLocation == null || SelectedToLocation == null
                    ? new ValidationResult("Для перемещения должны быть выбраны исходная и целевая локации")
                    : SelectedFromLocation.IdLocation == SelectedToLocation.IdLocation
                    ? new ValidationResult("Исходная и целевая локации не могут совпадать")
                    : ValidationResult.Success,

                (int)DeliveryOperationType.Incoming =>
                    SelectedToLocation == null
                    ? new ValidationResult("Для поставки должна быть выбрана целевая локация")
                    : ValidationResult.Success,

                (int)DeliveryOperationType.Outgoing =>
                    SelectedFromLocation == null
                    ? new ValidationResult("Для отгрузки должна быть выбрана исходная локация")
                    : ValidationResult.Success,

                _ => new ValidationResult("Неизвестный тип доставки")
            };
        }

        private ValidationResult ValidateStockAvailability()
        {
            if (SelectedProduct == null)
                return ValidationResult.Success;

            switch (SelectedDeliveryType?.IdDeliveryType)
            {
                case (int)DeliveryOperationType.Moving:
                    if (!_hasSourceStock)
                        return new ValidationResult("Товар отсутствует на исходной локации");

                    if (Quantity > _availableQuantity)
                        return new ValidationResult($"Недостаточно товара на исходной локации. Доступно: {_availableQuantity}");

                    if (!_hasDestinationStock)
                        return new ValidationResult("Товар отсутствует на целевой локации. Сначала нужно добавить товар на локацию");

                    if (Quantity > _availableSpace)
                        return new ValidationResult($"Недостаточно места на целевой локации. Доступно места: {_availableSpace}");
                    break;

                case (int)DeliveryOperationType.Incoming:
                    if (Quantity > _availableSpace)
                        return new ValidationResult($"Недостаточно места на целевой локации. Доступно места: {_availableSpace}");
                    break;

                case (int)DeliveryOperationType.Outgoing:
                    if (!_hasSourceStock)
                        return new ValidationResult("Товар отсутствует на исходной локации");

                    if (Quantity > _availableQuantity)
                        return new ValidationResult($"Недостаточно товара на исходной локации. Доступно: {_availableQuantity}");
                    break;
            }

            return ValidationResult.Success;
        }

        private ValidationResult ValidateBusinessRules()
        {
            // Проверка срока годности (если применимо)
            if (SelectedProduct?.ExpirationDate != null && SelectedProduct.ExpirationDate < DateTime.Now)
                return new ValidationResult("Товар с истекшим сроком годности");

            return ValidationResult.Success;
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            UpdateStockInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedClientChanged(Client? value)
        {
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedDeliveryTypeChanged(DeliveryType? value)
        {
            UpdateLocationVisibility();
            UpdateStockInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedFromLocationChanged(Location? value)
        {
            UpdateStockInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnSelectedToLocationChanged(Location? value)
        {
            UpdateStockInfo();
            OnPropertyChanged(nameof(IsFormValid));
        }

        partial void OnQuantityTextChanged(string value)
        {
            if (int.TryParse(value, out int quantity) && quantity >= 0)
            {
                Quantity = quantity;
            }
            else
            {
                Quantity = 0;
            }
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

        private void UpdateStockInfo()
        {
            _availableQuantity = 0;
            _availableSpace = 0;
            _hasSourceStock = false;
            _hasDestinationStock = false;

            // Получаем информацию об исходной локации
            if (SelectedFromLocation != null &&
                (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                 SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Outgoing))
            {
                var sourceStock = _db.CURS_Stocks
                    .FirstOrDefault(s => s.ProductId == SelectedProduct.IdProduct &&
                                       s.LocationId == SelectedFromLocation.IdLocation);

                if (sourceStock != null)
                {
                    _hasSourceStock = true;
                    _availableQuantity = sourceStock.ProductQuantity;
                }
            }

            // Получаем информацию о целевой локации
            if (SelectedToLocation != null &&
                (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                 SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Incoming))
            {
                var destinationStock = _db.CURS_Stocks
                    .FirstOrDefault(s => s.ProductId == SelectedProduct.IdProduct &&
                                       s.LocationId == SelectedToLocation.IdLocation);

                if (destinationStock != null)
                {
                    _hasDestinationStock = true;
                    _availableSpace = destinationStock.MaxProductQuantity - destinationStock.ProductQuantity;
                }
            }

            // Обновляем информационное сообщение
            UpdateMaxQuantityInfoMessage();
        }

        private void UpdateMaxQuantityInfoMessage()
        {
            var messages = new List<string>();

            if (_hasSourceStock && (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                                   SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Outgoing))
            {
                messages.Add($"Доступно на исходной: {_availableQuantity}");
            }
            else if (SelectedFromLocation != null &&
                    (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                     SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Outgoing))
            {
                messages.Add("Товар отсутствует на исходной локации");
            }

            if (_hasDestinationStock && (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                                        SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Incoming))
            {
                messages.Add($"Доступно места: {_availableSpace}");
            }
            else if (SelectedToLocation != null &&
                    (SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Moving ||
                     SelectedDeliveryType?.IdDeliveryType == (int)DeliveryOperationType.Incoming))
            {
                messages.Add("Товар отсутствует на целевой локации");
            }

            MaxQuantityInfo = messages.Count != 0 ? string.Join(" | ", messages) : string.Empty;
        }

        #endregion
    }

    // Вспомогательный класс для результатов валидации
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public ValidationResult(string errorMessage)
        {
            IsValid = false;
            ErrorMessage = errorMessage;
        }

        private ValidationResult()
        {
            IsValid = true;
            ErrorMessage = string.Empty;
        }

        public static ValidationResult Success { get; } = new ValidationResult();
    }
}