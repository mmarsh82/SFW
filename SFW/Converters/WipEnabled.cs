using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public sealed class WipEnabled : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var _val in values)
            {
                if (_val.GetType() == typeof(bool) && bool.TryParse(_val.ToString(), out bool b) && !b)
                {
                    return false;
                }
                if (_val.GetType() == typeof(TimerState) && Enum.TryParse(_val.ToString(), out TimerState ts) && ts != TimerState.Sleeping)
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
