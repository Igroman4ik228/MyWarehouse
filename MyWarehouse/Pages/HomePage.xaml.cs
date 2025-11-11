using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Services;
using System.Windows;
using System.Windows.Controls;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public bool IsAdmin = UserSession.IsAdmin;

        public HomePage()
        {
            InitializeComponent();
            UserFirstNameTextBlock.Text = UserSession.CurrentUser.FirstName;

            ShowProductsView();
        }

        private void ShowProductsView()
        {
            MainFrame.Navigate(App.ServiceProvider.GetService<ProductsPage>());
        }

        private void AdminPanel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыть админ панель (только для админов).", "Админ панель", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void MainFrame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            MessageBox.Show("Загрузка завершена.", "Навигация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
