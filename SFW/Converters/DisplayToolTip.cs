using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public sealed class DisplayToolTip : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }
            switch (parameter.ToString())
            {
                case "WO":
                    return value.ToString() == "Y" ? "Deviated Part Print." : "Standard Part Print.";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        #endregion
    }
}
