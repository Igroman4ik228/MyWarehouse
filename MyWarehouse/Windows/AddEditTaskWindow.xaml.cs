using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using System.Linq;
using System.Windows;

namespace MyWarehouse.Windows
{
    public partial class AddEditTaskWindow : Window
    {
        private readonly AppDbContext _db;

        public int SelectedProductId { get; private set; }
        public int SelectedClientId { get; private set; }
        public int SelectedDeliveryTypeId { get; private set; }
        public int? SelectedFromLocationId { get; private set; }
        public int? SelectedToLocationId { get; private set; }
        public int Quantity { get; private set; }

        public AddEditTaskWindow(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            InitializeDataSources();
        }

        public AddEditTaskWindow(DeliveryTask task, AppDbContext db) : this(db)
        {
            PopulateFieldsFromTask(task);
        }

        private void InitializeDataSources()
        {
            ProductCombo.ItemsSource = _db.CURS_Products.ToList();
            ClientCombo.ItemsSource = _db.CURS_Clients.ToList();
            DeliveryTypeCombo.ItemsSource = _db.CURS_DeliveryTypes.ToList();
            FromLocationCombo.ItemsSource = _db.CURS_Locations.ToList();
            ToLocationCombo.ItemsSource = _db.CURS_Locations.ToList();
        }

        private void PopulateFieldsFromTask(DeliveryTask task)
        {
            ProductCombo.SelectedValue = task.ProductId;
            ClientCombo.SelectedValue = task.ClientId;
            DeliveryTypeCombo.SelectedValue = task.DeliveryTypeId;
            QuantityTextBox.Text = task.ProductQuantity.ToString();
            FromLocationCombo.SelectedValue = task.FromLocationId;
            ToLocationCombo.SelectedValue = task.ToLocationId;

            // Обновляем видимость полей в соответствии с типом доставки
            UpdateLocationVisibility((int)task.DeliveryTypeId);
        }

        private void DeliveryTypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DeliveryTypeCombo.SelectedValue is int typeId)
            {
                UpdateLocationVisibility(typeId);
            }
        }

        private void UpdateLocationVisibility(int deliveryTypeId)
        {
            // Сначала скрываем все и очищаем значения
            FromLocationStack.Visibility = Visibility.Collapsed;
            ToLocationStack.Visibility = Visibility.Collapsed;

            // Очищаем значения скрытых полей
            if (FromLocationStack.Visibility == Visibility.Collapsed)
                FromLocationCombo.SelectedValue = null;
            if (ToLocationStack.Visibility == Visibility.Collapsed)
                ToLocationCombo.SelectedValue = null;

            // Показываем нужные поля в зависимости от типа доставки
            switch (deliveryTypeId)
            {
                case 1: // Внутренняя — обе локации
                    FromLocationStack.Visibility = Visibility.Visible;
                    ToLocationStack.Visibility = Visibility.Visible;
                    break;
                case 2: // Внешняя поставка — только "куда"
                    ToLocationStack.Visibility = Visibility.Visible;
                    break;
                case 3: // Выгрузка — только "откуда"
                    FromLocationStack.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            SetPropertiesFromInput();
            DialogResult = true;
            Close();
        }

        private bool ValidateInput()
        {
            if (!ValidateRequiredSelections())
                return false;

            if (!ValidateQuantity())
                return false;

            if (!ValidateLocations())
                return false;

            return true;
        }

        private bool ValidateRequiredSelections()
        {
            if (ProductCombo.SelectedValue == null ||
                ClientCombo.SelectedValue == null ||
                DeliveryTypeCombo.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукт, клиента и тип доставки.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateQuantity()
        {
            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateLocations()
        {
            var deliveryTypeId = (int)DeliveryTypeCombo.SelectedValue;

            switch (deliveryTypeId)
            {
                case 1: // Внутренняя — обе локации обязательны
                    if (FromLocationCombo.SelectedValue == null || ToLocationCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Для внутренней доставки выберите обе локации (откуда и куда).",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    break;

                case 2: // Внешняя поставка — только "куда" обязательно
                    if (ToLocationCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Для внешней поставки выберите локацию, куда доставить.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    break;

                case 3: // Выгрузка — только "откуда" обязательно
                    if (FromLocationCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Для выгрузки выберите локацию, откуда выгружать.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    break;
            }

            return true;
        }

        private void SetPropertiesFromInput()
        {
            SelectedProductId = (int)ProductCombo.SelectedValue;
            SelectedClientId = (int)ClientCombo.SelectedValue;
            SelectedDeliveryTypeId = (int)DeliveryTypeCombo.SelectedValue;
            Quantity = int.Parse(QuantityTextBox.Text);

            // Устанавливаем значения локаций в зависимости от типа доставки
            var deliveryTypeId = SelectedDeliveryTypeId;

            SelectedFromLocationId = (deliveryTypeId == 1 || deliveryTypeId == 3) ?
                (int?)FromLocationCombo.SelectedValue : null;

            SelectedToLocationId = (deliveryTypeId == 1 || deliveryTypeId == 2) ?
                (int?)ToLocationCombo.SelectedValue : null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}