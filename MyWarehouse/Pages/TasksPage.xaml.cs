using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Services;
using MyWarehouse.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyWarehouse.Pages
{
    public partial class TasksPage : Page
    {
        private readonly AppDbContext db = new();

        public ObservableCollection<TaskViewModel> Tasks { get; } = [];

        public TasksPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async Task LoadTasks()
        {
            var tasks = await db.CURS_DeliveryTasks
                .Include(t => t.Product)
                .Include(t => t.Client)
                .Include(t => t.DeliveryType)
                .Include(t => t.TaskStatus)
                .Include(t => t.ExecutorUser)
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .ToListAsync();

            Tasks.Clear();

            foreach (var t in tasks)
            {
                var vm = new TaskViewModel
                {
                    Id = t.IdDeliveryTask,
                    ProductName = t.Product?.Name ?? "-",
                    ClientName = t.Client?.Name ?? "-",
                    DeliveryTypeName = t.DeliveryType?.Name ?? "-",
                    Quantity = t.ProductQuantity,
                    CreatedAt = t.CreatedAt,
                    TaskStatusId = t.TaskStatusId,
                    TaskStatusName = t.TaskStatus?.Name ?? "-",
                    ExecutorUserId = t.ExecutorUserId,
                    ExecutorName = t.ExecutorUser?.FirstName,
                    FromLocationName = t.FromLocation != null ? $"{t.FromLocation?.Zone} {t.FromLocation?.Shelf} {t.FromLocation?.Cell}" : null,
                    ToLocationName = t.ToLocation != null ? $"{t.ToLocation?.Zone} {t.ToLocation?.Shelf} {t.ToLocation?.Cell}" : null,
                    DeliveryTaskEntity = t,
                    CanAction = false,
                    ActionBackground = new SolidColorBrush(Colors.Gray),
                    ActionText = t.TaskStatus?.Name ?? "-"
                };

                // Логика действия
                if (t.TaskStatusId == (int)DeliveryTaskStatus.NotTaken)
                {
                    vm.CanAction = true;
                    vm.ActionText = "Взять";
                    vm.ActionBackground = new SolidColorBrush(Colors.Green);
                }

                if (t.TaskStatusId == (int)DeliveryTaskStatus.InProgress && t.ExecutorUserId == UserSession.CurrentUser.IdUser)
                {
                    vm.CanAction = true;
                    vm.ActionText = "Завершить";
                    vm.ActionBackground = new SolidColorBrush(Colors.Orange);
                }

                if (t.TaskStatusId == (int)DeliveryTaskStatus.Completed)
                    vm.CanAction = false;

                if (t.TaskStatusId == (int)DeliveryTaskStatus.InProgress && t.ExecutorUserId.HasValue && t.ExecutorUserId != UserSession.CurrentUser.IdUser)
                    vm.CanAction = false;

                // Кнопка "Отменить" — только для админа или менеджера
                vm.CanCancel =
                    (UserSession.IsAdmin || UserSession.IsManager)
                    && t.TaskStatusId != (int)DeliveryTaskStatus.Completed
                    && t.TaskStatusId != (int)DeliveryTaskStatus.Rejected;

                Tasks.Add(vm);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTasks();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Tasks.Clear();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddEditTaskWindow();
            if (w.ShowDialog() == true)
            {
                var newTask = new DeliveryTask
                {
                    ProductId = w.SelectedProductId,
                    ClientId = w.SelectedClientId,
                    DeliveryTypeId = w.SelectedDeliveryTypeId,
                    ProductQuantity = w.Quantity,
                    CreatedUserId = UserSession.CurrentUser.IdUser,
                    TaskStatusId = (int)DeliveryTaskStatus.NotTaken,
                    FromLocationId = w.SelectedFromLocationId,
                    ToLocationId = w.SelectedToLocationId
                };

                db.CURS_DeliveryTasks.Add(newTask);
                await db.SaveChangesAsync();
                await LoadTasks();
            }
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TaskViewModel vm)
            {
                var task = await db.CURS_DeliveryTasks.FindAsync(vm.Id);
                if (task == null) return;

                if (task.TaskStatusId == (int)DeliveryTaskStatus.NotTaken)
                {
                    task.TaskStatusId = (int)DeliveryTaskStatus.InProgress;
                    task.ExecutorUserId = UserSession.CurrentUser.IdUser;
                }
                else if (task.TaskStatusId == (int)DeliveryTaskStatus.InProgress && task.ExecutorUserId == UserSession.CurrentUser.IdUser)
                {
                    task.TaskStatusId = (int)DeliveryTaskStatus.Completed;
                }

                db.Update(task);
                await db.SaveChangesAsync();
                await LoadTasks();
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TaskViewModel vm)
            {
                if (MessageBox.Show("Вы действительно хотите отменить задачу?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var task = await db.CURS_DeliveryTasks.FindAsync(vm.Id);
                    if (task == null) return;

                    task.TaskStatusId = (int)DeliveryTaskStatus.Rejected;
                    db.Update(task);
                    await db.SaveChangesAsync();
                    await LoadTasks();
                }
            }
        }
    }

    public class TaskViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string DeliveryTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskStatusId { get; set; }
        public string TaskStatusName { get; set; } = string.Empty;
        public int? ExecutorUserId { get; set; }
        public string? ExecutorName { get; set; }
        public bool HasExecutor => !string.IsNullOrEmpty(ExecutorName);

        public string? FromLocationName { get; set; }
        public string? ToLocationName { get; set; }
        public bool HasFromLocation => !string.IsNullOrEmpty(FromLocationName);
        public bool HasToLocation => !string.IsNullOrEmpty(ToLocationName);

        public bool CanAction { get; set; }
        public bool CanCancel { get; set; }

        public string ActionText { get; set; } = string.Empty;
        public SolidColorBrush ActionBackground { get; set; } = new(Colors.Gray);

        public DeliveryTask? DeliveryTaskEntity { get; set; }
    }
}
