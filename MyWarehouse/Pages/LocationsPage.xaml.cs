using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyWarehouse.Pages
{
    public partial class LocationsPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Location> _locations;
        private AppDbContext _db;
        private bool _isReadOnlyMode;

        public ObservableCollection<Location> Locations
        {
            get => _locations;
            set
            {
                _locations = value;
                OnPropertyChanged(nameof(Locations));
            }
        }

        public bool IsReadOnlyMode
        {
            get => _isReadOnlyMode;
            set
            {
                _isReadOnlyMode = value;
                OnPropertyChanged(nameof(IsReadOnlyMode));
                UpdateUI();
            }
        }

        public LocationsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем права доступа
                IsReadOnlyMode = UserSession.CurrentUser.RoleId == (int)RoleType.Courier;

                await LoadLocations();

                StatusTextBlock.Text = "Данные загружены";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка загрузки данных";
            }
        }

        private async Task LoadLocations()
        {
            Locations = new ObservableCollection<Location>(
                await _db.CURS_Locations
                    .AsNoTracking()
                    .OrderBy(l => l.Zone)
                    .ThenBy(l => l.Shelf)
                    .ThenBy(l => l.Cell)
                    .ToListAsync()
            );
        }

        private void UpdateUI()
        {
            if (IsReadOnlyMode)
            {
                AddButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Collapsed;
                LocationsDataGrid.IsReadOnly = true;
            }
            else
            {
                AddButton.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Visible;
                LocationsDataGrid.IsReadOnly = false;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newLocation = new Location
                {
                    Zone = "Новая зона",
                    Shelf = "A",
                    Cell = "1",
                    MaxLength = 0,
                    MaxWidth = 0,
                    MaxHeight = 0,
                    MaxWeight = 0
                };

                Locations.Add(newLocation);

                // Переходим к редактированию новой строки
                var newItemIndex = Locations.Count - 1;
                LocationsDataGrid.SelectedIndex = newItemIndex;
                LocationsDataGrid.ScrollIntoView(Locations[newItemIndex]);
                LocationsDataGrid.BeginEdit();

                StatusTextBlock.Text = "Добавлена новая локация";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (LocationsDataGrid.SelectedItem is Location location)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить локацию '{location.DisplayName}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Проверяем, есть ли связанные запасы
                        var hasStocks = _db.CURS_Stocks.Any(s => s.LocationId == location.IdLocation);
                        if (hasStocks)
                        {
                            MessageBox.Show(
                                "Невозможно удалить локацию, так как с ней связаны запасы товаров.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Проверяем, есть ли связанные задачи доставки
                        var hasDeliveryTasks = _db.CURS_DeliveryTasks.Any(dt =>
                            dt.FromLocationId == location.IdLocation || dt.ToLocationId == location.IdLocation);
                        if (hasDeliveryTasks)
                        {
                            MessageBox.Show(
                                "Невозможно удалить локацию, так как с ней связаны задачи доставки.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (location.IdLocation > 0)
                        {
                            var entity = _db.CURS_Locations.Find(location.IdLocation);
                            if (entity != null)
                            {
                                _db.CURS_Locations.Remove(entity);
                            }
                        }

                        Locations.Remove(location);
                        StatusTextBlock.Text = "Локация удалена";
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show(
                            "Невозможно удалить локацию, так как она используется в системе.",
                            "Ошибка удаления",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveRowButton_Click(object sender, RoutedEventArgs e)
        {
            LocationsDataGrid.CommitEdit();
            StatusTextBlock.Text = "Изменения в строке сохранены";
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            LocationsDataGrid.CancelEdit();

            // Если это новая запись, удаляем ее
            var currentItem = LocationsDataGrid.CurrentItem as Location;
            if (currentItem?.IdLocation == 0)
            {
                Locations.Remove(currentItem);
            }

            StatusTextBlock.Text = "Редактирование отменено";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LocationsDataGrid.CommitEdit();

                // Валидация данных перед сохранением
                foreach (var location in Locations)
                {
                    if (string.IsNullOrWhiteSpace(location.Zone) ||
                        string.IsNullOrWhiteSpace(location.Shelf) ||
                        string.IsNullOrWhiteSpace(location.Cell))
                    {
                        MessageBox.Show("Все поля (Зона, Полка, Ячейка) должны быть заполнены.", "Ошибка валидации",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (location.MaxLength < 0 || location.MaxWidth < 0 ||
                        location.MaxHeight < 0 || location.MaxWeight < 0)
                    {
                        MessageBox.Show("Все размеры и вес должны быть неотрицательными.", "Ошибка валидации",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Сохраняем изменения в базе данных
                foreach (var location in Locations)
                {
                    if (location.IdLocation == 0)
                    {
                        // Новая локация
                        _db.CURS_Locations.Add(location);
                    }
                    else
                    {
                        // Существующая локация
                        var entity = _db.CURS_Locations.Find(location.IdLocation);
                        if (entity != null)
                        {
                            _db.Entry(entity).CurrentValues.SetValues(location);
                        }
                    }
                }

                await _db.SaveChangesAsync();

                // Обновляем IDs для новых локаций
                await LoadLocations();

                StatusTextBlock.Text = "Все изменения сохранены";
                MessageBox.Show("Изменения успешно сохранены.", "Сохранение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка сохранения в базу данных: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка сохранения";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка сохранения";
            }
        }

        private void LocationsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Валидация числовых полей
            if (e.EditAction == DataGridEditAction.Commit && e.EditingElement is TextBox textBox)
            {
                var column = e.Column as DataGridTemplateColumn;
                var location = e.Row.Item as Location;

                if (column?.Header?.ToString()?.StartsWith("Макс.") == true)
                {
                    if (!decimal.TryParse(textBox.Text, out decimal value) || value < 0)
                    {
                        MessageBox.Show("Введите корректное неотрицательное числовое значение.", "Ошибка валидации",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _locations?.Clear();
        }
    }
}