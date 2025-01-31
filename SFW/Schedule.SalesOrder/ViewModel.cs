using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Data;

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
                _selectedSO = value;
                if(value != null)
                {
                    var _sku = new Model.Sku(value.Row.Field<string>("PartNbr"), 'S', App.SiteNumber, true);
                    var _soObj = new Model.SalesOrder(value.Row);
                    Controls.WorkSpaceDock.UpdateChildDock(8, 1, new ShopRoute.SalesOrder.ViewModel(_soObj, _sku));
                }
                else
                {
                    Controls.WorkSpaceDock.UpdateChildDock(8, 1, new ShopRoute.SalesOrder.ViewModel());
                }
                OnPropertyChanged(nameof(SelectedSalesOrder));
            }
        }

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
                if (!_inLoad)
                {
                    if (value)
                    {
                        RefreshSchedule();
                    }
                    else
                    {
                        SalesScheduleView = SalesScheduleView.Table.AsEnumerable()
                            .GroupBy(r => r.Field<string>("SoNbr"))
                            .Select(g => g.First())
                            .CopyToDataTable()
                            .AsDataView();
                        SearchFilter = SearchFilter;
                        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SalesScheduleView)));
                    }
                }
                _inLoad = false;
                _schedType = value;
                OnPropertyChanged(nameof(ScheduleType));
                OnPropertyChanged(nameof(ScheduleTypeContent));
            }
        }
        public string ScheduleTypeContent { get { return ScheduleType ? "Detail:" : "Header:"; } }
        private bool _inLoad;

        private bool _isSched;
        public bool IsSchedule
        {
            get { return _isSched; }
            set
            {
                var _valAsStr = string.Empty;
                if (value)
                {
                    _valAsStr = "[IsWOLinked]=0";
                    SelectedType = OrderTypeList.FirstOrDefault(o => o.Contains("DAI"));
                    PickSelected = true;
                }
                else
                {
                    SelectedType = OrderTypeList.FirstOrDefault(o => o.Contains("All"));
                    PickSelected = false;
                }
                FilterSchedule(_valAsStr, 6);
                _isSched = value;
                OnPropertyChanged(nameof(IsSchedule));
                OnPropertyChanged(nameof(IsScheduleContent));
            }
        }
        public string IsScheduleContent { get { return IsSchedule ? "New:" : "Open:"; } }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public static IAsyncResult LoadAsyncComplete { get; set; }

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            if (CurrentUser.HasSalesOrderModule)
            {
                SalesTableFilter = new string[10];
                if (OrderTypeList == null)
                {
                    OrderTypeList = Model.SalesOrder.GetOrderTypeList();
                    OrderTypeList.Insert(0, "All");
                }
                if (CreditStatusList == null)
                {
                    CreditStatusList = new List<string>
                {
                    "Any"
                    ,"A"
                    ,"H"
                    ,"W"
                };
                }
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                FilterAsyncDelegate = new LoadDelegate(FilterView);
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke("", new AsyncCallback(ViewLoaded), null);
                if (CurrentUser.HasSalesOrderModule)
                {
                    RefreshTimer.Add(RefreshSchedule);
                }
                _inLoad = true;
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
            if (SalesScheduleView != null && !filter.Contains("Machine"))
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

        public void FilterView(string filter)
        {
            ViewLoading(filter);
        }

        public void ViewLoading(string filter)
        {
            try
            {
                SalesScheduleView = ModelBase.MasterDataSet.Tables["SalesMaster"].AsDataView();
                if (SalesScheduleView != null)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        SalesScheduleView.RowFilter = filter;
                        OnPropertyChanged(nameof(SalesScheduleView));
                    }
                    SelectedCredStatus = CreditStatusList[0];
                    IsSchedule = false;
                    ScheduleType = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Unhandled Exception", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        public void ViewLoaded(IAsyncResult r)
        {

        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            try
            {
                var _drow = SelectedSalesOrder;
                SalesScheduleView = ModelBase.MasterDataSet.Tables["SalesMaster"].AsDataView();
                OnPropertyChanged(nameof(SalesScheduleView));
                if (_drow != null && (SalesScheduleView.Table.AsEnumerable().Any(r => r.Field<string>("ID") == ((DataRowView)_drow).Row.Field<string>("ID"))))
                {
                    SelectedSalesOrder = _drow;
                }
                else
                {
                    SelectedSalesOrder = null;
                }
                SearchFilter = SearchFilter;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SalesScheduleView)));
            }
            catch (Exception)
            {

            }
        }
    }
}
