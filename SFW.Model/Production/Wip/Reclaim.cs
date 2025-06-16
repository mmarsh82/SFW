namespace SFW.Model.Production.Wip
{
    public class Reclaim : ModelBase
    {
        #region Properties

        private int? qty;
        /// <summary>
        /// Quantity of reclaim for the wip receipt
        /// </summary>
        public string Quantity
        {
            get
            { return qty.ToString(); }
            set
            {
                if (int.TryParse(value, out int _qty))
                {
                    qty = _qty;
                }
                else
                {
                    qty = null;
                }
                OnPropertyChanged(nameof(Quantity));
            }
        }

        /// <summary>
        /// Parent part number for the reclaim transaction
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// Reference information for a reclaim transaction, typically the work order and QIR number
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Assembly Quantity for a reclaim transaction
        /// </summary>
        public decimal ParentAssyQty { get; set; }

        #endregion

        /// <summary>
        /// Scrap Default constructor
        /// </summary>
        public Reclaim()
        { }
    }
}
