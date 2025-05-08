using SFW.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace SFW.Schedule.SalesOrder
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public string[] SalesTableFilter;
        public DataView SalesScheduleView { get; set; }

        private DataRowView _selectedSO;
        public DataRowView SelectedSalesOrder
        {
            get { return _selectedSO; }
            set
            {
                try
                {

                    _selectedSO = value;
                    if (value != null && App.LoadedModule == Enumerations.UsersControls.SalesOrder)
                    {
                        var _sku = new Sku(value.Row.Field<string>("PartNbr"), 'S', App.SiteNumber, true);
                        var _soObj = new Model.SalesOrder(value.Row);
                        var _action = new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(8, 1, new ShopRoute.SalesOrder.ViewModel(_soObj, _sku)); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                    OnPropertyChanged(nameof(SelectedSalesOrder));
                }
                catch (Exception)
                { }
            }
        }
        public int SelectedIndex;

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                var _fltr = !string.IsNullOrEmpty(value) ? $"{SalesScheduleView.Table.SearchRowFilter(value)}" : "";
                FilterSchedule(_fltr, 0);
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        public IList<string> OrderTypeList { get; set; }
        private string _selType;
        public string SelectedType
        {
            get { return _selType; }
            set
            {
                var _fltr = value != "All" ? $"[Type]='{value}'" : "";
                FilterSchedule(_fltr, 1);
                _selType = value;
                OnPropertyChanged(nameof(SelectedType));
            }
        }

        public IList<string> CreditStatusList { get; set; }
        private string _credStatus;
        public string SelectedCredStatus
        {
            get { return _credStatus; }
            set
            {
                var _fltr = value != "Any" ? $"[CredStatus]='{value}'" : "";
                FilterSchedule(_fltr, 5);
                _credStatus = value;
                OnPropertyChanged(nameof(SelectedCredStatus));
            }
        }

        private bool _isNew;
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                var _fltr = value ? $"[DateAdded]='{DateTime.Today.ToString("yyyy-MM-dd")}'" : "";
                FilterSchedule(_fltr, 7);
                _isNew = value;
                OnPropertyChanged(nameof(IsNew));
            }
        }

        private bool _pickSel;
        public bool PickSelected
        {
            get { return _pickSel; }
            set
            {
                FilterSchedule(value ? "[MTO]='0'" : "[MTO]='1'", 2);
                _pickSel = value;
                OnPropertyChanged(nameof(PickSelected));
                OnPropertyChanged(nameof(PickContent));
            }
        }
        public string PickContent { get { return PickSelected == true ? "Pick:" : "MTO:"; } }

        private bool _schedType;
        public bool ScheduleType
        {
            get { return _schedType; }
            set
            {
                if (value && SalesScheduleView != null)
                {
                    RefreshSchedule();
                }
                else if (SalesScheduleView != null)
                {
                    SalesScheduleView = SalesScheduleView.Table.AsEnumerable()
                        .GroupBy(r => r.Field<string>("SoNbr"))
                        .Select(g => g.First())
                        .CopyToDataTable()
                        .AsDataView();
                    SearchFilter = SearchFilter;
                    OnPropertyChanged(nameof(SalesScheduleView));
                }
                _schedType = value;
                OnPropertyChanged(nameof(ScheduleType));
                OnPropertyChanged(nameof(ScheduleTypeContent));
            }
        }
        public string ScheduleTypeContent { get { return ScheduleType ? "Detail:" : "Header:"; } }

        private bool _isSched;
        public bool IsSchedule
        {
            get { return _isSched; }
            set
            {
                if (value)
                {
                    SelectedType = OrderTypeList.FirstOrDefault(o => o.Contains("DAI"));
                    PickSelected = true;
                }
                else
                {
                    SelectedType = OrderTypeList.FirstOrDefault(o => o.Contains("All"));
                    PickSelected = false;
                }
                _isSched = value;
                OnPropertyChanged(nameof(IsSchedule));
                OnPropertyChanged(nameof(IsScheduleContent));
            }
        }
        public string IsScheduleContent { get { return IsSchedule ? "New:" : "Open:"; } }

        public delegate void LoadDelegate(string s, int i);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public static IAsyncResult LoadAsyncComplete { get; set; }

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            ScheduleType = true;
            if (App.SiteNumber == 1)
            {
                RefreshTimer.Add(RefreshSchedule);
                SalesScheduleView = new DataView();
                SalesTableFilter = new string[10];
                OrderTypeList = Model.SalesOrder.GetOrderTypeList();
                OrderTypeList.Insert(0, "All");
                IsSchedule = false;
                CreditStatusList = new List<string>
                {
                    "Any"
                    ,"A"
                    ,"H"
                    ,"W"
                };
                SelectedCredStatus = CreditStatusList[0];
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(SalesScheduleView.RowFilter, 0, new AsyncCallback(ViewLoaded), null);
            }
        }

        /// <summary>
        /// Filter the schedule view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Order Type Filter
        /// 2 = Pick Selected Filter
        /// 3 = Work Center Filter
        /// 4 = Work Center Group Filter
        /// 5 = Credit Status Filter
        /// 6 = Scheduling Status Filter
        /// 7 = New Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public void FilterSchedule(string filter, int index)
        {
            if (SalesScheduleView != null)
            {
                SalesTableFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in SalesTableFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                SalesScheduleView.RowFilter = _filterStr;
            }
        }

        #region Loading Async Delegation Implementation

        public void ViewLoading(string filter, int index)
        {
            try
            {
                SelectedIndex = index < 0 ? 0 : index;
                SalesScheduleView = ModelBase.MasterDataSet.Tables["SalesMaster"].AsDataView();
                SearchFilter = !string.IsNullOrEmpty(SearchFilter) ? SearchFilter : string.Empty;
                SalesScheduleView.RowFilter = filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sales Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ViewLoaded(IAsyncResult r)
        {
            if (SalesScheduleView != null && SalesScheduleView.Count > 0)
            {
                SelectedSalesOrder = SalesScheduleView.Count >= SelectedIndex ? SalesScheduleView[SelectedIndex] : null;
            }
            if (OrderTypeList.Count == 1)
            {
                OrderTypeList.Clear();
                OrderTypeList = Model.SalesOrder.GetOrderTypeList();
                OrderTypeList.Insert(0, "All");
                OnPropertyChanged(nameof(OrderTypeList));
            }
            OnPropertyChanged(nameof(SalesScheduleView));
            if (App.LoadedModule == Enumerations.UsersControls.SalesOrder)
            {
                MainWindowViewModel.DisplayAction = false;
            }
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            var _filter = SalesScheduleView != null ? SalesScheduleView.RowFilter : string.Empty;
            var _index = 0;
            if (SelectedSalesOrder != null)
            {
                var _targetId = SelectedSalesOrder.Row.SafeGetField<string>("ID");
                _index = SalesScheduleView.Cast<DataRowView>().Select((row, idx) => new { row, idx }).FirstOrDefault(o => o.row["ID"].ToString() == _targetId)?.idx ?? 0;
            }
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, _index, new AsyncCallback(ViewLoaded), null);
        }
    }
}
