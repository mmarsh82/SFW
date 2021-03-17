using System;
using System.Windows.Data;

namespace SFW.Converters
{
    public sealed class CountToHeight : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var _stick = false;
            var _cnt = 0;
            var _cstHgt = value != null && int.TryParse(value.ToString(), out _cnt) && _cnt > 0 ? 30 : 0;
            if (parameter != null)
            {
                if (parameter.ToString().Contains("L"))
                {
                    _cstHgt = int.TryParse(parameter.ToString().Replace("L", ""), out int i) ? i : 0;
                }
                else if (parameter.ToString().Contains("S"))
                {
                    _stick = true;
                    _cstHgt = int.TryParse(parameter.ToString().Replace("S", ""), out int i) && _cnt > 0 ? i : 0;
                }
            }
            return !_stick ? _cnt * _cstHgt : _cstHgt;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }

        #endregion
    }
}
