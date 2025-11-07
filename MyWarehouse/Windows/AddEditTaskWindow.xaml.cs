using MyWarehouse.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyWarehouse.Windows
{
    /// <summary>
    /// Interaction logic for AddEditTaskWindow.xaml
    /// </summary>
    public partial class AddEditTaskWindow : Window
    {
        public AddEditTaskWindow()
        {
            InitializeComponent();

            ProductCombo.ItemsSource = db.CURS_Products.ToList();
            ProductCombo.DisplayMemberPath = "Name";
            ProductCombo.SelectedValuePath = "IdProduct";

            ClientCombo.ItemsSource = db.CURS_Clients.ToList();
            ClientCombo.DisplayMemberPath = "Name";
            ClientCombo.SelectedValuePath = "IdClient";

            DeliveryTypeCombo.ItemsSource = db.CURS_DeliveryTypes.ToList();
            DeliveryTypeCombo.DisplayMemberPath = "Name";
            DeliveryTypeCombo.SelectedValuePath = "IdDeliveryType";
        }

        private readonly AppDbContext db = new();

        public int SelectedProductId { get; private set; }
        public int SelectedClientId { get; private set; }
        public int SelectedDeliveryTypeId { get; private set; }
        public int Quantity { get; private set; }


        public AddEditTaskWindow(DeliveryTask task) : this()
        {
            if (task == null) return;

            ProductCombo.SelectedValue = task.ProductId;
            ClientCombo.SelectedValue = task.ClientId;
            DeliveryTypeCombo.SelectedValue = task.DeliveryTypeId;
            QtyBox.Text = task.ProductQuantity.ToString();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedValue == null || ClientCombo.SelectedValue == null || DeliveryTypeCombo.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукт, клиента и тип доставки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QtyBox.Text, out var q) || q <= 0)
            {
                MessageBox.Show("Введите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedProductId = (int)ProductCombo.SelectedValue;
            SelectedClientId = (int)ClientCombo.SelectedValue;
            SelectedDeliveryTypeId = (int)DeliveryTypeCombo.SelectedValue;
            Quantity = q;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

