using System;

namespace M2kClient.M2kADIArray
{
    public class Locxfer
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to ISSUE
        /// </summary>
        public string TranType { get { return "LOCXFER"; } }

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
        /// From location in the move
        /// </summary>
        public string FromLoc { get; set; }

        /// <summary>
        /// Field 8
        /// To location in the move
        /// </summary>
        public string ToLoc { get; set; }

        /// <summary>
        /// Field 9
        /// Quantity moved
        /// </summary>
        public int Qty { get; set; }

        /// <summary>
        /// Field 10
        /// Lot number
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// Field 12
        /// Unit of measure
        /// </summary>
        public string Uom { get; set; }

        /// <summary>
        /// Field 19
        /// Free text reference that can be tagged to the move
        /// </summary>
        public string Reference { get; set; }

        #endregion

        /// <summary>
        /// Locxfer object constructor
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="partNbr"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="qty"></param>
        /// <param name="lot"></param>
        /// <param name="reference"></param>
        /// <param name="facCode"></param>
        /// <param name="uom"></param>
        public Locxfer(string stationId, string partNbr, string from, string to, int qty, string lot, string reference, string facCode, string uom = null)
        {
            StationId = stationId;
            PartNbr = partNbr;
            FromLoc = from;
            ToLoc = to;
            Qty = qty;
            Lot = lot;
            Reference = reference;
            FacilityCode = facCode;
            Uom = uom;
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Wip ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //1~Transaction type~2~Station ID~3~Transaction time~4~Transaction date~5~Facility code~6~Part number~7~From location~8~To location~9~Quantity #1~10~Lot #1~9~Quantity #2~10~Lot #2~19~Reference~99~COMPLETE
            //Must meet this format in order to work with M2k

            return !string.IsNullOrEmpty(Lot)
                ? $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{PartNbr}|{FacilityCode}~7~{FromLoc.ToUpper()}~8~{ToLoc.ToUpper()}~9~{Qty}~10~{Lot}|P|{FacilityCode}~19~{Reference}~99~COMPLETE"
                : $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{PartNbr}|{FacilityCode}~7~{FromLoc.ToUpper()}~8~{ToLoc.ToUpper()}~9~{Qty}~12~{Uom}~19~{Reference}~99~COMPLETE";
        }
    }
}
