using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1.Views
{
    /// <summary>
    /// Конвертер статуса в цвет для отображения
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString().ToLower() ?? "";

            switch (status)
            {
                case "completed":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80));     // Зеленый - #4CAF50
                case "in_progress":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0));     // Оранжевый - #FF9800
                case "created":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243));    // Синий - #2196F3
                case "cancelled":
                    return new SolidColorBrush(Color.FromRgb(158, 158, 158));   // Серый - #9E9E9E
                case "quality_control":
                    return new SolidColorBrush(Color.FromRgb(156, 39, 176));    // Фиолетовый - #9C27B0
                default:
                    return new SolidColorBrush(Color.FromRgb(117, 117, 117));   // Темно-серый - #757575
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер статуса в текст на русском языке
    /// </summary>
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString().ToLower() ?? "";

            switch (status)
            {
                case "completed":
                    return "ЗАВЕРШЕНА";
                case "in_progress":
                    return "В РАБОТЕ";
                case "created":
                    return "СОЗДАНА";
                case "cancelled":
                    return "ОТМЕНЕНА";
                case "quality_control":
                    return "КОНТРОЛЬ";
                default:
                    return status.ToUpper();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер даты для цвета текста
    /// </summary>
    public class DateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Color.FromRgb(158, 158, 158));

            return new SolidColorBrush(Color.FromRgb(117, 117, 117));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}