using SFW.Helpers;
using SFW.Model.Product;
using System;
using System.Windows.Input;

namespace SFW.ShopRoute.SalesOrder
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private Model.Sales.SalesOrder _order;
        public Model.Sales.SalesOrder Order
        {
            get { return _order; }
            set
            {
                _order = value;
                if (!string.IsNullOrEmpty(value.SalesNumber))
                {
                    Order.LineList = Model.Sales.SalesOrder.GetLineList(value.SalesNumber);
                }
                OnPropertyChanged(nameof(Order));
                OnPropertyChanged(nameof(CanAccept));
                OnPropertyChanged(nameof(CanHold));
                OnPropertyChanged(nameof(CanPending));
                LastSchedInput = Order.LastSchedDate;
            }
        }

        private Sku _part;
        public Sku Part
        {
            get { return _part; }
            set
            {
                _part = value;
                OnPropertyChanged(nameof(Part));
            }
        }

        private DateTime? _lastSched;
        public DateTime? LastSchedInput
        {
            get { return _lastSched; }
            set
            {
                _lastSched = value;
                OnPropertyChanged(nameof(LastSchedInput));
            }
        }

        public bool CanAccept { get { return Order.CreditStatus != "A"; } }
        public bool CanHold { get { return Order.CreditStatus != "H"; } }
        public bool CanPending { get { return Order.CreditStatus != "W"; } }

        RelayCommand _arUpdate;
        RelayCommand _commit;

        #endregion

        /// <summary>
        /// Shop Route Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (Order == null)
            {
                Order = new Model.Sales.SalesOrder();
            }
        }

        /// <summary>
        /// Shop Route Constructor for loading work orders
        /// </summary>
        /// <param name="salesOrder">Sales Order Object</param>
        /// <param name="part">Sku Object</param>
        public ViewModel(Model.Sales.SalesOrder salesOrder, Sku part)
        {
            Order = salesOrder;
            Part = part;
        }

        #region Accounts Receivable Update ICommand

        public ICommand ARUpdateICommand
        {
            get
            {
                if (_arUpdate == null)
                {
                    _arUpdate = new RelayCommand(ARUpdateExecute, ARUpdateCanExecute);
                }
                return _arUpdate;
            }
        }

        private void ARUpdateExecute(object parameter)
        {
            
        }
        private bool ARUpdateCanExecute(object parameter) => true;

        #endregion

        #region Last Actual Commit ICommand

        public ICommand CommitICommand
        {
            get
            {
                if (_commit == null)
                {
                    _commit = new RelayCommand(CommitExecute);
                }
                return _commit;
            }
        }

        private void CommitExecute(object parameter)
        {
            if (DateTime.TryParse(LastSchedInput.ToString(), out DateTime _dt))
            {
                var _dateVal = (_dt - Convert.ToDateTime("1967/12/31")).Days;
                M2kClient.M2kCommand.EditRecord("SOH", Order.SalesNumber, 58, _dateVal.ToString(), M2kClient.UdArrayCommand.Replace, App.ErpCon);
            }
        }

        #endregion
    }
}
