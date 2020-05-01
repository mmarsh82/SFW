using System;
using System.Windows;
using System.Windows.Data;
using System.Linq;

//Created by Michael Marsh 5-1-18

namespace SFW.Converters
{
    public sealed class StringToVisibility : IValueConverter, IMultiValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null)
            {
                var _val = parameter.ToString();
                switch (_val)
                {
                    case "i":
                        return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
                    case "Hide":
                        return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Hidden : Visibility.Visible;
                    case "Status":
                        return value?.ToString() == "O" ? Visibility.Collapsed : Visibility.Visible;
                    case "PriTime":
                        return value?.ToString() == "999" ? Visibility.Collapsed : Visibility.Visible;
                    case "N":
                        return value != null && !string.IsNullOrEmpty(value.ToString()) && value.ToString()[value.ToString().Length - 1] == 'N' ? Visibility.Visible : Visibility.Collapsed;
                    default:
                        return value.ToString() == parameter.ToString().Replace('~', ' ').Trim() ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible;
        }

        #endregion

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null)
            {
                switch (parameter.ToString())
                {
                    case "N":
                        return values.Any(o => !string.IsNullOrEmpty(o?.ToString()) && o?.ToString()[o.ToString().Length - 1] == 'N') ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
