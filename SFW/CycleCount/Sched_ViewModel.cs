using SFW.Controls;
using SFW.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Data;

namespace SFW.CycleCount
{
    public class Sched_ViewModel : ViewModelBase
    {
        #region Properties

        public ICollectionView ScheduleView { get; set; }

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
                    WorkSpaceDock.CountDock.Children.RemoveAt(1);
                    WorkSpaceDock.CountDock.Children.Insert(1, new Form_View { DataContext = new Form_ViewModel(_cnt) });
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
                    _originalFilter = ((DataView)ScheduleView.SourceCollection).RowFilter;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    var _sRowFilter = ((DataView)ScheduleView.SourceCollection).Table.SearchRowFilter(value);
                    ((DataView)ScheduleView.SourceCollection).RowFilter = !string.IsNullOrEmpty(_originalFilter)
                        ? $"{_originalFilter} AND ({_sRowFilter})"
                        : _sRowFilter;
                }
                else
                {
                    ((DataView)ScheduleView.SourceCollection).RowFilter = _originalFilter;
                }
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
                ScheduleView.Refresh();
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

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        #endregion

        /// <summary>
        /// Default Cycle Count ViewModel Constructor
        /// </summary>
        public Sched_ViewModel()
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
            if (string.IsNullOrEmpty(filter))
            {
                ViewLoading(string.Empty);
            }
            else
            {
                ((DataView)ScheduleView.SourceCollection).RowFilter = $"PartNumber = '{filter}'";
                OnPropertyChanged(nameof(ScheduleView));
            }
        }

        public void ViewLoading(string machineNbr)
        {
            ScheduleView = CollectionViewSource.GetDefaultView(Model.Count.GetScheduleData(App.AppSqlCon));
            ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("CountLoc"));
            EmptyCount = ScheduleView.Cast<object>().Count() == 0;
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
                var _oldItem = ScheduleView.CurrentItem;
                ScheduleView = CollectionViewSource.GetDefaultView(Model.Count.GetScheduleData(App.AppSqlCon));
                ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("CountLoc"));
                OnPropertyChanged(nameof(ScheduleView));
                if (_oldItem != null && ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().Any(r => r.Field<string>("CountID") == ((DataRowView)_oldItem).Row.Field<string>("CountID")))
                {
                    var schedList = ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().ToList();
                    var listIndex = schedList.FindIndex(r => r.Field<string>("CountID") == ((DataRowView)_oldItem).Row.Field<string>("CountID"));
                    ScheduleView.MoveCurrentToPosition(listIndex);
                }
                ScheduleView.Refresh();
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    SearchFilter = SearchFilter;
                }
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
                var _oldItem = ScheduleView.CurrentItem;
                ScheduleView = CollectionViewSource.GetDefaultView(Count.GetScheduleData(App.AppSqlCon));
                ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("CountLoc"));
                OnPropertyChanged(nameof(ScheduleView));
                ((DataView)ScheduleView.SourceCollection).RowFilter = $"CountID != '{filter}'";
                ScheduleView.Refresh();
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    SearchFilter = SearchFilter;
                }
            }
            catch (Exception)
            { }
        }
    }
}
