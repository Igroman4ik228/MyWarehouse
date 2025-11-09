using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyWarehouse.Pages
{
    public partial class ClientsPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Client> _clients;
        private AppDbContext _db;
        private bool _isReadOnlyMode;

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
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

        public ClientsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadClients();

                // Проверяем права доступа
                IsReadOnlyMode = UserSession.CurrentUser.RoleId == (int)RoleType.Courier;

                StatusTextBlock.Text = "Данные загружены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка загрузки данных";
            }
        }

        private async Task LoadClients()
        {
            Clients = new ObservableCollection<Client>(
                await _db.CURS_Clients
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync()
                 );
        }

        private void UpdateUI()
        {
            if (IsReadOnlyMode)
            {
                AddButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Collapsed;
                ClientsDataGrid.IsReadOnly = true;
            }
            else
            {
                AddButton.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Visible;
                ClientsDataGrid.IsReadOnly = false;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newClient = new Client
                {
                    Name = "Новый клиент",
                    Email = "",
                    Phone = "",
                    Description = ""
                };

                Clients.Add(newClient);

                // Переходим к редактированию новой строки
                var newItemIndex = Clients.Count - 1;
                ClientsDataGrid.SelectedIndex = newItemIndex;
                ClientsDataGrid.ScrollIntoView(Clients[newItemIndex]);
                ClientsDataGrid.BeginEdit();

                StatusTextBlock.Text = "Добавлен новый клиент";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client client)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить клиента '{client.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Проверяем, есть ли связанные задачи
                        var hasTasks = _db.CURS_DeliveryTasks.Any(t => t.ClientId == client.IdClient);
                        if (hasTasks)
                        {
                            MessageBox.Show(
                                "Невозможно удалить клиента, так как с ним связаны задачи доставки.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (client.IdClient > 0)
                        {
                            var entity = _db.CURS_Clients.Find(client.IdClient);
                            if (entity != null)
                            {
                                _db.CURS_Clients.Remove(entity);
                            }
                        }

                        Clients.Remove(client);
                        StatusTextBlock.Text = "Клиент удален";
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show(
                            "Невозможно удалить клиента, так как он используется в системе.",
                            "Ошибка удаления",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveRowButton_Click(object sender, RoutedEventArgs e)
        {
            ClientsDataGrid.CommitEdit();
            StatusTextBlock.Text = "Изменения в строке сохранены";
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ClientsDataGrid.CancelEdit();

            // Если это новая запись, удаляем ее
            var currentItem = ClientsDataGrid.CurrentItem as Client;
            if (currentItem?.IdClient == 0)
            {
                Clients.Remove(currentItem);
            }

            StatusTextBlock.Text = "Редактирование отменено";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClientsDataGrid.CommitEdit();

                // Сохраняем изменения в базе данных
                foreach (var client in Clients)
                {
                    if (client.IdClient == 0)
                    {
                        // Новый клиент
                        _db.CURS_Clients.Add(client);
                    }
                    else
                    {
                        // Существующий клиент
                        var entity = _db.CURS_Clients.Find(client.IdClient);
                        if (entity != null)
                        {
                            _db.Entry(entity).CurrentValues.SetValues(client);
                        }
                    }
                }

                _db.SaveChanges();

                // Обновляем IDs для новых клиентов
                await LoadClients();

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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка сохранения";
            }
        }

        private void ClientsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Валидация данных
            if (e.EditAction == DataGridEditAction.Commit && e.EditingElement is TextBox textBox)
            {
                var column = e.Column as DataGridTemplateColumn;
                var client = e.Row.Item as Client;

                if (column?.Header?.ToString() == "Email" && !string.IsNullOrEmpty(textBox.Text))
                {
                    if (!IsValidEmail(textBox.Text))
                    {
                        MessageBox.Show("Введите корректный email адрес.", "Ошибка валидации",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                    }
                }
                else if (column?.Header?.ToString() == "Телефон" && !string.IsNullOrEmpty(textBox.Text))
                {
                    if (!IsValidPhone(textBox.Text))
                    {
                        MessageBox.Show("Введите корректный номер телефона.", "Ошибка валидации",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                    }
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Простая валидация телефона - можно улучшить
            return phone.Length >= 5 && phone.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '(' || c == ')');
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Clients.Clear();
        }
    }

    // Converter для отображения курсивом пустых описаний
    public class NullToFontStyleConverter : IValueConverter
    {
        public static NullToFontStyleConverter Instance { get; } = new NullToFontStyleConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? FontStyles.Italic : FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}