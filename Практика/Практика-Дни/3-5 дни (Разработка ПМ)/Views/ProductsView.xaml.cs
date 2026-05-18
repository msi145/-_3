using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class ProductsView : UserControl
    {
        private ObservableCollection<Product> _products;

        public ProductsView()
        {
            InitializeComponent();
            Loaded += ProductsView_Loaded;
        }

        private async void ProductsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            try
            {
                var products = await DatabaseManager.GetAllProductsAsync();
                _products = new ObservableCollection<Product>(products);
                dgProducts.ItemsSource = _products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование продукции", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = new Product
            {
                Name = txtName.Text,
                ProductType = (cmbProductType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                ReleaseForm = (cmbReleaseForm.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                Status = "active"
            };

            await DatabaseManager.AddProductAsync(product);

            txtName.Clear();
            cmbProductType.SelectedIndex = -1;
            cmbReleaseForm.SelectedIndex = -1;

            await LoadProducts();
            MessageBox.Show("Продукция добавлена успешно", "Успех",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.Tag as Product;
            if (product == null) return;

            var dialog = new ProductEditDialog(product);
            if (dialog.ShowDialog() == true)
            {
                await LoadProducts();
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.Tag as Product;
            if (product == null) return;

            var result = MessageBox.Show($"Удалить продукцию '{product.Name}'?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await DatabaseManager.DeleteProductAsync(product.Id);
                await LoadProducts();
            }
        }
    }
}