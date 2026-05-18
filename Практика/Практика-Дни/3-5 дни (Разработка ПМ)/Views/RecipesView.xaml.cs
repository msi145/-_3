using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class RecipesView : UserControl
    {
        private User _currentUser;
        private ObservableCollection<Recipe> _recipes;

        public RecipesView(User user)
        {
            InitializeComponent();
            _currentUser = user;
            Loaded += RecipesView_Loaded;
        }

        private async void RecipesView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRecipes();
        }

        private async Task LoadRecipes()
        {
            try
            {
                var recipes = await DatabaseManager.GetAllRecipesAsync();
                _recipes = new ObservableCollection<Recipe>(recipes);
                dgRecipes.ItemsSource = _recipes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рецептур: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateRecipe_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RecipeEditDialog(_currentUser);
            if (dialog.ShowDialog() == true)
            {
                LoadRecipes();
            }
        }

        private async void ViewRecipe_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button)?.Tag as Recipe;
            if (recipe != null)
            {
                var fullRecipe = await DatabaseManager.GetRecipeWithComponentsAsync(recipe.Id);
                var dialog = new RecipeViewDialog(fullRecipe);
                dialog.ShowDialog();
            }
        }

        private async void EditRecipe_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button)?.Tag as Recipe;
            if (recipe != null)
            {
                var fullRecipe = await DatabaseManager.GetRecipeWithComponentsAsync(recipe.Id);
                var dialog = new RecipeEditDialog(_currentUser, fullRecipe);
                if (dialog.ShowDialog() == true)
                {
                    await LoadRecipes();
                }
            }
        }

        private async void DeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button)?.Tag as Recipe;
            if (recipe == null) return;

            var result = MessageBox.Show($"Удалить рецептуру {recipe.Id}?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Здесь добавить метод удаления
                await LoadRecipes();
            }
        }
    }
}