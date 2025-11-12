using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MyWarehouse.Windows
{
    public partial class AdminPanelWindow : Window
    {
        private AppDbContext _db;

        // Коллекции для привязки данных
        public ObservableCollection<Stock> Stocks { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<Unit> Units { get; set; }
        public ObservableCollection<Location> Locations { get; set; }
        public ObservableCollection<Client> Clients { get; set; }
        public ObservableCollection<DeliveryTask> DeliveryTasks { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<Role> Roles { get; set; }
        public ObservableCollection<DeliveryType> DeliveryTypes { get; set; }
        public ObservableCollection<Models.Entities.TaskStatus> TaskStatuses { get; set; }

        public AdminPanelWindow(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            InitializeCollections();
            LoadData();
            DataContext = this;
        }

        private void InitializeCollections()
        {
            Stocks = new ObservableCollection<Stock>();
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<Category>();
            Units = new ObservableCollection<Unit>();
            Locations = new ObservableCollection<Location>();
            Clients = new ObservableCollection<Client>();
            DeliveryTasks = new ObservableCollection<DeliveryTask>();
            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<Role>();
            DeliveryTypes = new ObservableCollection<DeliveryType>();
            TaskStatuses = new ObservableCollection<Models.Entities.TaskStatus>();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка всех данных из базы с отслеживанием изменений
                _db.CURS_Stocks.Load();
                _db.CURS_Products.Load();
                _db.CURS_Categories.Load();
                _db.CURS_Units.Load();
                _db.CURS_Locations.Load();
                _db.CURS_Clients.Load();
                _db.CURS_DeliveryTasks.Load();
                _db.CURS_Users.Load();
                _db.CURS_Roles.Load();
                _db.CURS_DeliveryTypes.Load();
                _db.CURS_TaskStatuses.Load();

                // Очистка коллекций и заполнение новыми данными
                Stocks.Clear();
                foreach (var item in _db.CURS_Stocks.Local)
                    Stocks.Add(item);

                Products.Clear();
                foreach (var item in _db.CURS_Products.Local)
                    Products.Add(item);

                Categories.Clear();
                foreach (var item in _db.CURS_Categories.Local)
                    Categories.Add(item);

                Units.Clear();
                foreach (var item in _db.CURS_Units.Local)
                    Units.Add(item);

                Locations.Clear();
                foreach (var item in _db.CURS_Locations.Local)
                    Locations.Add(item);

                Clients.Clear();
                foreach (var item in _db.CURS_Clients.Local)
                    Clients.Add(item);

                DeliveryTasks.Clear();
                foreach (var item in _db.CURS_DeliveryTasks.Local)
                    DeliveryTasks.Add(item);

                Users.Clear();
                foreach (var item in _db.CURS_Users.Local)
                    Users.Add(item);

                Roles.Clear();
                foreach (var item in _db.CURS_Roles.Local)
                    Roles.Add(item);

                DeliveryTypes.Clear();
                foreach (var item in _db.CURS_DeliveryTypes.Local)
                    DeliveryTypes.Add(item);

                TaskStatuses.Clear();
                foreach (var item in _db.CURS_TaskStatuses.Local)
                    TaskStatuses.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Принудительно завершаем редактирование во всех DataGrid
                StocksDataGrid.CommitEdit();
                ProductsDataGrid.CommitEdit();
                CategoriesDataGrid.CommitEdit();
                UnitsDataGrid.CommitEdit();
                LocationsDataGrid.CommitEdit();
                ClientsDataGrid.CommitEdit();
                DeliveryTasksDataGrid.CommitEdit();
                UsersDataGrid.CommitEdit();
                RolesDataGrid.CommitEdit();
                DeliveryTypesDataGrid.CommitEdit();
                TaskStatusesDataGrid.CommitEdit();

                // Обрабатываем новые записи
                ProcessNewEntries();

                // Сохранение изменений во всех таблицах
                var savedCount = _db.SaveChanges();

                MessageBox.Show($"Успешно сохранено {savedCount} изменений!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем данные после сохранения
                LoadData();
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = GetDetailedErrorMessage(dbEx);
                MessageBox.Show($"Ошибка при сохранении в базу данных: {errorMessage}",
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessNewEntries()
        {
            // Добавляем новые записи в контекст
            foreach (var stock in Stocks.Where(s => s.IdStocks == 0))
            {
                _db.CURS_Stocks.Add(stock);
            }

            foreach (var product in Products.Where(p => p.IdProduct == 0))
            {
                _db.CURS_Products.Add(product);
            }

            foreach (var category in Categories.Where(c => c.IdCategory == 0))
            {
                _db.CURS_Categories.Add(category);
            }

            foreach (var unit in Units.Where(u => u.IdUnit == 0))
            {
                _db.CURS_Units.Add(unit);
            }

            foreach (var location in Locations.Where(l => l.IdLocation == 0))
            {
                _db.CURS_Locations.Add(location);
            }

            foreach (var client in Clients.Where(c => c.IdClient == 0))
            {
                _db.CURS_Clients.Add(client);
            }

            foreach (var deliveryTask in DeliveryTasks.Where(dt => dt.IdDeliveryTask == 0))
            {
                _db.CURS_DeliveryTasks.Add(deliveryTask);
            }

            foreach (var user in Users.Where(u => u.IdUser == 0))
            {
                _db.CURS_Users.Add(user);
            }

            foreach (var role in Roles.Where(r => r.IdRole == 0))
            {
                _db.CURS_Roles.Add(role);
            }

            foreach (var deliveryType in DeliveryTypes.Where(dt => dt.IdDeliveryType == 0))
            {
                _db.CURS_DeliveryTypes.Add(deliveryType);
            }

            foreach (var taskStatus in TaskStatuses.Where(ts => ts.IdTaskStatus == 0))
            {
                _db.CURS_TaskStatuses.Add(taskStatus);
            }
        }

        private string GetDetailedErrorMessage(DbUpdateException dbEx)
        {
            var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

            // Если это исключение от SQL Server, можно получить более детальную информацию
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                errorMessage = $"SQL Error {sqlEx.Number}: {sqlEx.Message}";
            }

            return errorMessage;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Отменяем все незавершенные редактирования
                StocksDataGrid.CancelEdit();
                ProductsDataGrid.CancelEdit();
                CategoriesDataGrid.CancelEdit();
                UnitsDataGrid.CancelEdit();
                LocationsDataGrid.CancelEdit();
                ClientsDataGrid.CancelEdit();
                DeliveryTasksDataGrid.CancelEdit();
                UsersDataGrid.CancelEdit();
                RolesDataGrid.CancelEdit();
                DeliveryTypesDataGrid.CancelEdit();
                TaskStatusesDataGrid.CancelEdit();

                // Также отменяем редактирование для всех строк, которые могут быть в режиме добавления
                StocksDataGrid.CancelEdit(DataGridEditingUnit.Row);
                ProductsDataGrid.CancelEdit(DataGridEditingUnit.Row);
                CategoriesDataGrid.CancelEdit(DataGridEditingUnit.Row);
                UnitsDataGrid.CancelEdit(DataGridEditingUnit.Row);
                LocationsDataGrid.CancelEdit(DataGridEditingUnit.Row);
                ClientsDataGrid.CancelEdit(DataGridEditingUnit.Row);
                DeliveryTasksDataGrid.CancelEdit(DataGridEditingUnit.Row);
                UsersDataGrid.CancelEdit(DataGridEditingUnit.Row);
                RolesDataGrid.CancelEdit(DataGridEditingUnit.Row);
                DeliveryTypesDataGrid.CancelEdit(DataGridEditingUnit.Row);
                TaskStatusesDataGrid.CancelEdit(DataGridEditingUnit.Row);

                LoadData();

                MessageBox.Show("Данные успешно обновлены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}