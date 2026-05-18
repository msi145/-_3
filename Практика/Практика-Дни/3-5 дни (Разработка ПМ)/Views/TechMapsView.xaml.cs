using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class TechMapViewDialog : Window
    {
        public TechMapViewDialog(TechMap techMap)
        {
            InitializeComponent();

            if (techMap != null)
            {
                tbId.Text = techMap.Id.ToString();
                tbName.Text = techMap.Name ?? "Не указано";
                tbRecipe.Text = techMap.RecipeId.ToString();
                tbVersion.Text = techMap.Version.ToString();

                // Статус
                string status = techMap.Status ?? "draft";
                if (status == "draft") tbStatus.Text = "Черновик";
                else if (status == "active") tbStatus.Text = "Активен";
                else if (status == "approved") tbStatus.Text = "Утвержден";
                else tbStatus.Text = "Архивный";

                // Шаги
                if (techMap.Steps != null && techMap.Steps.Count > 0)
                {
                    var sortedSteps = techMap.Steps.OrderBy(s => s.StepNumber).ToList();
                    dgSteps.ItemsSource = new ObservableCollection<TechStep>(sortedSteps);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}