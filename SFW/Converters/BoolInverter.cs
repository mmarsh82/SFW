using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public class BoolInverter : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _bVal = bool.TryParse(value.ToString(), out bool b) ? b : false;
            return !_bVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }

        #endregion
    }
}
