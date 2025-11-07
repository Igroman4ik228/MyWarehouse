using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using System.Collections.ObjectModel;
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
            MainFrame.Navigate(new ProductPage());
        }

        private void AdminPanel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыть админ панель (только для админов).", "Админ панель", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TaskPage());
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ShowProductsView();
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыть список клиентов.", "Клиенты", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Stocks_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыть склады/запасы.", "Склады", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
