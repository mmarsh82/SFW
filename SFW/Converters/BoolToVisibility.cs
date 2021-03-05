using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-1-18

namespace SFW.Converters
{
    public class BoolToVisibility : IValueConverter, IMultiValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().Contains("*"))
            {
                value = value.ToString().Split('*')[0];
            }
            if (int.TryParse(value.ToString(), out int i))
            {
                value = System.Convert.ToBoolean(i);
            }
            if (bool.TryParse(value.ToString(), out bool bResult))
            {
                return parameter?.ToString() == "i"
                    ? bResult ? Visibility.Collapsed : Visibility.Visible
                    : bResult ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }

        #endregion

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values[0] != null)
            {
                switch (values[0])
                {
                    case "CSI":
                        values[0] = 0;
                        break;
                    case "WCCO":
                        values[0] = 1;
                        break;
                }
            }
            else
            {
                values[0] = false;
            }
            var _param = parameter?.ToString();
            var _rtnVal = true;
            if (parameter == null)
            {
                foreach (var o in values)
                {
                    if (o != DependencyProperty.UnsetValue && !System.Convert.ToBoolean(o))
                    {
                        _rtnVal = _param == "i" ? true : false;
                    }
                    if (o != DependencyProperty.UnsetValue && int.TryParse(o.ToString(), out int i))
                    {
                        if (i >= 999)
                        {
                            _rtnVal = _param == "i" ? true : false;
                        }
                    }
                }
            }
            return _rtnVal ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
