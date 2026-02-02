namespace SFW.Model.Production.Wip
{
    public class Scrap : ModelBase
    {
        #region Properties

        public string ID { get; set; }
        public string LotId { get; set; }
        public string OrderId { get; set; }
        public string ProductId { get; set; }

        private int? qty;
        /// <summary>
        /// Quantity of scrap for the wip receipt
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
                OnPropertyChanged(nameof(Valid));
            }
        }

        private int? _ref;
        /// <summary>
        /// Reference information for scrapping the material
        /// </summary>
        public string Reference
        {
            get
            { return _ref.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _ref = i;
                }
                else
                {
                    _ref = null;
                }
                OnPropertyChanged(nameof(Reference));
                OnPropertyChanged(nameof(Valid));
            }
        }
        public bool Valid
        {
            get
            {
                var _product = ScrapType == 'P' ? ProductId : LotId;
                return !string.IsNullOrEmpty(Quantity)
                        && !string.IsNullOrEmpty(Reference)
                        && Quality.QmsForm.IsValid(int.Parse(Reference), OrderId, _product, ScrapType);
            }
        }
        public char ScrapType { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public Scrap()
        { }

        /// <summary>
        /// Overridded constructor
        /// </summary>
        /// <param name="id">Scrap ID</param>
        /// <param name="lotId">Lot number</param>
        /// <param name="orderId">Work Order ID</param>
        public Scrap(string id, string lotId, string orderId, string prodId, char scrapType)
        {
            ID = id;
            LotId = lotId;
            OrderId = orderId;
            ProductId = prodId;
            ScrapType = scrapType;
        }
    }
}
