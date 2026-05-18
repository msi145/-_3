using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class CreateBatchDialog : Window
    {
        private User _user;
        private ProductionOrder _order;

        public CreateBatchDialog(User user, ProductionOrder order)
        {
            InitializeComponent();
            _user = user;
            _order = order;

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            // Заполнение информации о заказе
            tbOrderInfo.Text = $"Заказ №{order.OrderNumber}";
            tbProductInfo.Text = $"Продукт ID: {order.ProductId}";
            tbPlannedInfo.Text = $"Плановое количество: {order.PlannedQty} {order.QtyUnit}";

            // Генерация номера партии
            txtBatchNumber.Text = $"BATCH-{DateTime.Now:yyyyMMddHHmmss}";

            // Фокус на поле примечаний
            Loaded += (s, e) => txtNotes.Focus();
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            // Получение единицы измерения (без текста в скобках)
            string unit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "кг";
            if (unit.Contains("("))
                unit = unit.Substring(0, unit.IndexOf("(")).Trim();

            var batch = new ProductionBatch
            {
                OrderId = _order.Id,
                BatchNumber = txtBatchNumber.Text,
                QtyUnit = unit,
                Notes = txtNotes.Text,
                ResponsibleUser = _user.Id
            };

            int batchId = await DatabaseManager.CreateBatchAsync(batch);
            await DatabaseManager.StartBatchAsync(batchId, _user.Id);

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