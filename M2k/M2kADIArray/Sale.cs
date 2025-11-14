using System;
using System.Collections.Generic;

namespace M2kClient.M2kADIArray
{
    public class Sale
    {
        #region Properties

        /// <summary>
        /// Field 1
        /// Transaction Type
        /// Statically set to ADJUST
        /// </summary>
        public string TranType { get { return "SALE"; } }

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
        /// Sales Order Number
        /// Comes from the SOH file
        /// </summary>
        public string SaleOrderNumber { get; set; }

        /// <summary>
        /// Field 7
        /// Shipment Number
        /// Number assigned by the SHIPLISTS function
        /// </summary>
        public int ShipmentNbr { get; set; }

        /// <summary>
        /// List of Sales Order Line items in ADI format
        /// </summary>
        public IList<SaleLine> SaleLineList { get; set; }

        /// <summary>
        /// Field 42
        /// Package Number
        /// </summary>
        public int PackageNumber { get; set; }

        /// <summary>
        /// Field 43
        /// Cill of Lading Number
        /// </summary>
        public string BolNumber { get; set; }

        /// <summary>
        /// Field 44
        /// Carrier
        /// </summary>
        public string Carrier { get; set; }

        /// <summary>
        /// Field 45
        /// Ship Method
        /// </summary>
        public Method ShipMethod { get; set; }

        /// <summary>
        /// Field 47
        /// Carrier Ship Id
        /// </summary>
        public string CarrierShipId { get; set; }

        /// <summary>
        /// Field 50
        /// Shipping Notes
        /// </summary>
        public string Notes { get; set; }

        #endregion

        /// <summary>
        /// Sale object constructor
        /// </summary>
        /// <param name="statId">Station ID</param>
        /// <param name="facCode">Facility Code</param>
        /// <param name="soNbr">Sales order number</param>
        /// <param name="shipNbr">Shipment number</param>
        /// <param name="lineItems">Sales order line items</param>
        /// <param name="pkgNbr">Package number</param>
        /// <param name="bolNbr">Bill of lading number</param>
        /// <param name="carrier">Shipment carrier</param>
        /// <param name="shipMthd">Shipment method</param>
        /// <param name="carShipId">Shiment carrier ID</param>
        /// <param name="notes">And sales order shipment notes</param>
        public Sale(string statId, string facCode, string soNbr, int shipNbr, List<SaleLine> lineItems, int pkgNbr, string bolNbr, string carrier, Method shipMthd, string carShipId, string notes)
        {
            StationId = statId;
            FacilityCode = facCode;
            SaleOrderNumber = soNbr;
            ShipmentNbr = shipNbr;
            SaleLineList = lineItems;
            PackageNumber = pkgNbr;
            BolNumber = bolNbr;
            Carrier = carrier;
            ShipMethod = shipMthd;
            CarrierShipId = carShipId;
            Notes = notes;
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Sale (Sales Order Shipments) ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //Transaction Template
            //1~Trans Type~2~Station Id~3~Time Created~4~Date Created~5~Facility Code~6~Sales Order Number~7~Shipment Nbr
            //Add Sales Order Line objects here
            //42~Package Nbr~43~Bill of lading Nbr~44~Carrier~45~Ship Method~47~Carrier Ship Id
            //99~COMPLETE
            //Must meet this format in order to work with M2k

            var _rtnVal = $"1~{TranType}~2~{StationId}~3~{TranTime}~4~{TranDate}~5~{FacilityCode}~6~{SaleOrderNumber}~7~{ShipmentNbr}";

            foreach (var _line in SaleLineList)
            {
                _rtnVal = $"{_rtnVal}\n{_line}";
            }

            _rtnVal += $"\n42~{PackageNumber}~43~{BolNumber}~44~{Carrier}~45~{ShipMethod}~47~{CarrierShipId}";
            return _rtnVal + "\n99~COMPLETE";
        }
    }
}
