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
                    Controls.WorkSpaceDock.UpdateChildDock(9, 1, new ShopRoute.SalesOrder.ViewModel(_soObj, _sku));
                }
                else
                {
                    Controls.WorkSpaceDock.UpdateChildDock(9, 1, new ShopRoute.SalesOrder.ViewModel());
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
                var _fltr = !string.IsNullOrEmpty(value) ? $"{((DataView)SalesScheduleView.SourceCollection).Table.SearchRowFilter(_sFilter)}" : "";
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

        private bool _pickSel;
        public bool PickSelected
        {
            get { return _pickSel; }
            set
            {
                var _valAsInt = value ? 1 : 0;
                FilterSchedule($"[MTO]='{_valAsInt}'", 2);
                _pickSel = value;
                OnPropertyChanged(nameof(PickSelected));
                OnPropertyChanged(nameof(PickContent));
            }
        }
        public string PickContent { get { return PickSelected ? "Pick:" : "MTO:"; } }

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
            SalesTableFilter = new string[5];
            if (OrderTypeList == null)
            {
                OrderTypeList = Model.SalesOrder.GetOrderTypeList(App.AppSqlCon);
                OrderTypeList.Insert(0, "All");
            }
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            var _filter = "";
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, new AsyncCallback(ViewLoaded), null);
            PickSelected = false;
            SelectedType = OrderTypeList.FirstOrDefault();
            RefreshTimer.Add(RefreshSchedule);
            if (!string.IsNullOrEmpty(MainWindowViewModel.MachineFilter))
            {
                FilterSchedule(MainWindowViewModel.MachineFilter, 3);
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
            SelectedType = OrderTypeList[0];
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
                OnPropertyChanged(nameof(SalesScheduleView));
                SalesScheduleView.Refresh();
            }
            catch (Exception)
            {

            }
        }

    }
}
