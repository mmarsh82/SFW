using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Model
{
    public class PressReport : ModelBase
    {
        #region Properties

        public int? ReportID { get; set; }
        public int SlatTransfer { get; set; }
        public int RollLength { get; set; }
        public List<Press_ShiftReport> ShiftReportList { get; set; }

        #endregion

        public PressReport()
        {
            ShiftReportList = new List<Press_ShiftReport>();
        }
    }
}
