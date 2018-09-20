using SFW.Model;

namespace SFW.ShopRoute
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private WorkOrder shopOrder;
        public WorkOrder ShopOrder
        {
            get { return shopOrder; }
            set { shopOrder = value; OnPropertyChanged(nameof(ShopOrder)); OnPropertyChanged(nameof(FqSalesOrder)); }
        }

        public string FqSalesOrder
        {
            get { return $"{ShopOrder?.SalesOrder?.SalesNumber}*{ShopOrder?.SalesOrder?.LineNumber}"; }
        }

        public bool ViewLotList
        {
            get
            { return App.SiteNumber != 0; }
            set
            {
                value = App.SiteNumber != 0; OnPropertyChanged(nameof(ViewLotList));
            }
        }

        #endregion

        /// <summary>
        /// Shop Route Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ShopOrder == null)
            {
                ShopOrder = new WorkOrder();
            }
        }

        /// <summary>
        /// Shop Route Constructor for loading work orders
        /// </summary>
        /// <param name="workOrder">Work Order Object</param>
        public ViewModel(WorkOrder workOrder)
        {
            ShopOrder = workOrder;
        }
    }
}
