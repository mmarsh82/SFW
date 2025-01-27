using SFW.Model;
using System;
using System.Data;
using System.Linq;

namespace SFW.CycleCount
{
    public class Sched_ViewModel : ViewModelBase
    {
        #region Properties

        public DataView CountView { get; set; }

        private DataRowView _selCnt;
        public DataRowView SelectedCount
        {
            get { return _selCnt; }
            set
            {
                _selCnt = value;
                if (value != null)
                {
                    var _cnt = new Count(value.Row);
                    Controls.WorkSpaceDock.UpdateChildDock(3, 1, new Form_ViewModel(_cnt));
                }
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        private string _originalFilter;
        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                if (_sFilter == null || value == null)
                {
                    _originalFilter = CountView.RowFilter;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    var _sRowFilter = CountView.Table.SearchRowFilter(value);
                    CountView.RowFilter = !string.IsNullOrEmpty(_originalFilter)
                        ? $"{_originalFilter} AND ({_sRowFilter})"
                        : _sRowFilter;
                }
                else
                {
                    CountView.RowFilter = _originalFilter;
                }
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        private bool _emptyCnt;
        public bool EmptyCount
        {
            get
            { return _emptyCnt; }
            set
            {
                _emptyCnt = value;
                OnPropertyChanged(nameof(EmptyCount));
            }
        }

        public string ZoneLocation
        {
            get { return AppGlobal.ZoneLock; }
            set
            {
                if (value != AppGlobal.ZoneLock)
                {
                    AppGlobal.ZoneLock = value;
                }
                OnPropertyChanged(nameof(ZoneLocation));
            }
        }

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public static IAsyncResult LoadAsyncComplete { get; set; }

        #endregion

        /// <summary>
        /// Default Cycle Count ViewModel Constructor
        /// </summary>
        public Sched_ViewModel()
        {
            if (CurrentUser.IsInventoryControl)
            {
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                FilterAsyncDelegate = new LoadDelegate(FilterView);
                var _filter = "";
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, new AsyncCallback(ViewLoaded), null);
                if (CurrentUser.IsInventoryControl)
                {
                    RefreshTimer.Add(RefreshSchedule);
                }
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
            if (string.IsNullOrEmpty(filter))
            {
                ViewLoading(string.Empty);
            }
            else
            {
                CountView.RowFilter = $"PartNumber = '{filter}'";
                OnPropertyChanged(nameof(CountView));
            }
        }

        public void ViewLoading(string filter)
        {
            CountView = Count.GetScheduleData(App.AppSqlCon).AsDataView();
            EmptyCount = CountView.Cast<object>().Count() == 0;
            OnPropertyChanged(nameof(CountView));
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
                var _oldItem = SelectedCount;
                CountView = Count.GetScheduleData(App.AppSqlCon).AsDataView();
                SelectedCount = _oldItem != null
                    ? _oldItem
                    : null;
                SearchFilter = !string.IsNullOrEmpty(SearchFilter)
                    ? SearchFilter
                    : string.Empty;
                OnPropertyChanged(nameof(CountView));
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Refresh action for the schedule data
        /// Overloadded with a filter string
        /// </summary>
        public void RefreshSchedule(string filter)
        {
            try
            {
                var _oldItem = SelectedCount;
                var _schedData = Count.GetScheduleData(App.AppSqlCon);
                _schedData.Rows.Remove(_schedData.Select($"CountID == '{filter}'")[0]);
                _schedData.AcceptChanges();
                CountView = _schedData.AsDataView();
                SearchFilter = !string.IsNullOrEmpty(SearchFilter)
                     ? SearchFilter
                     : string.Empty;
                if (CountView.Count == 0)
                {
                    Controls.WorkSpaceDock.UpdateChildDock(3, 1, new Form_ViewModel());
                }
                OnPropertyChanged(nameof(CountView));
            }
            catch (Exception)
            { }
        }
    }
}
