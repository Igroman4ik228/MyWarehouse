using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyWarehouse.Pages
{
    public class TaskViewModel
    {
        public DeliveryTask Model { get; set; }
        public int Id => Model.IdDeliveryTask;
        public string ProductName => Model.Product?.Name ?? "";
        public string ClientName => Model.Client?.Name ?? "";
        public int ProductQuantity => Model.ProductQuantity;
        public string StatusName => Model.TaskStatus?.Name ?? Model.TaskStatusId.ToString();
        public string CreatedAtString => Model.CreatedAt.ToString("");

        public Brush StatusColor
        {
            get
            {
                return Model.TaskStatusId switch
                {
                    (int)DeliveryTaskStatus.NotTaken => Brushes.Gray,
                    (int)DeliveryTaskStatus.InProgress => Brushes.Orange,
                    (int)DeliveryTaskStatus.Completed => Brushes.Green,
                    (int)DeliveryTaskStatus.Rejected => Brushes.Red,
                    _ => Brushes.Gray,
                };
            }
        }

        // For couriers: show action button (Take / Complete). For others: hide
        public bool ActionVisible { get; set; }
        public string ActionLabel => Model.TaskStatusId == (int)DeliveryTaskStatus.NotTaken ? "Взять" : "Завершить";
    }

    /// <summary>
    /// Interaction logic for TaskPage.xaml
    /// </summary>
    public partial class TaskPage : Page
    {
        private readonly AppDbContext db = new();

        public TaskPage()
        {
            InitializeComponent();
            DataContext = this;

            LoadTasks();
        }

        private void LoadTasks()
        {
            
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        //private TaskViewModel GetTaskVMFromSender(object sender)
        //{
        //    if (sender is Button btn && btn.DataContext is TaskViewModel tvm)
        //        return tvm;
        //    return null;
        //}

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
