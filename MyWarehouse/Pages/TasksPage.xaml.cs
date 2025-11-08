using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyWarehouse.Pages
{

    /// <summary>
    /// Interaction logic for TaskPage.xaml
    /// </summary>
    public partial class TasksPage : Page
    {
        private readonly AppDbContext db = new();

        public TasksPage()
        {
            InitializeComponent();
        }

        private void LoadTasks()
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
