using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class LoginWindow : Window
    {
        private string _sessionId;

        public LoginWindow()
        {
            InitializeComponent();
            _sessionId = Guid.NewGuid().ToString();
            LoadCaptcha();

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            // Фокус на поле логина
            Loaded += (s, e) => txtUsername.Focus();
        }

        private void LoadCaptcha()
        {
            var captcha = DatabaseManager.GenerateCaptcha(_sessionId);
            using (var ms = new MemoryStream(captcha.ImageData))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgCaptcha.Source = bitmap;
            }
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            LoadCaptcha();
            txtCaptcha.Clear();
            txtCaptcha.Focus();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                lblError.Text = "Введите логин";
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                lblError.Text = "Введите пароль";
                txtPassword.Focus();
                return;
            }

            if (!DatabaseManager.VerifyCaptcha(_sessionId, txtCaptcha.Text.Trim()))
            {
                lblError.Text = "Неверный код подтверждения";
                LoadCaptcha();
                txtCaptcha.Clear();
                txtCaptcha.Focus();
                return;
            }

            var user = await DatabaseManager.AuthenticateUserAsync(txtUsername.Text, txtPassword.Password);

            if (user != null)
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                lblError.Text = "Неверное имя пользователя или пароль";
                LoadCaptcha();
                txtCaptcha.Clear();
                txtPassword.Clear();
                txtUsername.Focus();
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}