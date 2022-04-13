using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public class BoolToInt : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool b))
            {
                return b ? 1 : 3;
            }
            else
            {
                return 1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1;
        }

        #endregion
    }
}
