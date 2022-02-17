using SFW.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

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
                if (!string.IsNullOrEmpty(value.SalesNumber))
                {
                    Order.LineList = Model.SalesOrder.GetLineList(value.SalesNumber, App.AppSqlCon, Order.LineNumber);
                }
                OnPropertyChanged(nameof(Order));
                OnPropertyChanged(nameof(CanAccept));
                OnPropertyChanged(nameof(CanHold));
                OnPropertyChanged(nameof(CanPending));
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

        public bool CanAccept { get { return Order.CreditStatus != "A"; } }
        public bool CanHold { get { return Order.CreditStatus != "H"; } }
        public bool CanPending { get { return Order.CreditStatus != "W"; } }

        RelayCommand _arUpdate;

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
            var _code = parameter.ToString()[0];
            ///Credit Status = M2k Credit_Code - Field 20
            ///Credit Date = M2k Credit_Date - Field 18
            ///Credit Approver = M2k Credit_Chk - Field 17
            ///Credit Approver + Site = M2k Credit_Chk_Acc - Field 19
            var _changeRequest = M2kClient.M2kCommand.EditRecord("SOH", Order.SalesNumber
                , new int[4] { 20, 18, 17, 19 }
                , new string[4] { _code.ToString(), DateTime.Now.ToString("MM-dd-yy"), CurrentUser.DomainUserName.ToUpper(), $"{CurrentUser.DomainUserName}:{App.Site.Replace('_', '.')}" }, M2kClient.UdArrayCommand.Replace, App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
            }
        }
        private bool ARUpdateCanExecute(object parameter) => true;

        #endregion
    }
}
