using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SFW.Converters
{
    public class PasswordConcat : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pArray = new PasswordBox[3];
            if (values.Count() > 3)
            {
                return null;
            }
            var counter = 0;
            foreach (object v in values)
            {
                if (v != null && v != DependencyProperty.UnsetValue && v.GetType() == typeof(PasswordBox))
                {
                    pArray[counter] = (PasswordBox)v;
                    counter++;
                }
            }
            return pArray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
