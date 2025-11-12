using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly AppDbContext _db;
        private readonly ITaskProcessingService _taskProcessor;
        public ObservableCollection<TaskViewModel> Tasks { get; } = [];

        public TasksPage(AppDbContext db, ITaskProcessingService taskProcessor)
        {
            _db = db;
            _taskProcessor = taskProcessor;
            InitializeComponent();

            if (UserSession.IsAdmin) { 
                AddButton.Visibility = Visibility.Visible;
            }

            DataContext = this;
        }

        private async Task LoadActiveTasks()
        {
            try
            {
                var activeStatuses = new[] {
                    (int)DeliveryTaskStatus.NotTaken,
                    (int)DeliveryTaskStatus.InProgress
                };

                var tasks = await _db.CURS_DeliveryTasks
                    .Include(t => t.Product)
                    .Include(t => t.Client)
                    .Include(t => t.DeliveryType)
                    .Include(t => t.TaskStatus)
                    .Include(t => t.ExecutorUser)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Where(t => activeStatuses.Contains(t.TaskStatusId))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                Tasks.Clear();

                foreach (var task in tasks)
                {
                    Tasks.Add(CreateTaskViewModel(task));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static TaskViewModel CreateTaskViewModel(DeliveryTask task)
        {
            var vm = new TaskViewModel
            {
                Id = task.IdDeliveryTask,
                ProductName = task.Product.Name,
                ClientName = task.Client.Name,
                DeliveryTypeName = task.DeliveryType.Name,
                Quantity = task.ProductQuantity,
                CreatedAt = task.CreatedAt,
                TaskStatusId = task.TaskStatusId,
                TaskStatusName = task.TaskStatus.Name,
                ExecutorUserId = task.ExecutorUserId,
                ExecutorName = task.ExecutorUser?.FirstName,
                FromLocationName = FormatLocation(task.FromLocation),
                ToLocationName = FormatLocation(task.ToLocation)
            };

            // Логика действий
            if (task.TaskStatusId == (int)DeliveryTaskStatus.NotTaken)
            {
                vm.CanAction = true;
                vm.ActionText = "Взять";
                vm.ActionBackground = new SolidColorBrush(Colors.Green);
            }
            else if (task.TaskStatusId == (int)DeliveryTaskStatus.InProgress &&
                     task.ExecutorUserId == UserSession.CurrentUser.IdUser)
            {
                vm.CanAction = true;
                vm.ActionText = "Завершить";
                vm.ActionBackground = new SolidColorBrush(Colors.Orange);
            }

            // Логика отмены
            vm.CanCancel = (UserSession.IsAdmin || UserSession.IsManager)
                && task.TaskStatusId != (int)DeliveryTaskStatus.Completed
                && task.TaskStatusId != (int)DeliveryTaskStatus.Rejected;

            return vm;
        }

        private static string? FormatLocation(Location? location)
        {
            return location != null ? $"{location.Zone} {location.Shelf} {location.Cell}" : null;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadActiveTasks();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Tasks.Clear();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyPage = App.ServiceProvider.GetService<TasksHistoryPage>();
            NavigationService?.Navigate(historyPage);
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditTaskWindow(_db);
            if (window.ShowDialog() == true)
            {
                try
                {
                    var newTask = new DeliveryTask
                    {
                        ProductId = window.SelectedProductId,
                        ClientId = window.SelectedClientId,
                        DeliveryTypeId = window.SelectedDeliveryTypeId,
                        ProductQuantity = window.Quantity,
                        CreatedUserId = UserSession.CurrentUser.IdUser,
                        TaskStatusId = (int)DeliveryTaskStatus.NotTaken,
                        FromLocationId = window.SelectedFromLocationId,
                        ToLocationId = window.SelectedToLocationId,
                        CreatedAt = DateTime.Now
                    };

                    _db.CURS_DeliveryTasks.Add(newTask);
                    await _db.SaveChangesAsync();
                    await LoadActiveTasks();

                    MessageBox.Show("Задача успешно создана!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания задачи: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: TaskViewModel viewModel })
            {
                try
                {
                    var task = await _db.CURS_DeliveryTasks.FindAsync(viewModel.Id);
                    if (task == null) return;

                    if (task.TaskStatusId == (int)DeliveryTaskStatus.NotTaken)
                    {
                        // Берем задачу в работу
                        task.TaskStatusId = (int)DeliveryTaskStatus.InProgress;
                        task.ExecutorUserId = UserSession.CurrentUser.IdUser;
                        await _db.SaveChangesAsync();
                    }
                    else if (task.TaskStatusId == (int)DeliveryTaskStatus.InProgress &&
                             task.ExecutorUserId == UserSession.CurrentUser.IdUser)
                    {
                        // Завершаем задачу с обработкой бизнес-логики
                        var success = await _taskProcessor.CompleteTaskAsync(task.IdDeliveryTask);
                        if (!success)
                        {
                            MessageBox.Show("Не удалось завершить задачу", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    await LoadActiveTasks();
                    MessageBox.Show("Действие выполнено успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка выполнения действия: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: TaskViewModel viewModel })
            {
                var result = MessageBox.Show("Вы действительно хотите отменить задачу?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var success = await _taskProcessor.CancelTaskAsync(viewModel.Id);
                        if (success)
                        {
                            await LoadActiveTasks();
                            MessageBox.Show("Задача отменена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка отмены задачи: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
        public string OperationTypeName { get; set; } = string.Empty;
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
    }
}