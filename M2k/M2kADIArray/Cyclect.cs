using System;

namespace M2kClient.M2kADIArray
{
    public class Cyclect
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to CYCLECT
        /// </summary>
        public string TranType { get { return "CYCLECT"; } }

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
        /// Statically set to 'R' for replacing existing quantity, based on the need from the ERP
        /// </summary>
        public char TranOperation { get { return 'R'; } }

        /// <summary>
        /// Field 13
        /// Transaction Quantity
        /// </summary>
        public int TranQuantity { get; set; }

        /// <summary>
        /// Field 14
        /// Location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Field 15
        /// Optional field, Lot Number
        /// </summary>
        public string LotNumber { get; set; }

        #endregion

        /// <summary>
        /// Adjust object constructor
        /// </summary>
        /// <param name="statId">Station ID</param>
        /// <param name="facCode">Facility Code</param>
        /// <param name="ccNbr">Cycle Count Number</param>
        /// <param name="compFlg">Item Completion Flag</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="aCode">Adjust Code</param>
        /// <param name="tranQty">Transaction Quantity</param>
        /// <param name="loc">Location</param>
        /// <param name="lot">Optional: Lot Number</param>
        /// <param name="uom">Optional: Unit of Measure</param>
        public Cyclect(string statId, string facCode, string ccNbr, CompletionFlag compFlg, string partNbr, AdjustCode aCode, int tranQty, string loc, string lot = "", string uom = "")
        {
            StationId = statId;
            FacilityCode = facCode;
            CcNumber = ccNbr;
            IcFlag = compFlg;
            ItemNumber = partNbr;
            ReasonCode = aCode;
            TranQuantity = tranQty;
            Location = loc;
            LotNumber = lot;
            UOM = uom;
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Cycle Count Entry (CYCLECT) ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //Transaction Template
            //1~Transaction Type~2~Station Id~3~Transaction Time~4~Transaction Date~5~Facility~6~Cycle Count Number~8~Item Number~9~UOM~10~Reason Code~12~Transaction Operation~13~Transaction Quantity~14~Location~15~Lot Number~99~COMPLETE


            return !string.IsNullOrEmpty(LotNumber)
                ? $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{CcNumber}~7~{IcFlag}~8~{ItemNumber}~10~{ReasonCode}~12~{TranOperation}~13~{TranQuantity}~14~{Location}~15~{LotNumber}|P~99~COMPLETE"
                : $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{CcNumber}~7~{IcFlag}~8~{ItemNumber}~10~{ReasonCode}~12~{TranOperation}~13~{TranQuantity}~14~{Location}~99~COMPLETE";
        }
    }
}
