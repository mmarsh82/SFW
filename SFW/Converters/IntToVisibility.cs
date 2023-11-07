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
            var _isInt = int.TryParse(value.ToString(), out int _val);
            var _parInt = int.TryParse(parameter?.ToString(), out int _par);
            if (_parInt && _isInt)
            {
                return _val == _par ? Visibility.Visible : Visibility.Collapsed; 
            }
            else if (parameter != null && _isInt)
            {
                return _val > 0 ? Visibility.Visible : Visibility.Collapsed;
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
