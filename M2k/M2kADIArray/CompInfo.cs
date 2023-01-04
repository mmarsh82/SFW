namespace M2kClient.M2kADIArray
{
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

        /// <summary>
        /// Field 70
        /// </summary>
        public string IssueLoc { get; set; }

        #endregion
    }
}
