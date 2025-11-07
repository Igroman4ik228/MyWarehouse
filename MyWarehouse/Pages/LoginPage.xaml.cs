using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
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
        private readonly AppDbContext db = new();

        public LoginPage()
        {
            InitializeComponent();
            LoginBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите логин и пароль.");
                return;
            }


            var user = db.CURS_Users.FirstOrDefault(u => u.Login == login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ShowError("Неверный логин или пароль.");
                PasswordBox.Clear();
                PasswordBox.Focus();
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
