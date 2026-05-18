using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class ProductionOrdersView : UserControl
    {
        private User _currentUser;
        private ObservableCollection<ProductionOrder> _orders;

        public ProductionOrdersView(User user)
        {
            InitializeComponent();
            _currentUser = user;
            Loaded += ProductionOrdersView_Loaded;
        }

        private async void ProductionOrdersView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                var orders = await DatabaseManager.GetAllProductionOrdersAsync();
                _orders = new ObservableCollection<ProductionOrder>(orders);
                dgOrders.ItemsSource = _orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductionOrderDialog(_currentUser);
            if (dialog.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private async void StartOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button)?.Tag as ProductionOrder;
            if (order == null) return;

            var dialog = new CreateBatchDialog(_currentUser, order);
            if (dialog.ShowDialog() == true)
            {
                await LoadOrders();
                MessageBox.Show("Партия создана и запущена", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewBatches_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button)?.Tag as ProductionOrder;
            if (order != null)
            {
                var dialog = new BatchesListDialog(order);
                dialog.ShowDialog();
            }
        }

        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button)?.Tag as ProductionOrder;
            if (order == null) return;

            var result = MessageBox.Show($"Удалить заказ '{order.OrderNumber}'?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Здесь добавить логику удаления
                await LoadOrders();
            }
        }
    }
}