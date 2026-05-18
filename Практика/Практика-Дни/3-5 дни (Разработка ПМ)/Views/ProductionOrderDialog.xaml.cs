using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class ProductionOrderDialog : Window
    {
        private User _user;

        public ProductionOrderDialog(User user)
        {
            InitializeComponent();
            _user = user;

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            LoadProducts();
            dpPlannedDate.SelectedDate = DateTime.Now.AddDays(7);
        }

        private async void LoadProducts()
        {
            var products = await DatabaseManager.GetAllProductsAsync();
            cmbProduct.ItemsSource = products;
            cmbProduct.DisplayMemberPath = "Name";
            cmbProduct.SelectedValuePath = "Id";
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Выберите продукт для производства", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbProduct.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPlannedQty.Text))
            {
                MessageBox.Show("Введите плановое количество продукции", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPlannedQty.Focus();
                return;
            }

            if (!decimal.TryParse(txtPlannedQty.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество (положительное число)", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPlannedQty.Focus();
                return;
            }

            if (dpPlannedDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите плановую дату производства", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получение единицы измерения
            string unit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "кг";
            if (unit.Contains("("))
                unit = unit.Substring(0, unit.IndexOf("(")).Trim();

            // Получение приоритета
            int priority = cmbPriority.SelectedIndex + 1;

            var order = new ProductionOrder
            {
                ProductId = (int?)cmbProduct.SelectedValue,
                PlannedQty = qty,
                QtyUnit = unit,
                PlannedDate = dpPlannedDate.SelectedDate.Value,
                Priority = priority,
                Status = "planned",
                CreatedBy = _user.Id
            };

            await DatabaseManager.AddProductionOrderAsync(order);

            MessageBox.Show("Производственный заказ успешно создан!", "Успех",
                           MessageBoxButton.OK, MessageBoxImage.Information);

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