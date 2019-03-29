using M2kClient;
using SFW.Commands;
using SFW.Enumerations;
using SFW.Model;
using SFW.Reports;
using System;
using System.Windows;
using System.Windows.Input;

namespace SFW.ShopRoute
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private WorkOrder shopOrder;
        public WorkOrder ShopOrder
        {
            get { return shopOrder; }
            set
            {
                shopOrder = value;
                OnPropertyChanged(nameof(ShopOrder));
                OnPropertyChanged(nameof(FqSalesOrder));
                ShopOrderNotes = null;
                MachineGroup = string.Empty;
                OnPropertyChanged(nameof(CanCheckHistory));
                OnPropertyChanged(nameof(HasStarted));
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanSeeWip));
            }
        }

        public string FqSalesOrder
        {
            get { return $"{ShopOrder?.SalesOrder?.SalesNumber}*{ShopOrder?.SalesOrder?.LineNumber}"; }
        }

        public int CurrentSite { get { return App.SiteNumber; } }

        private string _shopNotes;
        public string ShopOrderNotes
        {
            get
            { return _shopNotes; }
            set
            {
                _shopNotes = string.IsNullOrEmpty(value) ? ShopOrder?.Notes : value;
                OnPropertyChanged(nameof(ShopOrderNotes));
            }
        }

        private string machGroup;
        public string MachineGroup
        {
            get
            { return machGroup; }
            set
            { machGroup = string.IsNullOrEmpty(value) ? Machine.GetMachineGroup(App.AppSqlCon, ShopOrder?.OrderNumber, ShopOrder?.Seq) : value; OnPropertyChanged(nameof(MachineGroup)); }
        }
        public bool CanCheckHistory { get { return ShopOrder?.StartQty != ShopOrder?.CurrentQty; } }
        public bool HasStarted { get { return CurrentUser.IsLoggedIn && MachineGroup == "PRESS" && ShopOrder.ActStartDate != DateTime.MinValue; } }
        public bool CanStart { get { return CurrentUser.IsLoggedIn && MachineGroup == "PRESS" && ShopOrder.ActStartDate == DateTime.MinValue; } }
        public bool CanSeeWip { get { return CurrentUser.IsLoggedIn && ((MachineGroup == "PRESS" && HasStarted) || MachineGroup != "PRESS"); } }

        private RelayCommand _noteChange;
        private RelayCommand _loadReport;

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

        /// <summary>
        /// Updates static properties from the CurrentUser object to the local view
        /// </summary>
        public void UpdateView()
        {
            OnPropertyChanged(nameof(CanSeeWip));
            OnPropertyChanged(nameof(HasStarted));
            OnPropertyChanged(nameof(CanStart));
        }

        #region Work Order Note Change ICommand

        public ICommand WONoteChgICommand
        {
            get
            {
                if (_noteChange == null)
                {
                    _noteChange = new RelayCommand(NoteChgExecute, NoteChgCanExecute);
                }
                return _noteChange;
            }
        }

        private void NoteChgExecute(object parameter)
        {
            var _noteArray = ShopOrderNotes.Replace("\r", "").Replace("\n", "|").Split('|');
            var _changeRequest = M2kCommand.EditMVRecord("WP", ShopOrder.OrderNumber, 39, _noteArray, App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
                ShopOrderNotes = ShopOrder.Notes;
            }
        }
        private bool NoteChgCanExecute(object parameter) => true;

        #endregion

        #region Load the work order report ICommand

        public ICommand ReportICommand
        {
            get
            {
                if (_loadReport == null)
                {
                    _loadReport = new RelayCommand(ReportExecute, ReportCanExecute);
                }
                return _loadReport;
            }
        }

        private void ReportExecute(object parameter)
        {

            if (parameter != null && Enum.TryParse(parameter.ToString(), out PressReportActions pressAction))
            {
                using (var report = new Press_ViewModel(ShopOrder, pressAction))
                {
                    new Press_View { DataContext = report }.ShowDialog();
                }
            }
        }
        private bool ReportCanExecute(object parameter) => true;

        #endregion
    }
}
