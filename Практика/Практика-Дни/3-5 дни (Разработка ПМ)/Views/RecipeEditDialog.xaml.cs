using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class RecipeEditDialog : Window
    {
        private User _currentUser;
        private Recipe _recipe;
        private ObservableCollection<RecipeComponent> _components;

        public RecipeEditDialog(User user, Recipe recipe = null)
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();

            _currentUser = user;
            _recipe = recipe ?? new Recipe();
            _components = new ObservableCollection<RecipeComponent>();

            Loaded += async (s, e) => await LoadData();

            _components.CollectionChanged += (s, e) => UpdateComponentsCount();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            await LoadProducts();

            if (_recipe.Id != 0)
            {
                txtVersion.Text = _recipe.Version.ToString();
                txtDescription.Text = _recipe.Description;

                // Установка статуса
                if (!string.IsNullOrEmpty(_recipe.Status))
                {
                    switch (_recipe.Status.ToLower())
                    {
                        case "draft": cmbStatus.SelectedIndex = 0; break;
                        case "active": cmbStatus.SelectedIndex = 1; break;
                        case "approved": cmbStatus.SelectedIndex = 2; break;
                        case "archived": cmbStatus.SelectedIndex = 3; break;
                    }
                }

                await LoadComponents(_recipe.Id);
            }
        }

        private async System.Threading.Tasks.Task LoadProducts()
        {
            var products = await DatabaseManager.GetAllProductsAsync();
            cmbProduct.ItemsSource = products;
            cmbProduct.DisplayMemberPath = "Name";
            cmbProduct.SelectedValuePath = "Id";

            if (_recipe.ProductId > 0)
            {
                cmbProduct.SelectedValue = _recipe.ProductId;
            }
        }

        private async System.Threading.Tasks.Task LoadComponents(int recipeId)
        {
            var fullRecipe = await DatabaseManager.GetRecipeWithComponentsAsync(recipeId);
            if (fullRecipe?.Components != null)
            {
                _components.Clear();
                foreach (var comp in fullRecipe.Components)
                {
                    _components.Add(comp);
                }
                dgComponents.ItemsSource = _components;
                UpdateComponentsCount();
            }
        }

        private void AddComponent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RecipeComponentDialog();
            if (dialog.ShowDialog() == true && dialog.Component != null)
            {
                _components.Add(dialog.Component);
            }
        }

        private void EditComponent_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgComponents.SelectedItem as RecipeComponent;
            if (selected == null)
            {
                MessageBox.Show("Выберите компонент для редактирования", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new RecipeComponentDialog(selected);
            if (dialog.ShowDialog() == true && dialog.Component != null)
            {
                var index = _components.IndexOf(selected);
                _components[index] = dialog.Component;
                dgComponents.Items.Refresh();
            }
        }

        private void DeleteComponent_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgComponents.SelectedItem as RecipeComponent;
            if (selected == null)
            {
                MessageBox.Show("Выберите компонент для удаления", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить компонент '{selected.MaterialName}'?", "Подтверждение",
                               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _components.Remove(selected);
            }
        }

        private void UpdateComponentsCount()
        {
            tbComponentsCount.Text = $"Всего компонентов: {_components.Count}";
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Выберите продукт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbProduct.Focus();
                return;
            }

            _recipe.ProductId = (int)cmbProduct.SelectedValue;
            _recipe.Version = int.TryParse(txtVersion.Text, out int version) ? version : 1;

            string statusText = (cmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            if (statusText.Contains("draft")) _recipe.Status = "draft";
            else if (statusText.Contains("active")) _recipe.Status = "active";
            else if (statusText.Contains("approved")) _recipe.Status = "approved";
            else if (statusText.Contains("archived")) _recipe.Status = "archived";
            else _recipe.Status = "draft";

            _recipe.Description = txtDescription.Text;
            _recipe.Components = _components.ToList();

            if (_recipe.Id == 0)
            {
                _recipe.CreatedBy = _currentUser.Id;
                await DatabaseManager.AddRecipeAsync(_recipe);
            }
            else
            {
                await DatabaseManager.UpdateRecipeAsync(_recipe);
            }

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