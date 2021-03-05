using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.ShopRoute.SalesOrder
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private Model.SalesOrder _order;
        public Model.SalesOrder Order
        {
            get { return _order; }
            set
            {
                _order = value;
                OnPropertyChanged(nameof(Order));
                Part = new Model.Sku();
                OnPropertyChanged(nameof(Part));
            }
        }
        public Model.Sku Part { get; set; }

        #endregion


        /// <summary>
        /// Shop Route Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (Order == null)
            {
                Order = new Model.SalesOrder();
            }
        }

        /// <summary>
        /// Shop Route Constructor for loading work orders
        /// </summary>
        /// <param name="salesOrder">Sales Order Object</param>
        public ViewModel(Model.SalesOrder salesOrder)
        {
            Order = salesOrder;
        }
    }
}
