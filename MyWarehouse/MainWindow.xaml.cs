using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Pages;
using System.Windows;

namespace MyWarehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(App.ServiceProvider.GetService<LoginPage>());
        }
    }
}