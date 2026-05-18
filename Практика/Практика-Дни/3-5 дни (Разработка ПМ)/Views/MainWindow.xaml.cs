using System;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public string UserFullName => _currentUser?.FullName ?? "Пользователь";

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = this;

            // Загрузка начальной страницы
            Products_Click(null, null);
        }

        private void SetStatus(string message)
        {
            statusBar.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new ProductsView();
            SetStatus("Загрузка справочника продукции");
        }

        private void Recipes_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new RecipesView(_currentUser);
            SetStatus("Загрузка рецептур");
        }

        private void TechMaps_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new TechMapsView(_currentUser);
            SetStatus("Загрузка технологических карт");
        }

        private void Materials_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new MaterialsView();
            SetStatus("Загрузка справочника сырья");
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new ProductionOrdersView(_currentUser);
            SetStatus("Загрузка производственных заказов");
        }

        private void Monitoring_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new BatchMonitoringView(_currentUser);
            SetStatus("Загрузка мониторинга партий");
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new ReportsView();
            SetStatus("Загрузка отчетов");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}