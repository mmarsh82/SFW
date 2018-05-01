using System;

namespace SFW.Model.Helpers
{
    public sealed class M2kConverters
    {
        public static DateTime GetTimeStamp(double date, double time)
        {
            var _tempDate = new DateTime(1967, 12, 31).AddDays(date).AddSeconds(time);
            return _tempDate;
        }

        public static DateTime GetTimeStamp(int date, int time)
        {
            var _tempDate = new DateTime(1967, 12, 31).AddDays(date).AddSeconds(time);
            return _tempDate;
        }
    }
}
