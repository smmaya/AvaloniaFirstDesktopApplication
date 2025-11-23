using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Shared.Enums;

namespace Avalonia.ToDo.Desktop.Converters
{
    public class LogTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                LogType.Create => new SolidColorBrush(Colors.Green, 0.5),
                LogType.Delete => new SolidColorBrush(Colors.Red, 0.5),
                LogType.Update => new SolidColorBrush(Colors.Blue, 0.5),
                LogType.Info => Brushes.Gray,
                _ => Brushes.DarkSlateGray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}