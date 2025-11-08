using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models.Entities;
using System.Linq;
using System.Windows;

namespace MyWarehouse.Windows
{
    public partial class AddEditTaskWindow : Window
    {
        private readonly AppDbContext db = new();

        public int SelectedProductId { get; private set; }
        public int SelectedClientId { get; private set; }
        public int SelectedDeliveryTypeId { get; private set; }
        public int? SelectedFromLocationId { get; private set; }
        public int? SelectedToLocationId { get; private set; }
        public int Quantity { get; private set; }

        public AddEditTaskWindow()
        {
            InitializeComponent();

            ProductCombo.ItemsSource = db.CURS_Products.ToList();
            ClientCombo.ItemsSource = db.CURS_Clients.ToList();
            DeliveryTypeCombo.ItemsSource = db.CURS_DeliveryTypes.ToList();
            FromLocationCombo.ItemsSource = db.CURS_Locations.ToList();
            ToLocationCombo.ItemsSource = db.CURS_Locations.ToList();


            

        }

        public AddEditTaskWindow(DeliveryTask task) : this()
        {
            if (task == null) 
                return;

            ProductCombo.SelectedValue = task.ProductId;
            ClientCombo.SelectedValue = task.ClientId;
            DeliveryTypeCombo.SelectedValue = task.DeliveryTypeId;
            QuantityTextBox.Text = task.ProductQuantity.ToString();
            FromLocationCombo.SelectedValue = task.FromLocationId;
            ToLocationCombo.SelectedValue = task.ToLocationId;
        }

        private void DeliveryTypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FromLocationStack.Visibility = Visibility.Collapsed;
            ToLocationStack.Visibility = Visibility.Collapsed;
            if (DeliveryTypeCombo.SelectedValue is not int typeId) 
                return;

            switch (typeId)
            {
                case 1: // Внешняя поставка — только "куда"
                     ToLocationStack.Visibility = Visibility.Visible;
                    break;
                case 2: // Внутренняя — обе локации
                    FromLocationStack.Visibility  = Visibility.Visible;
                     ToLocationStack.Visibility = Visibility.Visible;
                    break;
                case 3: // Выгрузка — только "откуда"
                    FromLocationStack.Visibility  = Visibility.Visible;
                    break;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedValue == null ||
                ClientCombo.SelectedValue == null ||
                DeliveryTypeCombo.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукт, клиента и тип доставки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var q) || q <= 0)
            {
                MessageBox.Show("Введите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedProductId = (int)ProductCombo.SelectedValue;
            SelectedClientId = (int)ClientCombo.SelectedValue;
            SelectedDeliveryTypeId = (int)DeliveryTypeCombo.SelectedValue;
            Quantity = q;

            // Проверка локаций по типу задачи
            switch (SelectedDeliveryTypeId)
            {
                case 1:
                    if (ToLocationCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Выберите локацию, куда доставить.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    SelectedToLocationId = (int)ToLocationCombo.SelectedValue;
                    SelectedFromLocationId = null;
                    break;

                case 2:
                    if (FromLocationCombo.SelectedValue == null || ToLocationCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Выберите обе локации (откуда и куда).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    SelectedFromLocationId = (int)FromLocationCombo.SelectedValue;
                    SelectedToLocationId = (int)ToLocationCombo.SelectedValue;
                    break;

                case 3:
                    if (FromLocationCombo.SelectedValue == null)
                    {
                        MessageBox.Show("Выберите локацию, откуда выгружать.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    SelectedFromLocationId = (int)FromLocationCombo.SelectedValue;
                    SelectedToLocationId = null;
                    break;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
