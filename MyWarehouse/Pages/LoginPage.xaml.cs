using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MyWarehouse.Pages
{
    public partial class LoginPage : Page
    {
        private readonly IAuthService _authService;

        public LoginPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
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

            var user = await Task.Run(() => _authService.AuthenticateAsync(login, password));

            if (user == null)
            {
                ShowError("Неверный логин или пароль.");
                PasswordBox.Clear();
                PasswordBox.Focus();
                LoginButton.IsEnabled = true;
                return;
            }

            UserSession.CurrentUser = user;
            NavigationService.Navigate(App.ServiceProvider.GetService<HomePage>());
        }

        private void ShowError(string text)
        {
            ErrorTextBlock.Text = text;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}