using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using MyWarehouse.Windows;
using System.Windows;
using System.Windows.Controls;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly AppDbContext _db;
        private AdminPanelWindow? _adminPanelWindow;

        public HomePage(AppDbContext db)
        {
            _db = db;
            InitializeComponent();

            // Устанавливаем данные пользователя
            UserFirstNameTextBlock.Text = UserSession.CurrentUser.FirstName;
            if (UserSession.IsAdmin)
            {
                AdminPanelButton.Visibility = Visibility.Visible;
            }
            LoadUserRole();

            ShowProductsView();
        }

        private async void LoadUserRole()
        {
            try
            {
                var role = await _db.CURS_Roles.FirstOrDefaultAsync(r => r.IdRole == UserSession.CurrentUser.RoleId) ?? throw new Exception("Неизвестная роль");
                UserRoleTextBlock.Text = role.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить данные роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowProductsView()
        {
            MainFrame.Navigate(App.ServiceProvider.GetService<ProductsPage>());
        }

        private void AdminPanel_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin)
            {
                // Создаем окно только если его нет или оно закрыто
                if (_adminPanelWindow == null || !_adminPanelWindow.IsLoaded)
                {
                    _adminPanelWindow = App.ServiceProvider.GetService<AdminPanelWindow>();
                    _adminPanelWindow.Closed += (s, args) => { _adminPanelWindow = null; };
                    _adminPanelWindow.Show();
                }
                else
                {
                    // Если окно уже существует, активируем его
                    _adminPanelWindow.Activate();
                    if (_adminPanelWindow.WindowState == WindowState.Minimized)
                        _adminPanelWindow.WindowState = WindowState.Normal;
                }
            }
            else
            {
                MessageBox.Show("Недостаточно прав для доступа к админ панели.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(App.ServiceProvider.GetService<TasksPage>());
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ShowProductsView();
        }

        private void Locations_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(App.ServiceProvider.GetService<LocationsPage>());
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(App.ServiceProvider.GetService<ClientsPage>());
        }
    }
}