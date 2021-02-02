using SFW.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;

namespace SFW.Schedule.SalesOrder
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public ICollectionView ScheduleView { get; set; }

        private DataRowView _selectedSO;
        public DataRowView SelectedSalesOrder
        {
            get { return _selectedSO; }
            set
            {

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
            ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(UserConfig.GetIROD(), App.AppSqlCon));
            ScheduleView.SortDescriptions.Add(new SortDescription("SaleOrder", ListSortDirection.Ascending));
            if (!string.IsNullOrEmpty(filter))
            {
                ((DataView)ScheduleView.SourceCollection).RowFilter = filter;
                OnPropertyChanged(nameof(ScheduleView));
            }
        }
        public void ViewLoaded(IAsyncResult r)
        {
            ScheduleView.Refresh();
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            try
            {
                
            }
            catch (Exception)
            {

            }
        }

    }
}
