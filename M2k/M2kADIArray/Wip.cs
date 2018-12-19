using SFW.Model;
using System;
using System.Collections.Generic;

namespace M2kClient.M2kADIArray
{
    public class Wip
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to WIP
        /// </summary>
        public string TranType { get { return "WIP"; } }

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
        /// Parent Work Order Number
        /// </summary>
        public string WorkOrderNbr { get; set; }

        /// <summary>
        /// Field 7
        /// Parent Quantity Received
        /// </summary>
        public int QtyReceived { get; set; }

        /// <summary>
        /// Field 8
        /// Completion Flag
        /// </summary>
        public CompletionFlag CFlag { get; set; }

        /// <summary>
        /// Field 9
        /// Work order operation or sequence
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Field 14
        /// Parent Receipt Location
        /// To be treated as a 'To' location not a 'From' location
        /// </summary>
        public string RcptLocation { get; set; }

        /// <summary>
        /// Fields 10,11,12
        /// List of each display information object
        /// </summary>
        public List<DisplayInfo> DisplayInfoList { get; set; }

        /// <summary>
        /// Fields 24,25,26,27
        /// List of all child component objects and their information
        /// </summary>
        public List<CompInfo> ComponentInfoList { get; set; }

        /// <summary>
        /// Field 15
        /// Parent lot number
        /// When none exists one will be assigned to the transaction
        /// </summary>
        public string Lot { get; set; }

        #endregion

        /// <summary>
        /// M2k Wip ADI Array overloaded constructor
        /// Maps a work order object to a M2k Wip ADI Array object
        /// </summary>
        /// <param name="wo"></param>
        public Wip(WipReceipt wipRecord)
        {
            //TODO: need to remove the hard coded work order and bind the properties to the work order object
            StationID = wipRecord.Submitter;
            FacilityCode = "01";
            WorkOrderNbr = wipRecord.WipWorkOrder.OrderNumber;
            QtyReceived = Convert.ToInt32(wipRecord.WipQty);
            CFlag = Enum.TryParse(wipRecord.SeqComplete.ToString().ToUpper(), out CompletionFlag cFlag) ? cFlag : CompletionFlag.N;
            Operation = wipRecord.WipWorkOrder.Seq;
            RcptLocation = wipRecord.ReceiptLocation;
            //TODO: need to split this into single and mutli wip
            DisplayInfoList = new List<DisplayInfo>
            {
                new DisplayInfo { Code = CodeType.S, Quantity = QtyReceived, Reference = "STOCK" }
            }; //will need to be passed in as an array
            Lot = wipRecord.WipLot.LotNumber;
            ComponentInfoList = new List<CompInfo>
            {
                new CompInfo { Lot = "1811-1516" /*Will need to be manually assigned*/, WorkOrderNbr = wipRecord.WipWorkOrder.OrderNumber, PartNbr = "1005623" /*passed from the work order components*/, Quantity = 300 /*Manual input must handle multiple inputs*/ }
            }; //will need to be passed in as an array of values and matched to the corresponding components
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Wip ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //First Line of the Transaction:
            //1~Transaction type~2~Station ID~3~Transaction time~4~Transaction date~5~FacilityCode~6~Work order number~7~Quantity received~8~Complete Flag~9~Operation~14~Receipt Location
            //Second and Subsequent Lines of the Transaction:
            //10~Disp code~11~Disp reference~12~Disp quantity
            //15~Lot number
            //24~Component #1 lot number~26~Component #1 work order~25~Component #1 item number~27~Component # 1 lot quantity
            //24~Component #2 lot number~26~Component #2 work order~25~Component #2 item number~27~Component # 2 lot quantity
            //99~COMPLETE
            //Must meet this format in order to work with M2k

            var _rValue = $"1~{TranType}~2~{StationID}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{WorkOrderNbr}~7~{QtyReceived}~8~{CFlag}~9~{Operation}~14~{RcptLocation}";
            foreach (var disp in DisplayInfoList)
            {
                _rValue += $"\n10~{disp.Code}~11~{disp.Reference}~12~{disp.Quantity}";
            }
            _rValue += $"\n15~{Lot}|P";
            foreach (var comp in ComponentInfoList)
            {
                _rValue += $"\n24~{comp.Lot}|P~26~{comp.WorkOrderNbr}~25~{comp.PartNbr}~27~{comp.Quantity}";
            }
            _rValue += $"\n99~COMPLETE";

            return _rValue;
        }
    }
}