using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;

namespace SFW.Schedule.SalesOrder
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public ICollectionView SalesScheduleView { get; set; }

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
                OnPropertyChanged(nameof(SelectedSalesOrder));
            }
        }

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                ((DataView)SalesScheduleView.SourceCollection).RowFilter = "";
                if (!string.IsNullOrEmpty(value))
                {
                    ((DataView)SalesScheduleView.SourceCollection).RowFilter = $"({((DataView)SalesScheduleView.SourceCollection).Table.SearchRowFilter(value)})";
                }
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
                SelectedType = SelectedType;
            }
        }

        public IList<string> OrderTypeList { get; set; }
        private string _selType;
        public string SelectedType
        {
            get { return _selType; }
            set
            {
                ((DataView)SalesScheduleView.SourceCollection).RowFilter = ((DataView)SalesScheduleView.SourceCollection).RowFilter.Replace($" AND [Type]='{_selType}'", "");
                ((DataView)SalesScheduleView.SourceCollection).RowFilter = ((DataView)SalesScheduleView.SourceCollection).RowFilter.Replace($"[Type]='{_selType}'", "");
                if (value != "All")
                {
                    if (string.IsNullOrEmpty(((DataView)SalesScheduleView.SourceCollection).RowFilter))
                    {
                        ((DataView)SalesScheduleView.SourceCollection).RowFilter = $"[Type]='{value}'";
                    }
                    else
                    {
                        ((DataView)SalesScheduleView.SourceCollection).RowFilter += $" AND [Type]='{value}'";
                    }
                }
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
                if (value)
                {
                    if (string.IsNullOrEmpty(((DataView)SalesScheduleView.SourceCollection).RowFilter))
                    {
                        ((DataView)SalesScheduleView.SourceCollection).RowFilter = $"[MTO]='{_valAsInt}'";
                    }
                    else
                    {
                        ((DataView)SalesScheduleView.SourceCollection).RowFilter += $" AND [MTO]='{_valAsInt}'";
                    }
                }
                else
                {
                    _valAsInt = _pickSel ? 1 : 0;
                    ((DataView)SalesScheduleView.SourceCollection).RowFilter = ((DataView)SalesScheduleView.SourceCollection).RowFilter.Replace($" AND [MTO]='{_valAsInt}'", "");
                    ((DataView)SalesScheduleView.SourceCollection).RowFilter = ((DataView)SalesScheduleView.SourceCollection).RowFilter.Replace($"[MTO]='{_valAsInt}'", "");
                }
                _pickSel = value;
                OnPropertyChanged(nameof(PickSelected));
            }
        }

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
            if (OrderTypeList == null)
            {
                OrderTypeList = Model.SalesOrder.GetOrderTypeList(App.AppSqlCon);
                OrderTypeList.Insert(0, "All");
            }
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            var _filter = "";
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, new AsyncCallback(ViewLoaded), null);
            RefreshTimer.Add(RefreshSchedule);
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
            SalesScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("CustName"));
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
                var _srch = SearchFilter;
                var _drow = SelectedSalesOrder;
                SalesScheduleView = CollectionViewSource.GetDefaultView(Model.SalesOrder.GetScheduleData(App.AppSqlCon));
                SalesScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("Cust_Name"));
                OnPropertyChanged(nameof(SalesScheduleView));
                if(_drow != null)
                {
                    SelectedSalesOrder = _drow;
                }
                if(!string.IsNullOrEmpty(_srch))
                {
                    SearchFilter = _srch;
                }
            }
            catch (Exception)
            {

            }
        }

    }
}
