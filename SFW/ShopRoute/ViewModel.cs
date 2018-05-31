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

        #endregion

        public ViewModel()
        {
            if (ShopOrder == null)
            {
                ShopOrder = new WorkOrder();
            }
        }

        public ViewModel(WorkOrder workOrder)
        {
            ShopOrder = workOrder;
        }
    }
}
