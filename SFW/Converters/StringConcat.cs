using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 9-26-18

namespace SFW.Converters
{
    public sealed class StringConcat : IValueConverter, IMultiValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value}*{parameter}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || values.Length == 0)
            {
                var _rtnStr = string.Empty;
                foreach (var o in values)
                {
                    _rtnStr += o.ToString();
                }
                return _rtnStr;
            }
            else
            {
                return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
