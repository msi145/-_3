using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class BatchesListDialog : Window
    {
        private ProductionOrder _order;

        public BatchesListDialog(ProductionOrder order)
        {
            InitializeComponent();
            _order = order;

            Title = $"Партии по заказу #{order.OrderNumber}";
            TitleText.Text = $"Заказ #{order.OrderNumber}";

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            Loaded += async (s, e) => await LoadBatches(order.Id);
        }

        private async Task LoadBatches(int orderId)
        {
            try
            {
                var batches = await DatabaseManager.GetBatchesByOrderAsync(orderId);
                dgBatches.ItemsSource = new ObservableCollection<ProductionBatch>(batches);
                LastUpdatedText.Text = $"Последнее обновление: {DateTime.Now:HH:mm:ss dd.MM.yyyy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}