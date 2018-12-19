using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2kClient.M2kADIArray
{
    public class Issue
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

        public override string ToString()
        {
            //First Line of the Transaction:
            //1~Transaction Type~2~Station ID~3~Time~4~Date~5~Facility Code~6~Item Number~7~WorkOrder Number~8~Work Order Sequence~10~Reason Code
            //Second and Subsequent Lines of the Transaction:
            //13~Transaction Quantity~14~Location~15~Lot Numbers
            //13~Transaction Quantity~14~Location~15~Lot Numbers~99~COMPLETE
            //Must meet this format in order to work with M2k

            var _rValue = $"1~{TranType}~2~{StationID}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{PartNbr}~7~{WorkOrderNbr}~8~{Operation}~10~{Reason}";
            

            return null;


        }
    }
}
