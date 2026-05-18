using System.Windows;
using System.Windows.Input;

namespace WpfApp1.Views
{
    public partial class BatchCompleteDialog : Window
    {
        public decimal ActualQty { get; private set; }

        public BatchCompleteDialog()
        {
            InitializeComponent();

            // Установка фокуса и обработка клавиш
            Loaded += (s, e) => txtActualQty.Focus();
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Cancel_Click(null, null);
            else if (e.Key == Key.Enter)
                Complete_Click(null, null);
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtActualQty.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество продукции (больше 0)",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtActualQty.Text = "";
                txtActualQty.Focus();
                return;
            }

            ActualQty = qty;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void txtActualQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, точку и запятую
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}