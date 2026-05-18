using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class ProductEditDialog : Window
    {
        private Product _product;

        public ProductEditDialog(Product product)
        {
            InitializeComponent();
            _product = product;

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            // Загрузка данных
            txtName.Text = product.Name;

            // Установка выбранного типа
            if (!string.IsNullOrEmpty(product.ProductType))
            {
                foreach (ComboBoxItem item in cmbProductType.Items)
                {
                    string itemText = item.Content.ToString().Replace("🌾 ", "").Replace("🐛 ", "")
                                     .Replace("🍄 ", "").Replace("🌱 ", "").Replace("🧪 ", "");
                    if (itemText == product.ProductType)
                    {
                        cmbProductType.SelectedItem = item;
                        break;
                    }
                }
            }

            // Установка выбранной формы выпуска
            if (!string.IsNullOrEmpty(product.ReleaseForm))
            {
                foreach (ComboBoxItem item in cmbReleaseForm.Items)
                {
                    string itemText = item.Content.ToString();
                    if (itemText.Contains("("))
                        itemText = itemText.Substring(0, itemText.IndexOf("(")).Trim();
                    itemText = itemText.Replace("💧 ", "").Replace("🧴 ", "").Replace("⚪ ", "")
                                     .Replace("🧪 ", "");

                    if (itemText == product.ReleaseForm)
                    {
                        cmbReleaseForm.SelectedItem = item;
                        break;
                    }
                }
            }

            // Установка статуса
            if (product.Status == "active")
                cmbStatus.SelectedIndex = 0;
            else if (product.Status == "pending")
                cmbStatus.SelectedIndex = 1;
            else if (product.Status == "archived")
                cmbStatus.SelectedIndex = 2;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование продукции", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Получение чистого текста без эмодзи
            string productType = (cmbProductType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            productType = productType.Replace("🌾 ", "").Replace("🐛 ", "").Replace("🍄 ", "")
                                     .Replace("🌱 ", "").Replace("🧪 ", "");

            string releaseForm = (cmbReleaseForm.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            if (releaseForm.Contains("("))
                releaseForm = releaseForm.Substring(0, releaseForm.IndexOf("(")).Trim();
            releaseForm = releaseForm.Replace("💧 ", "").Replace("🧴 ", "").Replace("⚪ ", "")
                                     .Replace("🧪 ", "");

            string status = "";
            int statusIndex = cmbStatus.SelectedIndex;
            if (statusIndex == 0)
                status = "active";
            else if (statusIndex == 1)
                status = "pending";
            else
                status = "archived";

            _product.Name = txtName.Text;
            _product.ProductType = productType;
            _product.ReleaseForm = releaseForm;
            _product.Status = status;

            await DatabaseManager.UpdateProductAsync(_product);

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