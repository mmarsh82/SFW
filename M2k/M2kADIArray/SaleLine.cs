namespace M2kClient.M2kADIArray
{
    public class SaleLine
    {
        #region Properties

        /// <summary>
        /// Field 10
        /// Sales Order Line Number
        /// Second part of the SOD file key
        /// </summary>
        public int SaleOrderLineNumber { get; set; }

        /// <summary>
        /// Field 11
        /// Part Number
        /// Product ID for the sale
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Field 14
        /// Lot Numbers
        /// Lot number for the product shipment
        /// </summary>
        public string LotNumber { get; set; }

        /// <summary>
        /// Field 15
        /// Lot Quantity
        /// </summary>
        public int LotQuantity { get; set; }

        /// <summary>
        /// Field 16
        /// Lot Location
        /// </summary>
        public string LotLocation { get; set; }

        /// <summary>
        /// Field 17
        /// Location
        /// Optional field, if non lot traceable this field will be used for the location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Field 18
        /// Location Quantity
        /// Optional field, if non lot traceable this field will be used for the location quantity
        /// </summary>
        public int LocationQuantity { get; set; }

        #endregion

        /// <summary>
        /// Sale Line object constructor
        /// </summary>
        /// <param name="soLine">Sales order line number</param>
        /// <param name="partNbr">Line product number</param>
        /// <param name="lotNbr">Optional:Product lot number, leave empty if non-lot</param>
        /// <param name="qty">Line quantity</param>
        /// <param name="loc">Product location</param>
        public SaleLine(int soLine, string partNbr, string lotNbr, int qty, string loc)
        {
            SaleOrderLineNumber = soLine;
            PartNumber = partNbr;
            if (!string.IsNullOrEmpty(lotNbr))
            {
                LotNumber = lotNbr;
                LotLocation = loc;
                LotQuantity = qty;
            }
            else
            {
                Location = loc;
                LocationQuantity = qty;
            }
        }

        /// <summary>
        /// Method Override
        /// Takes the object and deliminates it along with adding in the referenced field tag numbers
        /// </summary>
        /// <returns>Standard Sale Line object (SaleLine) ADI string needed for the BTI to read</returns>
        public override string ToString()
        {
            //Transaction Template
            //Lot traceable
            //10~SO Line Nbr 1~11~Part Nbr~14~Lot Number~15~Lot Qty~16~Lot Location
            //Non-lot traceable
            //10~SO Line Nbr 2~11~Part Nbr~17~Location~18~Location Quantity

            return !string.IsNullOrEmpty(LotNumber)
                ? $"10~{SaleOrderLineNumber}~11~{PartNumber}~14~{LotNumber}~15~{LotQuantity}~16~{LotLocation}"
                : $"10~{SaleOrderLineNumber}~11~{PartNumber}~17~{Location}~18~{LocationQuantity}";
        }
    }
}
