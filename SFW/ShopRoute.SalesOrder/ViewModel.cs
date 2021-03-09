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
            }
        }

        private Model.Sku _part;
        public Model.Sku Part
        {
            get { return _part; }
            set
            {
                _part = value;
                OnPropertyChanged(nameof(Part));
            }
        }

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
        /// <param name="part">Sku Object</param>
        public ViewModel(Model.SalesOrder salesOrder, Model.Sku part)
        {
            Order = salesOrder;
            Part = part;
        }
    }
}
