using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace M2kClient
{
    public class M2kWipADIArray
    {
        #region Sub-Class Display Info 

        public class DisplayInfo
        {
            #region Enumerations

            public enum CodeType
            {
                [Description("Stock")]
                S = 0,
                [Description("Inspection")]
                I = 1,
                [Description("General Ledger Account")]
                G = 2,
                [Description("Work Order")]
                W = 3
            }

            #endregion

            #region Properties

            /// <summary>
            /// Field 10
            /// Wip Display Information
            /// </summary>
            public CodeType Code { get; set; }

            /// <summary>
            /// Field 11
            /// Wip Display Reference
            /// Generally used in conjunction with 'STOCK' or 'ASSY'
            /// </summary>
            public string Reference { get; set; }

            /// <summary>
            /// Field 12
            /// Wip Display Quantity
            /// Must balance to the totality of wip quantity
            /// </summary>
            public int Quantity { get; set; }

            #endregion
        }

        #endregion

        #region Sub-Class Component Info 

        public class CompInfo
        {
            #region Properties

            /// <summary>
            /// Field 24
            /// Component Lot Number
            /// </summary>
            public string Lot { get; set; }

            /// <summary>
            /// Field 26
            /// Component work order number
            /// When used in non-sequenced work orders must match the parent work order
            /// </summary>
            public string WorkOrderNbr { get; set; }

            /// <summary>
            /// Field 25
            /// Component part number
            /// </summary>
            public string PartNbr { get; set; }

            /// <summary>
            /// Field 27
            /// </summary>
            public int Quantity { get; set; }

            #endregion
        }

        #endregion

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
        /// Can only be 'Y' or 'N'
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
        public M2kWipADIArray(WorkOrder wo)
        {
            StationID = "HelloWorld"; //manual input
            FacilityCode = "01";
            WorkOrderNbr = wo.OrderNumber;
            QtyReceived = 250; //manual input
            CFlag = CompletionFlag.N; //manual input with a defualt set to N
            Operation = wo.Seq;
            RcptLocation = "G17"; //manual input
            DisplayInfoList = new List<DisplayInfo>
            {
                new DisplayInfo { Code = DisplayInfo.CodeType.S, Quantity = 250, Reference = "STOCK" }
            }; //will need to be passed in as an array
            Lot = "1810-1234"; //manual input or if the value is not passed then retreive it from M2k
            ComponentInfoList = new List<CompInfo>
            {
                new CompInfo { Lot = "1810-4567", WorkOrderNbr = wo.OrderNumber, PartNbr = "1005623", Quantity = 300 }
            }; //will need to be passed in as an array of values and matched to the corresponding components
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns></returns>
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