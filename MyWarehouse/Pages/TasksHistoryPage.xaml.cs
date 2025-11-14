using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyWarehouse.Pages
{
    public partial class TasksHistoryPage : Page
    {
        private readonly AppDbContext _db;
        public ObservableCollection<HistoryTaskViewModel> Tasks { get; } = [];
        public ObservableCollection<HistoryTaskViewModel> FilteredTasks { get; } = [];

        public TasksHistoryPage(AppDbContext db)
        {
            _db = db;
            InitializeComponent();
            DataContext = this;
        }

        private async Task LoadHistoryTasks()
        {
            try
            {
                var completedStatuses = new[] { (int)DeliveryTaskStatus.Completed, (int)DeliveryTaskStatus.Rejected };

                var tasks = await _db.CURS_DeliveryTasks
                    .Include(t => t.Product)
                    .Include(t => t.Client)
                    .Include(t => t.DeliveryType)
                    .Include(t => t.TaskStatus)
                    .Include(t => t.ExecutorUser)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Where(t => completedStatuses.Contains(t.TaskStatusId))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                Tasks.Clear();

                foreach (var task in tasks)
                {
                    Tasks.Add(CreateHistoryTaskViewModel(task));
                }

                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearchFilter()
        {
            var searchText = SearchTextBox.Text?.Trim().ToLower();

            FilteredTasks.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? Tasks
                : Tasks.Where(t =>
                    t.ProductName.ToLower().Contains(searchText) ||
                    t.ClientName.ToLower().Contains(searchText) ||
                    (t.ExecutorName?.ToLower().Contains(searchText) ?? false) ||
                    t.DeliveryTypeName.ToLower().Contains(searchText) ||
                    t.TaskStatusName.ToLower().Contains(searchText) ||
                    (t.FromLocationName?.ToLower().Contains(searchText) ?? false) ||
                    (t.ToLocationName?.ToLower().Contains(searchText) ?? false)
                );

            foreach (var task in filtered)
            {
                FilteredTasks.Add(task);
            }
        }

        private static HistoryTaskViewModel CreateHistoryTaskViewModel(DeliveryTask task)
        {
            var statusColor = task.TaskStatusId == (int)DeliveryTaskStatus.Completed
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);

            return new HistoryTaskViewModel
            {
                Id = task.IdDeliveryTask,
                ProductName = task.Product?.Name ?? "-",
                ClientName = task.Client?.Name ?? "-",
                DeliveryTypeName = task.DeliveryType?.Name ?? "-",
                Quantity = task.ProductQuantity,
                CreatedAt = task.CreatedAt,
                TaskStatusId = task.TaskStatusId,
                TaskStatusName = task.TaskStatus?.Name ?? "-",
                ExecutorUserId = task.ExecutorUserId,
                ExecutorName = task.ExecutorUser?.FirstName,
                FromLocationName = FormatLocation(task.FromLocation),
                ToLocationName = FormatLocation(task.ToLocation),
                StatusColor = statusColor,
                CompletedAt = GetCompletedDate(task)
            };
        }

        private static string? FormatLocation(Location? location)
        {
            return location != null ? $"{location.Zone} {location.Shelf} {location.Cell}" : null;
        }

        private static DateTime? GetCompletedDate(DeliveryTask task)
        {
            // Здесь можно добавить логику для получения даты завершения
            // Пока используем CreatedAt как пример
            return task.TaskStatusId == (int)DeliveryTaskStatus.Completed ? task.CreatedAt.AddHours(2) : null;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadHistoryTasks();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Tasks.Clear();
            FilteredTasks.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
    }

    public class HistoryTaskViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string DeliveryTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool HasCompletedAt => CompletedAt.HasValue;
        public int TaskStatusId { get; set; }
        public string TaskStatusName { get; set; } = string.Empty;
        public int? ExecutorUserId { get; set; }
        public string? ExecutorName { get; set; }
        public bool HasExecutor => !string.IsNullOrEmpty(ExecutorName);
        public string? FromLocationName { get; set; }
        public string? ToLocationName { get; set; }
        public bool HasFromLocation => !string.IsNullOrEmpty(FromLocationName);
        public bool HasToLocation => !string.IsNullOrEmpty(ToLocationName);
        public SolidColorBrush StatusColor { get; set; } = new(Colors.Gray);
    }
}