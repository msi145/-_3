using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Data.SqlClient;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class RegisterWindow : Window
    {
        private string _sessionId;

        public RegisterWindow()
        {
            InitializeComponent();
            _sessionId = Guid.NewGuid().ToString();

            // Перетаскивание окна
            MouseLeftButtonDown += (s, e) => DragMove();

            LoadCaptcha();

            // Фокус на поле ФИО
            Loaded += (s, e) => txtFullName.Focus();
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

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                lblError.Text = "Введите ФИО";
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                lblError.Text = "Введите логин";
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                lblError.Text = "Введите email";
                txtEmail.Focus();
                return;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                lblError.Text = "Введите корректный email адрес";
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                lblError.Text = "Введите пароль";
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Password.Length < 6)
            {
                lblError.Text = "Пароль должен содержать не менее 6 символов";
                txtPassword.Focus();
                return;
            }

            // Проверка капчи
            if (string.IsNullOrWhiteSpace(txtCaptcha.Text))
            {
                lblError.Text = "Введите код подтверждения";
                txtCaptcha.Focus();
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

            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Проверка существования пользователя
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";
                    using (var cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        int count = (int)await cmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            lblError.Text = "Пользователь с таким логином или email уже существует";
                            return;
                        }
                    }

                    // Создание пользователя
                    string insertQuery = @"
                        INSERT INTO users (username, password_hash, full_name, email, is_active, created_at)
                        VALUES (@username, @password, @fullName, @email, 1, @createdAt)";

                    using (var cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@password", DatabaseManager.HashPassword(txtPassword.Password));
                        cmd.Parameters.AddWithValue("@fullName", txtFullName.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Регистрация успешно завершена!\nТеперь вы можете войти в систему.",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = $"Ошибка регистрации: {ex.Message}";
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}