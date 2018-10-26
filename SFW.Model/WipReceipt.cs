using SFW.Model.Enumerations;

//Created by Michael Marsh 10-23-18

namespace SFW.Model
{
    /// <summary>
    /// Wip Receipt Record Object
    /// </summary>
    public class WipReceipt : WorkOrder
    {
        #region Properties

        /// <summary>
        /// Currently logged in user
        /// </summary>
        public string Submitter { get; private set; }

        /// <summary>
        /// Wip receipt record quantity
        /// </summary>
        public int WipQty { get; set; }

        /// <summary>
        /// Sequence completion flag
        /// </summary>
        public Complete SeqComplete { get; set; }

        /// <summary>
        /// Receipt Location for the product
        /// </summary>
        public string ReceiptLocation { get; set; }

        #endregion

        /// <summary>
        /// Wip Receipt Constructor
        /// </summary>
        /// <param name="submitter">Currently logged in user</param>
        public WipReceipt(string submitter)
        {
            
        }
    }
}
