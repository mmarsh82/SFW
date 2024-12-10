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
            if (parameter != null && parameter.ToString().Contains("Ncr"))
            {
                var _temp = int.TryParse(value.ToString(), out int i) ? i : 0;
                switch (parameter.ToString())
                {
                    case "NcrHeader":
                        return _temp == 0 ? Visibility.Visible : Visibility.Collapsed;
                    case "NcrDetail":
                        return _temp == 0 ? Visibility.Hidden : Visibility.Visible;

                }
            }
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
