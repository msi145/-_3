using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class RecipeComponentDialog : Window
    {
        public RecipeComponent Component { get; private set; }
        private string _materialName;

        public RecipeComponentDialog(RecipeComponent component = null)
        {
            InitializeComponent();

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            LoadMaterials();

            if (component != null)
            {
                Component = component;
                cmbMaterial.SelectedValue = component.MaterialId;
                txtQuantity.Text = component.Quantity.ToString();

                // Установка единицы измерения
                foreach (ComboBoxItem item in cmbUnit.Items)
                {
                    if (item.Content.ToString() == component.Unit)
                    {
                        cmbUnit.SelectedItem = item;
                        break;
                    }
                }

                txtTolerance.Text = component.TolerancePct?.ToString() ?? "5";

                // Установка порядка загрузки
                int loadOrder = component.LoadOrder ?? 1;
                if (loadOrder >= 1 && loadOrder <= 10)
                    cmbLoadOrder.SelectedIndex = loadOrder - 1;

                _materialName = component.MaterialName;
            }
        }

        private async void LoadMaterials()
        {
            var materials = await DatabaseManager.GetAllRawMaterialsAsync();
            cmbMaterial.ItemsSource = materials;
            cmbMaterial.DisplayMemberPath = "Name";
            cmbMaterial.SelectedValuePath = "Id";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaterial.SelectedValue == null)
            {
                MessageBox.Show("Выберите сырье или материал", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbMaterial.Focus();
                return;
            }

            if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество (положительное число)", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            if (!decimal.TryParse(txtTolerance.Text, out decimal tolerance) || tolerance < 0 || tolerance > 100)
            {
                MessageBox.Show("Введите корректный допуск (0-100%)", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTolerance.Focus();
                return;
            }

            string unit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "кг";
            if (unit.Contains("("))
                unit = unit.Substring(0, unit.IndexOf("(")).Trim();

            int loadOrder = cmbLoadOrder.SelectedIndex + 1;

            var selectedMaterial = cmbMaterial.SelectedItem as RawMaterial;

            Component = new RecipeComponent
            {
                MaterialId = (int)cmbMaterial.SelectedValue,
                MaterialName = selectedMaterial?.Name ?? "",
                Quantity = quantity,
                Unit = unit,
                TolerancePct = tolerance,
                LoadOrder = loadOrder
            };

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