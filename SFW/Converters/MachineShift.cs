using SFW.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public class MachineShift : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _shifts = Machine.GetMachineShift(value.ToString());
            return string.IsNullOrEmpty(_shifts) ? "None" : _shifts;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "None";
        }

        #endregion
    }
}
