using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class MaterialsView : UserControl
    {
        private ObservableCollection<RawMaterial> _materials;
        private ObservableCollection<RawMaterialBatch> _batches;

        public MaterialsView()
        {
            InitializeComponent();
            Loaded += MaterialsView_Loaded;
        }

        private async void MaterialsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMaterials();
            await LoadBatches();
        }

        private async Task LoadMaterials()
        {
            try
            {
                var materials = await DatabaseManager.GetAllRawMaterialsAsync();
                _materials = new ObservableCollection<RawMaterial>(materials);
                dgMaterials.ItemsSource = _materials;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сырья: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBatches()
        {
            try
            {
                var batches = await DatabaseManager.GetRawMaterialBatchesAsync();
                _batches = new ObservableCollection<RawMaterialBatch>(batches);
                dgBatches.ItemsSource = _batches;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий сырья: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaterialName.Text))
            {
                MessageBox.Show("Введите наименование сырья", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMaterialName.Focus();
                return;
            }

            var material = new RawMaterial
            {
                Name = txtMaterialName.Text,
                Unit = "кг",
                MaterialType = (cmbMaterialType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Сырье",
                CasNumber = txtCasNumber.Text,
                IsHazardous = false
            };

            await DatabaseManager.AddRawMaterialAsync(material);

            MessageBox.Show("Сырье успешно добавлено", "Успех",
                           MessageBoxButton.OK, MessageBoxImage.Information);

            txtMaterialName.Clear();
            txtCasNumber.Clear();
            cmbMaterialType.SelectedIndex = 0;

            await LoadMaterials();
            txtMaterialName.Focus();
        }
    }
}