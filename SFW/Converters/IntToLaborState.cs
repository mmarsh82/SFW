using SFW.Enumerations;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public sealed class IntToLaborState : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int i) && i >= 0)
            {
                if (Enum.TryParse(i.ToString(), out QueState ls))
                {
                    return ls.ToString();
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        #endregion
    }
}
