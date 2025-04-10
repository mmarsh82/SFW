using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-1-18

namespace SFW.Converters
{
    public sealed class FormTypeToVisibility : IValueConverter, IMultiValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           if (parameter != null)
           {
                return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
           }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Collapsed;
        }

        #endregion

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null && values.Count() == 2 && bool.TryParse(values[1].ToString(), out bool b))
            {
                var _type = parameter.ToString();
                var _std = true;
                if (_type.Contains('*'))
                {
                    _std = _type.Split('*')[1] == "i" ? false : true;
                    _type = _type.Split('*')[0];
                }
                return values[0].ToString() == parameter.ToString() && b ? Visibility.Visible : Visibility.Collapsed;
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
