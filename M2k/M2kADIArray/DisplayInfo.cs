namespace M2kClient.M2kADIArray
{
    /// <summary>
    /// Display Info Object
    /// </summary>
    public class DisplayInfo
    {
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
}
