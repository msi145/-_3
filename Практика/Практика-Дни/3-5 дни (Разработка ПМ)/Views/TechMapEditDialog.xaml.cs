using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class TechMapsView : UserControl
    {
        private User _currentUser;

        public TechMapsView(User user)
        {
            InitializeComponent();
            _currentUser = user;
            Loaded += TechMapsView_Loaded;
        }

        private async void TechMapsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTechMaps();
        }

        private async Task LoadTechMaps()
        {
            try
            {
                var techMaps = await DatabaseManager.GetAllTechMapsAsync();
                dgTechMaps.ItemsSource = new ObservableCollection<TechMap>(techMaps);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void CreateTechMap_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Создание техкарты");
        }

        private void ViewTechMap_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Просмотр техкарты");
        }

        private void EditTechMap_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Редактирование техкарты");
        }

        private void DeleteTechMap_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Удаление техкарты");
        }
    }
}