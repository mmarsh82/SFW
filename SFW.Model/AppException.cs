using System;
using System.Diagnostics;

namespace SFW.Model
{
    public class AppException : ModelBase
    {
        #region Properties

        public string Message { get; set; }
        public int LineNumber { get; set; }
        public string Method { get; set; }
        public string Class { get; set; }
        public DateTime Occurance { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AppException()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="ex"></param>
        public AppException(Exception ex)
        {
            Message = ex.Message;
            var _exFrame = new StackTrace(ex, true).GetFrame(0);
            if (_exFrame != null)
            {
                Method = _exFrame.GetMethod()?.Name ?? "Unknown Method";
                LineNumber = _exFrame.GetFileLineNumber();
                Class = ex.TargetSite.DeclaringType?.FullName ?? "Unknown Class";
                Occurance = DateTime.Now;
            }
        }
    }
}
