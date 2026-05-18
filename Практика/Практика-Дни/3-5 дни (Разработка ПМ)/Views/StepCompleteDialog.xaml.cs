using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class StepCompleteDialog : Window
    {
        public decimal? ActualValue { get; private set; }
        public int? ActualDuration { get; private set; }
        public string Comment { get; private set; }

        public StepCompleteDialog(StepExecution step)
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();

            tbStepName.Text = step.StepName ?? $"Шаг #{step.TechStepId}";
            txtActualValue.Text = step.ActualParamValue?.ToString() ?? "";
            txtActualDuration.Text = step.ActualDurationMin?.ToString() ?? "";
            txtComment.Text = step.Comment ?? "";

            Loaded += (s, e) => txtActualValue.Focus();
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            ActualValue = decimal.TryParse(txtActualValue.Text, out decimal v) ? v : (decimal?)null;
            ActualDuration = int.TryParse(txtActualDuration.Text, out int d) ? d : (int?)null;
            Comment = txtComment.Text;

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