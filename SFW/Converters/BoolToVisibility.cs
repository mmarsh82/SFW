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
            if (_param == "pri")
            {
                return bool.Parse(values[0].ToString()) && int.Parse(values[1].ToString()) > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            var _rtnVal = true;
            if (values.Length < 3)
            {
                foreach (var o in values)
                {
                    if (o != DependencyProperty.UnsetValue && !System.Convert.ToBoolean(o))
                    {
                        _rtnVal = _param == "i";
                    }
                    if (o != DependencyProperty.UnsetValue && int.TryParse(o.ToString(), out int i))
                    {
                        if (i >= 999)
                        {
                            _rtnVal = _param == "i";
                        }
                        else if (i > 0)
                        {
                            _rtnVal = _param != "i";
                        }
                    }
                }
            }
            else
            {
                if (values[0].GetType() == typeof(bool) && System.Convert.ToBoolean(values[0]))
                {
                    int.TryParse(values[1].ToString(), out int mto);
                    int.TryParse(values[2].ToString(), out int wo);
                    switch (_param)
                    {
                        case "mto":
                            _rtnVal = mto == 1;
                            break;
                        case "wo":
                            _rtnVal = wo == 1;
                            break;
                        case "Imto":
                        case "Iwo":
                            _rtnVal = mto + wo <= 0;
                            break;
                    }
                }
                else
                {
                    _rtnVal = false;
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
