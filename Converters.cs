using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace XLinkViewer
{
    /// <summary>
    /// Static class containing common converters for the application
    /// </summary>
    public static class Converters
    {
        public static readonly IValueConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverter();
        
        public static readonly CountToVisibilityConverter CountToVisibilityConverter = new CountToVisibilityConverter();
    }

    /// <summary>
    /// Converts a count to visibility - shows when count is 0 (for "no items" messages)
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}