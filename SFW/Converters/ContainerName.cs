using SFW.Model.Product;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-3-18

namespace SFW.Converters
{
    public class ContainerNameConverter : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value != null && value != DependencyProperty.UnsetValue && int.TryParse(value.ToString(), out int i)
                        ? $"Container:  {value}      Located in: {SkuContainer.GetContainerLocation(i, App.AppSqlCon)}"
                        : string.Empty;
            }
            catch
            {
                return "Error in load";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
