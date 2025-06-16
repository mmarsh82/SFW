namespace M2kClient
{
    public class Transaction
    {
        #region Properties

        public int Quantity { get; set; }
        public string Location { get; set; }
        public string LotNumber { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Transaction()
        { }

        /// <summary>
        /// Overridded Constructor
        /// </summary>
        /// <param name="qty">Quantity of the transaction</param>
        /// <param name="loc">Location for the transaction</param>
        /// <param name="lot">Lot identification for the transaction</param>
        public Transaction(int qty, string loc, string lot)
        {
            Quantity = qty;
            Location = loc;
            LotNumber = lot;
        }
    }
}
