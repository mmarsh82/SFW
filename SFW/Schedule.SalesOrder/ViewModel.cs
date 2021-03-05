using System;
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
                    Controls.WorkSpaceDock.UpdateChildDock(9, 1, new ShopRoute.SalesOrder.ViewModel(new Model.SalesOrder(value.Row)));
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
                if (!string.IsNullOrEmpty(value))
                {
                    ((DataView)SalesScheduleView.SourceCollection).RowFilter = ((DataView)SalesScheduleView.SourceCollection).Table.SearchRowFilter(value);
                }
                else
                {
                    ((DataView)SalesScheduleView.SourceCollection).RowFilter = "";
                }
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
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
            SalesScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("Cust_Name"));
            if (!string.IsNullOrEmpty(filter))
            {
                ((DataView)SalesScheduleView.SourceCollection).RowFilter = filter;
                OnPropertyChanged(nameof(SalesScheduleView));
            }
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
