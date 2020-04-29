using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SFW.Converters
{
    public class IntToVisibility : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _isInt = int.TryParse(value.ToString(), out int i);
            if (parameter != null && _isInt)
            {
                return i > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return _isInt ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }

        #endregion
    }
}
