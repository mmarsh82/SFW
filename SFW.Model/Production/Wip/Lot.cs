using System.Collections.ObjectModel;

namespace SFW.Model.Production.Wip
{
    public class Lot : ModelBase
    {
        #region Properties

        private string _id;
        public string ID
        {
            get
            { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }
        public string ProductId { get; set; }

        private bool _valid;
        public bool Valid
        {
            get
            { return _valid; }
            set
            {
                _valid = value;
                OnPropertyChanged(nameof(Valid));
            }
        }

        private bool _validAmt;
        public bool ValidAmount
        {
            get
            { return _validAmt; }
            set
            {
                _validAmt = value;
                OnPropertyChanged(nameof(ValidAmount));
            }
        }

        private int? _qty;
        public string Quantity
        {
            get
            { return _qty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _qty = i;
                }
                else
                {
                    _qty = null;
                }
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Stock));
            }
        }
        public bool QuantityLocked { get; set; }

        private string _loc;
        public string Location
        {
            get
            { return _loc; }
            set
            {
                _loc = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        private bool _status;
        public bool Status
        {
            get
            { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public int SystemStock { get; set; }
        public int Stock
        {
            get
            { return SystemStock - (int.TryParse(Quantity, out int i) ? i : 0); }
        }
        public int RequiredQuantity { get; set; }

        private string _uom;
        public string LotUom
        {
            get
            { return _uom; }
            set
            {
                _uom = value;
                OnPropertyChanged(nameof(LotUom));
            }
        }

        private Enumerations.Complete _scrap;
        public Enumerations.Complete HasScrap
        {
            get
            { return _scrap; }
            set
            {
                if (ScrapCollection == null)
                {
                    ScrapCollection = new ObservableCollection<Scrap>();
                }
                _scrap = value;
                switch (value)
                {
                    case Enumerations.Complete.N:
                        ScrapCollection.Clear();
                        break;
                    case Enumerations.Complete.Y:
                        ScrapCollection.Add(new Scrap("0", ID, OrderId, Product.Lot.GetSkuNumber(ID)));
                        break;
                }
                OnPropertyChanged(nameof(HasScrap));
                OnPropertyChanged(nameof(ScrapCollection));
            }
        }
        public ObservableCollection<Scrap> ScrapCollection { get; set; }
        public string OrderId { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Lot()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="orderId">Work Order ID</param>
        /// <param name="productId">Product ID</param>
        public Lot(string orderId, string productId)
        {
            OrderId = orderId;
            ProductId = productId;
        }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="rqdQty">Required Quantity</param>
        /// <param name="orderId">Work Order ID</param>
        /// <param name="productId">Product ID</param>
        public Lot(int rqdQty, string orderId, string productId)
        {
            RequiredQuantity = rqdQty;
            QuantityLocked = false;
            OrderId = orderId;
            ProductId = productId;
        }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="id">Lot number</param>
        /// <param name="valid">Is the lot number validated</param>
        /// <param name="qty">Lot quantity</param>
        /// <param name="loc">Lot location</param>
        /// <param name="rqdQty">Required quantity</param>
        /// <param name="orderId">Work Order ID</param>
        /// <param name="productId">Product ID</param>
        public Lot(string id, bool valid, int qty, string loc, int rqdQty, string orderId, string productId)
        {
            ID = id;
            SystemStock = Product.Lot.GetOnHandQuantity(id);
            Valid = valid;
            Quantity = qty.ToString();
            Location = loc;
            QuantityLocked = false;
            RequiredQuantity = rqdQty;
            LotUom = Product.Lot.GetUom(id);
            OrderId = orderId;
            ProductId = productId;
        }
    }
}
