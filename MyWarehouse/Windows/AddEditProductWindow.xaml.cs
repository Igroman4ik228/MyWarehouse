using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;

namespace MyWarehouse.Windows
{
    public partial class AddEditProductWindow : Window
    {
        private readonly AddEditProductViewModel _viewModel;

        // Конструктор для создания нового товара
        public AddEditProductWindow(AppDbContext db)
        {
            InitializeComponent();
            _viewModel = new AddEditProductViewModel(db);
            DataContext = _viewModel;
        }

        // Конструктор для редактирования существующего товара
        public AddEditProductWindow(AppDbContext db, int productId)
        {
            InitializeComponent();
            _viewModel = new AddEditProductViewModel(db, true, productId);
            DataContext = _viewModel;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);

            if (await _viewModel.SaveAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }
    }

    public class AddEditProductViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly bool _isEditMode;
        private readonly int _productId;

        public AddEditProductViewModel(AppDbContext db, bool isEditMode = false, int productId = 0)
        {
            _db = db;
            _isEditMode = isEditMode;
            _productId = productId;
        }

        #region Properties

        public string WindowTitle => _isEditMode ? "Редактирование товара" : "Добавление нового товара";

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        private string _sku = string.Empty;
        public string SKU
        {
            get => _sku;
            set { _sku = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set { _categoryId = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        private int _unitId;
        public int UnitId
        {
            get => _unitId;
            set { _unitId = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        private decimal _weight;
        public decimal Weight
        {
            get => _weight;
            set { _weight = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        private decimal _length;
        public decimal Length
        {
            get => _length;
            set { _length = value; OnPropertyChanged(); }
        }

        private decimal _width;
        public decimal Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }

        private decimal _height;
        public decimal Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }

        private DateTime? _expirationDate;
        public DateTime? ExpirationDate
        {
            get => _expirationDate;
            set { _expirationDate = value; OnPropertyChanged(); }
        }

        private bool _isFragile;
        public bool IsFragile
        {
            get => _isFragile;
            set { _isFragile = value; OnPropertyChanged(); }
        }

        private bool _isWaterSensitive;
        public bool IsWaterSensitive
        {
            get => _isWaterSensitive;
            set { _isWaterSensitive = value; OnPropertyChanged(); }
        }

        private int _initialQuantity;
        public int InitialQuantity
        {
            get => _initialQuantity;
            set { _initialQuantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        private int _locationId;
        public int LocationId
        {
            get => _locationId;
            set { _locationId = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); }
        }

        public List<Category> Categories { get; private set; } = [];
        public List<Unit> Units { get; private set; } = [];
        public List<Location> Locations { get; private set; } = [];

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) &&
                              !string.IsNullOrWhiteSpace(SKU) &&
                              CategoryId > 0 &&
                              UnitId > 0 &&
                              Weight > 0 &&
                              (!_isEditMode ? (InitialQuantity >= 0 && LocationId > 0) : true);

        public bool ShowLocationSelection => !_isEditMode;

        #endregion

        #region Methods

        public async Task LoadDataAsync()
        {
            try
            {
                Categories = await _db.CURS_Categories.ToListAsync();
                Units = await _db.CURS_Units.ToListAsync();
                Locations = await _db.CURS_Locations.ToListAsync();

                OnPropertyChanged(nameof(Categories));
                OnPropertyChanged(nameof(Units));
                OnPropertyChanged(nameof(Locations));

                // Устанавливаем первую локацию по умолчанию, если есть
                if (Locations.Count != 0 && !_isEditMode)
                {
                    LocationId = Locations.First().IdLocation;
                }

                if (_isEditMode)
                {
                    await LoadProductAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadProductAsync()
        {
            try
            {
                var product = await _db.CURS_Products
                    .FirstOrDefaultAsync(p => p.IdProduct == _productId);

                if (product != null)
                {
                    Name = product.Name;
                    SKU = product.SKU;
                    Description = product.Description ?? string.Empty;
                    CategoryId = product.CategoryId;
                    UnitId = product.UnitId;
                    Weight = product.Weight;
                    Length = product.Length;
                    Width = product.Width;
                    Height = product.Height;
                    ExpirationDate = product.ExpirationDate;
                    IsFragile = product.IsFragile;
                    IsWaterSensitive = product.IsWaterSensitive;

                    // Уведомляем об изменении всех свойств
                    var properties = new[]
                    {
                        nameof(Name), nameof(SKU), nameof(Description), nameof(CategoryId),
                        nameof(UnitId), nameof(Weight), nameof(Length), nameof(Width),
                        nameof(Height), nameof(ExpirationDate), nameof(IsFragile),
                        nameof(IsWaterSensitive), nameof(IsValid)
                    };

                    foreach (var property in properties)
                    {
                        OnPropertyChanged(property);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (!IsValid)
                {
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля (*)", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Проверка уникальности SKU для нового товара
                if (!_isEditMode && await _db.CURS_Products.AnyAsync(p => p.SKU == SKU))
                {
                    MessageBox.Show("Товар с таким артикулом уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (_isEditMode)
                {
                    await UpdateProductAsync();
                }
                else
                {
                    await CreateProductAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task CreateProductAsync()
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Создаем товар
                var product = new Product
                {
                    Name = Name.Trim(),
                    SKU = SKU.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    CategoryId = CategoryId,
                    UnitId = UnitId,
                    Weight = Weight,
                    Length = Length,
                    Width = Width,
                    Height = Height,
                    ExpirationDate = ExpirationDate,
                    IsFragile = IsFragile,
                    IsWaterSensitive = IsWaterSensitive
                };

                await _db.CURS_Products.AddAsync(product);
                await _db.SaveChangesAsync();

                // Создаем запись об остатке
                if (InitialQuantity > 0)
                {
                    var selectedLocation = await _db.CURS_Locations
                        .FirstOrDefaultAsync(l => l.IdLocation == LocationId);

                    if (selectedLocation != null)
                    {
                        // Проверяем, помещается ли товар в выбранную локацию
                        if (!CanProductFitInLocation(product, selectedLocation))
                        {
                            throw new Exception($"Товар не помещается в выбранную локацию {selectedLocation.Zone}-{selectedLocation.Shelf}-{selectedLocation.Cell}. " +
                                                $"Проверьте габариты и вес товара.");
                        }

                        var stock = new Stock
                        {
                            ProductId = product.IdProduct,
                            ProductQuantity = InitialQuantity,
                            LocationId = LocationId,
                            MaxProductQuantity = CalculateMaxQuantity(selectedLocation, product)
                        };

                        await _db.CURS_Stocks.AddAsync(stock);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Выбранная локация не найдена");
                    }
                }

                await transaction.CommitAsync();
                MessageBox.Show("Товар успешно создан", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool CanProductFitInLocation(Product product, Location location)
        {
            return product.Length <= location.MaxLength &&
                   product.Width <= location.MaxWidth &&
                   product.Height <= location.MaxHeight &&
                   product.Weight * InitialQuantity <= location.MaxWeight;
        }

        private int CalculateMaxQuantity(Location location, Product product)
        {
            // Рассчитываем максимальное количество на основе доступного места и веса
            var byVolume = (int)((location.MaxLength / product.Length) *
                                (location.MaxWidth / product.Width) *
                                (location.MaxHeight / product.Height));

            var byWeight = (int)(location.MaxWeight / product.Weight);

            return Math.Min(byVolume, byWeight);
        }

        private async Task UpdateProductAsync()
        {
            var product = await _db.CURS_Products
                .FirstOrDefaultAsync(p => p.IdProduct == _productId);

            if (product != null)
            {
                product.Name = Name.Trim();
                product.SKU = SKU.Trim();
                product.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
                product.CategoryId = CategoryId;
                product.UnitId = UnitId;
                product.Weight = Weight;
                product.Length = Length;
                product.Width = Width;
                product.Height = Height;
                product.ExpirationDate = ExpirationDate;
                product.IsFragile = IsFragile;
                product.IsWaterSensitive = IsWaterSensitive;

                await _db.SaveChangesAsync();
                MessageBox.Show("Товар успешно обновлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}