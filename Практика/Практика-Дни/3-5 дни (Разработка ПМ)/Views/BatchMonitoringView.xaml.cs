using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    public partial class BatchMonitoringView : UserControl
    {
        private User _currentUser;
        private ObservableCollection<ProductionBatch> _batches;
        private int _selectedBatchId;

        public BatchMonitoringView(User user)
        {
            InitializeComponent();
            _currentUser = user;
            Loaded += BatchMonitoringView_Loaded;
        }

        private async void BatchMonitoringView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBatches();
        }

        private async Task LoadBatches()
        {
            try
            {
                var orders = await DatabaseManager.GetAllProductionOrdersAsync();
                _batches = new ObservableCollection<ProductionBatch>();
                foreach (var order in orders)
                {
                    var batches = await DatabaseManager.GetBatchesByOrderAsync(order.Id);
                    foreach (var batch in batches)
                    {
                        _batches.Add(batch);
                    }
                }
                cmbBatches.ItemsSource = _batches;
                cmbBatches.DisplayMemberPath = "BatchNumber";
                cmbBatches.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Batch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBatches.SelectedItem is ProductionBatch batch)
            {
                _selectedBatchId = batch.Id;

                // Обновление информационной панели
                tbBatchNumber.Text = batch.BatchNumber;
                tbStatus.Text = batch.Status;
                tbStartDate.Text = batch.StartedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не начата";
                tbResponsible.Text = batch.ResponsibleName ?? "Не назначен";

                await LoadStepExecutions();
                await LoadDeviations();
            }
        }

        private async Task LoadStepExecutions()
        {
            try
            {
                var executions = await DatabaseManager.GetBatchStepExecutionsAsync(_selectedBatchId);
                dgSteps.ItemsSource = executions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шагов: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadDeviations()
        {
            try
            {
                var deviations = await DatabaseManager.GetBatchDeviationsAsync(_selectedBatchId);
                dgDeviations.ItemsSource = deviations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отклонений: {ex.Message}", "Ошибка");
            }
        }

        private async void CompleteStep_Click(object sender, RoutedEventArgs e)
        {
            var step = (sender as Button)?.Tag as StepExecution;
            if (step == null) return;

            var dialog = new StepCompleteDialog(step);
            if (dialog.ShowDialog() == true)
            {
                await DatabaseManager.CompleteStepExecutionAsync(step.Id, _currentUser.Id,
                    dialog.ActualValue, dialog.ActualDuration, dialog.Comment);
                await LoadStepExecutions();
            }
        }

        private async void CompleteBatch_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BatchCompleteDialog();
            if (dialog.ShowDialog() == true)
            {
                await DatabaseManager.CompleteBatchAsync(_selectedBatchId, dialog.ActualQty);
                await LoadBatches();
                MessageBox.Show("Партия завершена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddDeviation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DeviationDialog(_currentUser, _selectedBatchId);
            if (dialog.ShowDialog() == true)
            {
                LoadDeviations();
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadBatches();
            if (_selectedBatchId != 0)
            {
                await LoadStepExecutions();
                await LoadDeviations();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработка переключения вкладок при необходимости
        }
    }
}