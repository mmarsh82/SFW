using SFW.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace SFW.Converters
{
    public class WorkCenterNameConverter : IValueConverter
    {
        #region Properties

        public List<Machine> WCList { get; private set; }

        #endregion

        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _mNbr = value.ToString();
            var _name = WCList.Find(o => o.MachineNumber == _mNbr).Name;
            return $"{_name} ({_mNbr})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion

        /// <summary>
        /// Work Center Name Converter Default Constructor
        /// </summary>
        /// <param name="workOrderList"></param>
        public WorkCenterNameConverter(List<Machine> workOrderList)
        {
            WCList = workOrderList;
        }
    }
}
