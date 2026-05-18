using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class RecipeViewDialog : Window
    {
        public RecipeViewDialog(Recipe recipe)
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();

            if (recipe != null)
            {
                tbId.Text = recipe.Id.ToString();
                tbProduct.Text = recipe.ProductId.ToString();
                tbVersion.Text = recipe.Version.ToString();
                tbDescription.Text = recipe.Description ?? "Нет описания";

                SetStatusStyle(recipe.Status);

                if (recipe.Components != null && recipe.Components.Count > 0)
                {
                    dgComponents.ItemsSource = new ObservableCollection<RecipeComponent>(recipe.Components);
                    tbComponentsCount.Text = $"Всего компонентов: {recipe.Components.Count}";
                }
                else
                {
                    tbComponentsCount.Text = "Нет компонентов";
                }
            }
        }

        private void SetStatusStyle(string status)
        {
            if (string.IsNullOrEmpty(status)) status = "draft";

            switch (status.ToLower())
            {
                case "draft":
                    tbStatus.Text = "Черновик";
                    statusBorder.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    break;
                case "active":
                    tbStatus.Text = "Активен";
                    statusBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    break;
                case "approved":
                    tbStatus.Text = "Утвержден";
                    statusBorder.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                    break;
                case "archived":
                    tbStatus.Text = "Архивный";
                    statusBorder.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    break;
                default:
                    tbStatus.Text = status;
                    statusBorder.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    break;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}