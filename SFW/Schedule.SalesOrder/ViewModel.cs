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

        public static string[] SalesTableFilter;
        public static ICollectionView SalesScheduleView { get; set; }

        private DataRowView _selectedSO;
        public DataRowView SelectedSalesOrder
        {
            get { return _selectedSO; }
            set
            {
                _selectedSO = value;
                if(value != null)
                {
                    var _sku = new Model.Sku(value.Row.Field<string>("PartNbr"), true, App.AppSqlCon);
                    var _soObj = new Model.SalesOrder(value.Row, App.AppSqlCon);
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
                var _fltr = !string.IsNullOrEmpty(value) ? $"{((DataView)SalesScheduleView.SourceCollection).Table.SearchRowFilter(value)}" : "";
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

        private bool? _pickSel;
        public bool? PickSelected
        {
            get { return _pickSel; }
            set
            {
                var _valAsStr = string.Empty;
                if (value == null)
                {
                    _valAsStr = "[MTO]='-1'";
                }
                else if (value == true)
                {
                    _valAsStr = "[MTO]='1'";
                }
                else
                {
                    _valAsStr = "[MTO]='0'";
                }
                FilterSchedule(_valAsStr, 2);
                _pickSel = value;
                OnPropertyChanged(nameof(PickSelected));
                OnPropertyChanged(nameof(PickContent));
            }
        }
        public string PickContent { get { return PickSelected == null ? "Off:" : PickSelected == true ? "Pick:" : "MTO:"; } }

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
                        SalesScheduleView = CollectionViewSource.GetDefaultView(((DataView)SalesScheduleView.SourceCollection).Table.AsEnumerable()
                            .GroupBy(r => r.Field<string>("SoNbr"))
                            .Select(g => g.First())
                            .CopyToDataTable());
                        SalesScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("FullCustName"));
                        SearchFilter = SearchFilter;
                        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SalesScheduleView)));
                        SalesScheduleView.Refresh();
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
                    PickSelected = null;
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
        public IAsyncResult LoadAsyncComplete { get; set; }

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
                    OrderTypeList = Model.SalesOrder.GetOrderTypeList(App.AppSqlCon);
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
                var _filter = "";
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, new AsyncCallback(ViewLoaded), null);
                if (CurrentUser.HasSalesOrderModule)
                {
                    RefreshTimer.Add(RefreshSchedule);
                }
                if (!string.IsNullOrEmpty(MainWindowViewModel.MachineFilter))
                {
                    FilterSchedule(MainWindowViewModel.MachineFilter, 3);
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
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public static void FilterSchedule(string filter, int index)
        {
            if (SalesScheduleView != null)
            {
                SalesTableFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in SalesTableFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                ((DataView)SalesScheduleView.SourceCollection).RowFilter = _filterStr;
                SalesScheduleView.Refresh();
            }
        }

        /// <summary>
        /// Async filter the schedule view
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        public void FilterSchedule(string filter)
        {
            LoadAsyncComplete = FilterAsyncDelegate.BeginInvoke(filter, new AsyncCallback(ViewLoaded), null);
        }

        #region Loading Async Delegation Implementation

        public void FilterView(string filter)
        {
            ViewLoading(filter);
        }

        public void ViewLoading(string filter)
        {
            SalesScheduleView = CollectionViewSource.GetDefaultView(Model.SalesOrder.GetScheduleData(App.AppSqlCon));
            SalesScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("FullCustName"));
            if (!string.IsNullOrEmpty(filter))
            {
                ((DataView)SalesScheduleView.SourceCollection).RowFilter = filter;
                OnPropertyChanged(nameof(SalesScheduleView));
            }
            SelectedCredStatus = CreditStatusList[0];
            IsSchedule = false;
            ScheduleType = true;
        }
        public void ViewLoaded(IAsyncResult r)
        {
            SalesScheduleView.Refresh();
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            try
            {
                var _drow = SalesScheduleView.CurrentItem;
                SalesScheduleView = CollectionViewSource.GetDefaultView(Model.SalesOrder.GetScheduleData(App.AppSqlCon));
                SalesScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("FullCustName"));
                if (_drow != null && ((DataView)SalesScheduleView.SourceCollection).Table.AsEnumerable().Any(r => r.Field<string>("ID") == ((DataRowView)_drow).Row.Field<string>("ID")))
                {
                    var _index = SalesScheduleView.IndexOf(_drow, "ID");
                    SalesScheduleView.MoveCurrentToPosition(_index);
                }
                else
                {
                    SalesScheduleView.MoveCurrentToPosition(-1);
                    SelectedSalesOrder = null;
                }
                SearchFilter = SearchFilter;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SalesScheduleView)));
                SalesScheduleView.Refresh();
            }
            catch (Exception)
            {

            }
        }
    }
}
