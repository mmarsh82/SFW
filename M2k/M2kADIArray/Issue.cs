using System;
using System.Collections.Generic;

namespace M2kClient.M2kADIArray
{
    public class Issue
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to ISSUE
        /// </summary>
        public string TranType { get { return "ISSUE"; } }

        /// <summary>
        /// Field 2
        /// Transaction Station ID
        /// </summary>
        public string StationId { get; set; }

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

        /// <summary>
        /// Fields 13 - 15
        /// List of tranasactions to post during Issue validation
        /// </summary>
        public List<Transaction> TranList { get; set; }

        #endregion

        /// <summary>
        /// Issue object constructor
        /// </summary>
        /// <param name="stationId">Station ID, typically user domain name</param>
        /// <param name="facCode">Facility code</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="rsn">Issue reason</param>
        /// <param name="tranList">Transaction object list</param>
        /// <param name="op">Optional: Operation also know as sequence number</param>
        public Issue(string stationId, string facCode, string partNbr, string woNbr, string rsn, List<Transaction> tranList, string op = "")
        {
            StationId = stationId;
            FacilityCode = facCode;
            PartNbr = partNbr;
            WorkOrderNbr = woNbr;
            Operation = op;
            Reason = rsn;
            TranList = tranList;
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Issue ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //First Line of the Transaction:
            //1~Transaction Type~2~Station ID~3~Time~4~Date~5~Facility Code~6~Item Number~7~WorkOrder Number~8~Work Order Sequence~10~Reason Code
            //Second and Subsequent Lines of the Transaction:
            //13~Transaction Quantity~14~Location~15~Lot Number
            //13~Transaction Quantity~14~Location~15~Lot Number~99~COMPLETE
            //Must meet this format in order to work with M2k

            var _rValue = !string.IsNullOrEmpty(Operation)
                ? $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{PartNbr}|{FacilityCode}~7~{WorkOrderNbr}~8~{Operation}~10~{Reason}"
                : $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{PartNbr}|{FacilityCode}~7~{WorkOrderNbr}~10~{Reason}";
            foreach (var t in TranList)
            {
                _rValue += !string.IsNullOrEmpty(t.LotNumber)
                    ? $"\n13~{t.Quantity}~14~{t.Location}~15~{t.LotNumber}|P|{FacilityCode}"
                    : $"\n13~{t.Quantity}~14~{t.Location}";
            }
            _rValue += "~99~COMPLETE";
            return _rValue;
        }
    }
}
