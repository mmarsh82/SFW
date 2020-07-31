using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Data;

namespace SFW.Converters
{
    public class StringToBool : IValueConverter
    {
        #region Static Properties

        private static IReadOnlyDictionary<string, bool> _machExIROD;
        public static IReadOnlyDictionary<string, bool> MachineExpanderIROD
        { 
            get
            { return _machExIROD; }
            set
            {
                _machExIROD = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(_machExIROD)));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null || string.IsNullOrEmpty(value.ToString()) || !MachineExpanderIROD.ContainsKey(value.ToString()))
            {
                return false;
            }
            else
            {
                return MachineExpanderIROD.First(o => o.Key == value.ToString()).Value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }

        #endregion
    }
}
