using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WotlkCPKTools.Core
{
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrEmpty(s))
                return Visibility.Collapsed; // Hide watermark when user types
            return Visibility.Visible;       // Show watermark when empty
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
