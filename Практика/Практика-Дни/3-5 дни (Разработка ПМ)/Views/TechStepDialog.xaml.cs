using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class TechStepDialog : Window
    {
        public TechStep Step { get; private set; }

        public TechStepDialog(TechStep step = null)
        {
            InitializeComponent();

            if (step != null)
            {
                Step = step;
                txtName.Text = step.Name;
                txtInstructions.Text = step.Instructions;
                txtParamName.Text = step.ParamName;

                if (step.ParamTarget.HasValue)
                    txtParamTarget.Text = step.ParamTarget.Value.ToString();

                if (step.ParamMin.HasValue)
                    txtParamMin.Text = step.ParamMin.Value.ToString();

                if (step.ParamMax.HasValue)
                    txtParamMax.Text = step.ParamMax.Value.ToString();

                // Установка единицы измерения
                if (!string.IsNullOrEmpty(step.ParamUnit))
                {
                    foreach (ComboBoxItem item in cmbParamUnit.Items)
                    {
                        if (item.Content.ToString() == step.ParamUnit)
                        {
                            cmbParamUnit.SelectedItem = item;
                            break;
                        }
                    }
                }

                if (step.DurationMin.HasValue)
                    txtDurationMin.Text = step.DurationMin.Value.ToString();

                chkIsMandatory.IsChecked = step.IsMandatory ?? false;

                // Установка типа шага
                if (!string.IsNullOrEmpty(step.StepType))
                {
                    foreach (ComboBoxItem item in cmbStepType.Items)
                    {
                        if (item.Content.ToString() == step.StepType)
                        {
                            cmbStepType.SelectedItem = item;
                            break;
                        }
                    }
                }
            }

            Loaded += (s, e) => txtName.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название шага", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            string stepType = (cmbStepType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string paramUnit = (cmbParamUnit.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "°C";

            Step = new TechStep
            {
                Name = txtName.Text,
                StepType = stepType,
                Instructions = txtInstructions.Text,
                ParamName = txtParamName.Text,
                ParamTarget = decimal.TryParse(txtParamTarget.Text, out decimal target) ? target : (decimal?)null,
                ParamMin = decimal.TryParse(txtParamMin.Text, out decimal min) ? min : (decimal?)null,
                ParamMax = decimal.TryParse(txtParamMax.Text, out decimal max) ? max : (decimal?)null,
                ParamUnit = paramUnit,
                DurationMin = int.TryParse(txtDurationMin.Text, out int duration) ? duration : (int?)null,
                IsMandatory = chkIsMandatory.IsChecked ?? false
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