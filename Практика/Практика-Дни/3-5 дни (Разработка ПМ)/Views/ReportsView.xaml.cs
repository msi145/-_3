using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using WpfApp1.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Microsoft.Win32;
using PrintDialog = System.Windows.Controls.PrintDialog;

namespace WpfApp1.Views
{
    public partial class ReportsView : UserControl
    {
        // Классы для данных отчетов
        public class ProductionReportItem
        {
            public string BatchNumber { get; set; }
            public string ProductName { get; set; }
            public decimal? ActualQty { get; set; }
            public string QtyUnit { get; set; }
            public string Status { get; set; }
            public DateTime? FinishedAt { get; set; }
            public string StatusColor => GetStatusColor(Status);

            private string GetStatusColor(string status)
            {
                return status?.ToLower() switch
                {
                    "completed" => "#4CAF50",
                    "in_progress" => "#FF9800",
                    "created" => "#2196F3",
                    "cancelled" => "#9E9E9E",
                    _ => "#757575"
                };
            }
        }

        public class DeviationReportItem
        {
            public string BatchNumber { get; set; }
            public string DeviationType { get; set; }
            public string Severity { get; set; }
            public string Description { get; set; }
            public string ResolutionStatus { get; set; }
            public DateTime? RegisteredAt { get; set; }
            public string SeverityColor => GetSeverityColor(Severity);

            private string GetSeverityColor(string severity)
            {
                return severity?.ToLower() switch
                {
                    "критическая" => "#D32F2F",
                    "высокая" => "#F44336",
                    "средняя" => "#FF9800",
                    "низкая" => "#4CAF50",
                    _ => "#9E9E9E"
                };
            }
        }

        public class MaterialUsageReportItem
        {
            public string MaterialName { get; set; }
            public decimal TotalUsed { get; set; }
            public int BatchCount { get; set; }
        }

        private ObservableCollection<ProductionReportItem> _productionItems;
        private ObservableCollection<DeviationReportItem> _deviationItems;
        private ObservableCollection<MaterialUsageReportItem> _materialItems;

        public ReportsView()
        {
            InitializeComponent();
            Loaded += ReportsView_Loaded;

            // Установка периода по умолчанию (текущий месяц)
            dpStartDate.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpEndDate.SelectedDate = DateTime.Now;

            _productionItems = new ObservableCollection<ProductionReportItem>();
            _deviationItems = new ObservableCollection<DeviationReportItem>();
            _materialItems = new ObservableCollection<MaterialUsageReportItem>();

            dgProduction.ItemsSource = _productionItems;
            dgDeviations.ItemsSource = _deviationItems;
            dgMaterialUsage.ItemsSource = _materialItems;

            // Установка лицензии для EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private void ReportsView_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateReport_Click(null, null);
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var startDate = dpStartDate.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var endDate = dpEndDate.SelectedDate ?? DateTime.Now;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                await Task.WhenAll(
                    LoadProductionReport(startDate, endDate),
                    LoadDeviationsReport(startDate, endDate),
                    LoadMaterialUsageReport(startDate, endDate)
                );

                System.Windows.MessageBox.Show($"Отчет за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} успешно сформирован!\n" +
                               $"Производство: {_productionItems.Count} партий\n" +
                               $"Отклонения: {_deviationItems.Count} записей\n" +
                               $"Сырье: {_materialItems.Count} наименований",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task LoadProductionReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT 
                            pb.batch_number as BatchNumber,
                            p.name as ProductName,
                            pb.actual_qty as ActualQty,
                            pb.qty_unit as QtyUnit,
                            pb.status as Status,
                            pb.finished_at as FinishedAt
                        FROM production_batches pb
                        LEFT JOIN production_orders po ON pb.order_id = po.id
                        LEFT JOIN products p ON po.product_id = p.id
                        WHERE pb.finished_at BETWEEN @startDate AND @endDate
                           OR (pb.status = 'in_progress' AND pb.started_at BETWEEN @startDate AND @endDate)
                        ORDER BY pb.finished_at DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate.AddDays(1));

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            _productionItems.Clear();
                            while (await reader.ReadAsync())
                            {
                                _productionItems.Add(new ProductionReportItem
                                {
                                    BatchNumber = reader["BatchNumber"]?.ToString() ?? "Н/Д",
                                    ProductName = reader["ProductName"]?.ToString() ?? "Н/Д",
                                    ActualQty = reader["ActualQty"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("ActualQty")) : (decimal?)null,
                                    QtyUnit = reader["QtyUnit"]?.ToString() ?? "кг",
                                    Status = reader["Status"]?.ToString() ?? "unknown",
                                    FinishedAt = reader["FinishedAt"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("FinishedAt")) : (DateTime?)null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadProductionReport error: {ex.Message}");
            }
        }

        private async Task LoadDeviationsReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT 
                            pb.batch_number as BatchNumber,
                            d.deviation_type as DeviationType,
                            d.severity as Severity,
                            d.description as Description,
                            d.resolution_status as ResolutionStatus,
                            d.registered_at as RegisteredAt
                        FROM deviations d
                        LEFT JOIN production_batches pb ON d.batch_id = pb.id
                        WHERE d.registered_at BETWEEN @startDate AND @endDate
                        ORDER BY d.registered_at DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate.AddDays(1));

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            _deviationItems.Clear();
                            while (await reader.ReadAsync())
                            {
                                _deviationItems.Add(new DeviationReportItem
                                {
                                    BatchNumber = reader["BatchNumber"]?.ToString() ?? "Н/Д",
                                    DeviationType = reader["DeviationType"]?.ToString() ?? "Не указан",
                                    Severity = reader["Severity"]?.ToString() ?? "Средняя",
                                    Description = reader["Description"]?.ToString() ?? "",
                                    ResolutionStatus = reader["ResolutionStatus"]?.ToString() ?? "open",
                                    RegisteredAt = reader["RegisteredAt"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("RegisteredAt")) : (DateTime?)null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDeviationsReport error: {ex.Message}");
            }
        }

        private async Task LoadMaterialUsageReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT 
                            rm.name as MaterialName,
                            SUM(ISNULL(brm.actual_qty, brm.planned_qty)) as TotalUsed,
                            COUNT(DISTINCT brm.batch_id) as BatchCount
                        FROM batch_raw_materials brm
                        LEFT JOIN raw_materials rm ON brm.raw_material_batches_id = rm.id
                        LEFT JOIN production_batches pb ON brm.batch_id = pb.id
                        WHERE pb.finished_at BETWEEN @startDate AND @endDate
                        GROUP BY rm.name
                        ORDER BY TotalUsed DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate.AddDays(1));

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            _materialItems.Clear();
                            while (await reader.ReadAsync())
                            {
                                _materialItems.Add(new MaterialUsageReportItem
                                {
                                    MaterialName = reader["MaterialName"]?.ToString() ?? "Неизвестно",
                                    TotalUsed = reader["TotalUsed"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalUsed")) : 0,
                                    BatchCount = reader["BatchCount"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("BatchCount")) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadMaterialUsageReport error: {ex.Message}");
            }
        }

        private async void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var startDate = dpStartDate.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var endDate = dpEndDate.SelectedDate ?? DateTime.Now;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"Отчет_производство_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    using (var package = new ExcelPackage())
                    {
                        // Лист "Производство"
                        var productionSheet = package.Workbook.Worksheets.Add("Производство");

                        // Заголовки
                        productionSheet.Cells[1, 1].Value = "Номер партии";
                        productionSheet.Cells[1, 2].Value = "Продукт";
                        productionSheet.Cells[1, 3].Value = "Количество";
                        productionSheet.Cells[1, 4].Value = "Ед.изм";
                        productionSheet.Cells[1, 5].Value = "Статус";
                        productionSheet.Cells[1, 6].Value = "Дата завершения";

                        // Стиль заголовков
                        using (var range = productionSheet.Cells[1, 1, 1, 6])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        // Данные
                        for (int i = 0; i < _productionItems.Count; i++)
                        {
                            var item = _productionItems[i];
                            productionSheet.Cells[i + 2, 1].Value = item.BatchNumber;
                            productionSheet.Cells[i + 2, 2].Value = item.ProductName;
                            productionSheet.Cells[i + 2, 3].Value = item.ActualQty;
                            productionSheet.Cells[i + 2, 4].Value = item.QtyUnit;
                            productionSheet.Cells[i + 2, 5].Value = item.Status;
                            productionSheet.Cells[i + 2, 6].Value = item.FinishedAt?.ToString("dd.MM.yyyy HH:mm") ?? "";
                        }

                        productionSheet.Cells[1, 1, _productionItems.Count + 1, 6].AutoFitColumns();

                        // Лист "Отклонения"
                        var deviationsSheet = package.Workbook.Worksheets.Add("Отклонения");

                        deviationsSheet.Cells[1, 1].Value = "Номер партии";
                        deviationsSheet.Cells[1, 2].Value = "Тип отклонения";
                        deviationsSheet.Cells[1, 3].Value = "Важность";
                        deviationsSheet.Cells[1, 4].Value = "Описание";
                        deviationsSheet.Cells[1, 5].Value = "Статус";
                        deviationsSheet.Cells[1, 6].Value = "Дата регистрации";

                        using (var range = deviationsSheet.Cells[1, 1, 1, 6])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        for (int i = 0; i < _deviationItems.Count; i++)
                        {
                            var item = _deviationItems[i];
                            deviationsSheet.Cells[i + 2, 1].Value = item.BatchNumber;
                            deviationsSheet.Cells[i + 2, 2].Value = item.DeviationType;
                            deviationsSheet.Cells[i + 2, 3].Value = item.Severity;
                            deviationsSheet.Cells[i + 2, 4].Value = item.Description;
                            deviationsSheet.Cells[i + 2, 5].Value = item.ResolutionStatus;
                            deviationsSheet.Cells[i + 2, 6].Value = item.RegisteredAt?.ToString("dd.MM.yyyy") ?? "";
                        }

                        deviationsSheet.Cells[1, 1, _deviationItems.Count + 1, 6].AutoFitColumns();

                        // Лист "Расход сырья"
                        var materialsSheet = package.Workbook.Worksheets.Add("Расход сырья");

                        materialsSheet.Cells[1, 1].Value = "Сырье";
                        materialsSheet.Cells[1, 2].Value = "Израсходовано (кг)";
                        materialsSheet.Cells[1, 3].Value = "Количество партий";

                        using (var range = materialsSheet.Cells[1, 1, 1, 3])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        for (int i = 0; i < _materialItems.Count; i++)
                        {
                            var item = _materialItems[i];
                            materialsSheet.Cells[i + 2, 1].Value = item.MaterialName;
                            materialsSheet.Cells[i + 2, 2].Value = item.TotalUsed;
                            materialsSheet.Cells[i + 2, 3].Value = item.BatchCount;
                        }

                        materialsSheet.Cells[1, 1, _materialItems.Count + 1, 3].AutoFitColumns();

                        // Лист с информацией
                        var infoSheet = package.Workbook.Worksheets.Add("Информация");
                        infoSheet.Cells[1, 1].Value = "Параметр";
                        infoSheet.Cells[1, 2].Value = "Значение";
                        infoSheet.Cells[2, 1].Value = "Период отчета";
                        infoSheet.Cells[2, 2].Value = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                        infoSheet.Cells[3, 1].Value = "Дата формирования";
                        infoSheet.Cells[3, 2].Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                        infoSheet.Cells[4, 1].Value = "Всего партий";
                        infoSheet.Cells[4, 2].Value = _productionItems.Count;
                        infoSheet.Cells[5, 1].Value = "Всего отклонений";
                        infoSheet.Cells[5, 2].Value = _deviationItems.Count;

                        using (var range = infoSheet.Cells[1, 1, 1, 2])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }

                        infoSheet.Cells[1, 1, 5, 2].AutoFitColumns();

                        // Сохранение файла
                        var fileInfo = new FileInfo(saveFileDialog.FileName);
                        await package.SaveAsAsync(fileInfo);

                        System.Windows.MessageBox.Show($"Отчет успешно экспортирован!\nФайл сохранен: {saveFileDialog.FileName}",
                                       "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Получаем текущий TabControl (нужно добавить x:Name="tabControl" в XAML)
                    var tabControl = FindName("mainTabControl") as System.Windows.Controls.TabControl;
                    if (tabControl != null)
                    {
                        if (tabControl.SelectedIndex == 0)
                            printDialog.PrintVisual(dgProduction, "Отчет по производству");
                        else if (tabControl.SelectedIndex == 1)
                            printDialog.PrintVisual(dgDeviations, "Отчет по отклонениям");
                        else
                            printDialog.PrintVisual(dgMaterialUsage, "Отчет по расходу сырья");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка печати: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}