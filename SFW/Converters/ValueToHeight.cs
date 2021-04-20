using System;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public sealed class ValueToHeight : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                #region Int Type

                if (value.GetType() == typeof(int))
                {
                    var _stick = false;
                    var _cnt = 0;
                    var _cstHgt = int.TryParse(value.ToString(), out _cnt) && _cnt > 0 ? 30 : 0;
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

                #endregion

                #region String Type

                else if (value.GetType() == typeof(string))
                {
                    if (parameter.ToString().Contains("L"))
                    {
                        if (int.TryParse(parameter.ToString().Replace("L", ""), out int hgt))
                        {
                            return value.ToString().Split(new[] { "\n", "\r" }, StringSplitOptions.None).Length * hgt;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        if (int.TryParse(parameter.ToString(), out int hgt))
                        {
                            return !string.IsNullOrEmpty(value.ToString()) ? hgt : 0;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }

                #endregion

                #region Bool Type

                else if (value.GetType() == typeof(bool))
                {
                    if (int.TryParse(parameter.ToString(), out int hgt))
                    {
                        return (bool)value ? hgt : 0;
                    }
                    else
                    {
                        return 0;
                    }
                }

                #endregion
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }

        #endregion
    }
}
