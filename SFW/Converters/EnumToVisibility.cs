using SFW.Model.Enumerations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SFW.Converters
{
    public sealed class EnumToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Enum.TryParse(value?.ToString(), out Complete _comp))
            {
                if (string.IsNullOrEmpty(parameter?.ToString()))
                {
                    return _comp == Complete.Y ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    if (parameter.ToString() == "i")
                    {
                        return _comp == Complete.N ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        return _comp.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            else if (value != null && Enum.TryParse(parameter?.ToString(), out M2kClient.AdjustCode _ac))
            {
                return _ac.GetDescription() == value.ToString() ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }
    }
}
