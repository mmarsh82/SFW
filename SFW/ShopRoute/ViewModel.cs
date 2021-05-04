using M2kClient;
using SFW.Helpers;
using SFW.Model;
using SFW.Reports;
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
                shopOrder.ToolList = shopOrder.ToolList ?? new System.Collections.Generic.List<string>();
                OnPropertyChanged(nameof(ShopOrder));
                OnPropertyChanged(nameof(FqSalesOrder));
                ShopOrderNotes = null;
                MachineGroup = string.Empty;
                OnPropertyChanged(nameof(CanCheckHistory));
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

        private bool loading;
        public bool IsLoading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public bool CanCheckHistory { get { return ShopOrder?.StartQty != ShopOrder?.CurrentQty; } }
        public bool CanReport { get { return CurrentUser.IsLoggedIn && MachineGroup == "PRESS"; } }
        public bool CanSeeTrim { get { return MachineGroup == "PRESS"; } }

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

        #region Press Report ICommand

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
            if (int.TryParse(parameter.ToString(), out int i))
            {
                var _repType = (Enumerations.PressReportActions)i;
                var _rep = new PressReport(shopOrder, App.AppSqlCon);
                if (_repType == Enumerations.PressReportActions.LogProgress && (_rep.IsNew || _rep.ShiftReportList.Count == 0))
                {
                    MessageBox.Show("There is currently no report created for this work order.\nPlease click on the report sheet button and submit a new report.", "No Report Sheet", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    new Press_View { DataContext = new Press_ViewModel(ShopOrder, _repType) }.Show();
                }
            }
        }
        private bool ReportCanExecute(object parameter) => true;

        #endregion
    }
}
