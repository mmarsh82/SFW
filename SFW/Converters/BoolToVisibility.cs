using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-1-18

namespace SFW.Converters
{
    public class BoolToVisibility : IValueConverter, IMultiValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().Contains("*"))
            {
                value = value.ToString().Split('*')[0];
            }
            if (int.TryParse(value.ToString(), out int i))
            {
                value = System.Convert.ToBoolean(i);
            }
            if (bool.TryParse(value.ToString(), out bool bResult))
            {
                return parameter?.ToString() == "i"
                    ? bResult ? Visibility.Collapsed : Visibility.Visible
                    : bResult ? Visibility.Visible : Visibility.Collapsed;
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

        #endregion

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() > 1)
            {
                if (parameter?.ToString().Contains('*') == true)
                {
                    return ConvertWithAnd(values, parameter.ToString());
                }
                else if (parameter?.ToString().Contains('|') == true)
                {
                    return ConvertWithOr(values, parameter.ToString());
                }
            }

            if (parameter?.ToString() == "SCR")
            {
                var _boolList = new List<bool>();
                foreach (var _val in values)
                {
                    if (_val.GetType() == typeof(bool))
                    {
                        _boolList.Add((bool)_val);
                    }
                    else if (_val.GetType() == typeof(int))
                    {
                        _boolList.Add((int)_val > 0);
                    }
                }
                return (_boolList[0] && _boolList[1]) || !_boolList[2] ? Visibility.Visible : Visibility.Collapsed; 
            }

            if (parameter?.ToString() == "NCR")
            {
                var _type = values[0];
                var _new = bool.TryParse(values[2].ToString(), out bool n) ? n : false;
                var _sched = bool.TryParse(values[1].ToString(), out bool s) ? s : false;
                switch (_type)
                {
                    case "Box":
                        return _new && !_sched ? Visibility.Visible : Visibility.Collapsed;
                    case "Block":
                        return (_sched && _new) || (_sched && !_new) || (!_sched && !_new) ? Visibility.Visible : Visibility.Collapsed;
                    case "Button":
                        return _sched || _new ? Visibility.Visible : Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
            if (int.TryParse(values[1].ToString(), out int nRef) && bool.TryParse(values[0].ToString(), out bool qRef))
            {
                return qRef && nRef > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (values[0] != null)
            {
                switch (values[0])
                {
                    case "Arlington":
                        values[0] = 0;
                        break;
                    case "Wahpeton":
                        values[0] = 1;
                        break;
                }
            }
            else
            {
                values[0] = false;
            }
            var _param = parameter?.ToString();
            if (_param == "iDev")
            {
                return bool.Parse(values[0].ToString()) && values[1].ToString() == "N" ? Visibility.Visible : Visibility.Collapsed;
            }
            if (_param == "dev")
            {
                return bool.Parse(values[0].ToString()) && values[1].ToString() == "Y" ? Visibility.Visible : Visibility.Collapsed;
            }
            if (_param == "pri")
            {
                return bool.Parse(values[0].ToString()) && int.Parse(values[1].ToString()) > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            var _rtnVal = true;
            if (values.Length < 3)
            {
                foreach (var o in values)
                {
                    if (o != DependencyProperty.UnsetValue && !System.Convert.ToBoolean(o))
                    {
                        _rtnVal = _param == "i";
                    }
                    if (o != DependencyProperty.UnsetValue && int.TryParse(o.ToString(), out int i))
                    {
                        if (i >= 999)
                        {
                            _rtnVal = _param == "i";
                        }
                        else if (i > 0)
                        {
                            _rtnVal = _param != "i";
                        }
                    }
                }
            }
            else
            {
                if (values[0].GetType() == typeof(bool) && System.Convert.ToBoolean(values[0]))
                {
                    int.TryParse(values[1].ToString(), out int mto);
                    int.TryParse(values[2].ToString(), out int wo);
                    switch (_param)
                    {
                        case "mto":
                            _rtnVal = mto == 1;
                            break;
                        case "wo":
                            _rtnVal = wo == 1;
                            break;
                        case "Imto":
                        case "Iwo":
                            _rtnVal = mto + wo <= 0;
                            break;
                    }
                }
                else
                {
                    _rtnVal = false;
                }
            }
            return _rtnVal ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Convert logic when the parameter contains a AND modifier
        /// </summary>
        /// <param name="values">array of values</param>
        /// <param name="parameter">Modifier string</param>
        /// <returns></returns>
        public Visibility ConvertWithAnd(object[] values, string parameter)
        {
            var _counter = 0;
            var _rtnBool = true;
            foreach (var _arg in parameter.ToString().Split('*'))
            {
                var _intVal = int.TryParse(_arg, out int i) && i > 0;
                if (!bool.TryParse(values[_counter].ToString(), out bool _boolVal))
                {
                    _boolVal = int.TryParse(values[_counter].ToString(), out int l) && l > 0;
                }
                _rtnBool = _boolVal == _intVal;
                if (!_rtnBool)
                {
                    return Visibility.Collapsed;
                }
                _counter++;
            }
            return Visibility.Visible;
        }

        /// <summary>
        /// Convert logic when the parameter contains a OR modifier
        /// </summary>
        /// <param name="values">array of values</param>
        /// <param name="parameter">Modifier string</param>
        /// <returns></returns>
        public Visibility ConvertWithOr(object[] values, string parameter)
        {
            var _counter = 0;
            var _rtnList = new List<bool>();
            foreach (var _arg in parameter.Split('|'))
            {
                var _intVal = int.TryParse(_arg, out int i) && i > 0;
                _rtnList.Add(bool.TryParse(values[_counter].ToString(), out bool b) && _intVal == b);
                _counter++;
            }
            return _rtnList.Count(o => o) > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
