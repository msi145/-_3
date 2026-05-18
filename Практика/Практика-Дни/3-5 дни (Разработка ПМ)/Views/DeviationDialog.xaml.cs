using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class DeviationDialog : Window
    {
        private User _user;
        private int _batchId;

        public DeviationDialog(User user, int batchId)
        {
            InitializeComponent();
            _user = user;
            _batchId = batchId;

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            // Фокус на поле описания
            Loaded += (s, e) => txtDescription.Focus();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Введите описание отклонения", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return;
            }

            // Получаем выбранные значения (без эмодзи)
            string deviationType = (cmbDeviationType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            deviationType = deviationType.Replace("🌡️ ", "").Replace("📊 ", "").Replace("⏱️ ", "")
                                       .Replace("🔬 ", "").Replace("⚙️ ", "").Replace("👤 ", "").Replace("📋 ", "");

            string severity = (cmbSeverity.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Средняя";
            severity = severity.Replace("🟢 ", "").Replace("🟡 ", "").Replace("🟠 ", "").Replace("🔴 ", "");

            var deviation = new Deviation
            {
                BatchId = _batchId,
                DeviationType = deviationType,
                Severity = severity,
                Description = txtDescription.Text,
                RegisteredBy = _user.Id
            };

            await DatabaseManager.AddDeviationAsync(deviation);

            MessageBox.Show("Отклонение успешно зарегистрировано", "Успех",
                           MessageBoxButton.OK, MessageBoxImage.Information);

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