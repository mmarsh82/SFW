using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2kClient
{
    public class M2kIssueADIArray
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to WIP
        /// </summary>
        public string TranType { get { return "ISSUE"; } }

        /// <summary>
        /// Field 2
        /// Transaction Station ID
        /// </summary>
        public string StationID { get; set; }

        /// <summary>
        /// Field 3
        /// Transaction Time
        /// Statically set to the time of the transaction on a 24 hour clock
        /// </summary>
        public string TranTime { get { return DateTime.Now.ToString("HH:mm"); } }

        /// <summary>
        /// Field 4
        /// Transaction Date
        /// Statically set to date of transaction using MM-dd-yyyy as model
        /// </summary>
        public string TranDate { get { return DateTime.Today.ToString("MM-dd-yyyy"); } }

        /// <summary>
        /// Field 5
        /// Facility Code
        /// </summary>
        public string FacilityCode { get; set; }

        /// <summary>
        /// Field 6
        /// Part number
        /// </summary>
        public string PartNbr { get; set; }

        /// <summary>
        /// Field 7
        /// Component work order number
        /// </summary>
        public string WorkOrderNbr { get; set; }

        /// <summary>
        /// Field 8
        /// Work order operation or sequence
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Field 10
        /// Issue Reason Code
        /// </summary>
        public string Reason { get; set; }

        #endregion
    }
}
