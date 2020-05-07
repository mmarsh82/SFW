using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2kClient.M2kADIArray
{
    public class Cyclect
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to ADJUST
        /// </summary>
        public string TranType { get { return "ADJUST"; } }

        /// <summary>
        /// Field 2
        /// Transaction Station ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// Field 3
        /// Transaction Time
        /// Typically set to the time of the transaction on a 24 hour clock
        /// </summary>
        public string TranTime { get { return DateTime.Now.ToString("HH:mm"); } }

        /// <summary>
        /// Field 4
        /// Transaction Date
        /// Typically set to DateTime.Today but could vary based on over night shift hours
        /// Transaction must use the MM-dd-yyyy format
        /// </summary>
        public string TranDate { get { return DateTime.Now.ToString("MM-dd-yyyy"); } }

        /// <summary>
        /// Field 5
        /// Facility Code
        /// </summary>
        public string FacilityCode { get; set; }

        /// <summary>
        /// Field 6
        /// Cycle Count Number
        /// One per transaction and is treated as the UID
        /// </summary>
        public string CcNumber { get; set; }

        /// <summary>
        /// Field 7
        /// Item Completion Flag
        /// </summary>
        public CompletionFlag IcFlag { get; set; }

        /// <summary>
        /// Field 8
        /// Item Number
        /// Optional field, only one can be sent per transaction, and must be a valid part number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Field 9
        /// Unit of Measure
        /// Optional field, can be filled out if there is a conversion that exists
        /// </summary>
        public string UOM { get; set; }

        /// <summary>
        /// Field 10
        /// Reason Code
        /// </summary>
        public AdjustCode ReasonCode { get; set; }

        /// <summary>
        /// Field 12
        /// Transaction Operation
        /// Statically set to R
        /// </summary>
        public char TranOp { get { return 'R'; } }

        #endregion
    }
}
