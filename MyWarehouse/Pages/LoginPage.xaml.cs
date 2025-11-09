using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MyWarehouse.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly AppDbContext _db = new();

        public LoginPage()
        {
            InitializeComponent();
            LoginBox.Focus();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = false;

            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите логин и пароль.");
                LoginButton.IsEnabled = true;
                return;
            }


            var user = await Task.Run(() => _db.CURS_Users.FirstOrDefaultAsync(u => u.Login == login));

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) 
            {
                ShowError("Неверный логин или пароль.");
                PasswordBox.Clear();
                PasswordBox.Focus();
                LoginButton.IsEnabled = true;
                return;
            }

            UserSession.CurrentUser = user;

            NavigationService.Navigate(new HomePage());
        }

        private void ShowError(string text)
        {
            ErrorTextBlock.Text = text;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}
