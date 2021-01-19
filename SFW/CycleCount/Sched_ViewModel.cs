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

        public ICollectionView CountView { get; set; }

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
                    _originalFilter = ((DataView)CountView.SourceCollection).RowFilter;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    var _sRowFilter = ((DataView)CountView.SourceCollection).Table.SearchRowFilter(value);
                    ((DataView)CountView.SourceCollection).RowFilter = !string.IsNullOrEmpty(_originalFilter)
                        ? $"{_originalFilter} AND ({_sRowFilter})"
                        : _sRowFilter;
                }
                else
                {
                    ((DataView)CountView.SourceCollection).RowFilter = _originalFilter;
                }
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
                CountView.Refresh();
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
                ((DataView)CountView.SourceCollection).RowFilter = $"PartNumber = '{filter}'";
                OnPropertyChanged(nameof(CountView));
            }
        }

        public void ViewLoading(string filter)
        {
            CountView = CollectionViewSource.GetDefaultView(Count.GetScheduleData(App.AppSqlCon));
            CountView.GroupDescriptions.Add(new PropertyGroupDescription("CountLoc"));
            EmptyCount = CountView.Cast<object>().Count() == 0;
        }
        public void ViewLoaded(IAsyncResult r)
        {
            CountView.Refresh();
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            try
            {
                var _oldItem = CountView.CurrentItem;
                CountView = CollectionViewSource.GetDefaultView(Count.GetScheduleData(App.AppSqlCon));
                CountView.GroupDescriptions.Add(new PropertyGroupDescription("CountLoc"));
                OnPropertyChanged(nameof(CountView));
                if (_oldItem != null && ((DataView)CountView.SourceCollection).Table.AsEnumerable().Any(r => r.Field<string>("CountID") == ((DataRowView)_oldItem).Row.Field<string>("CountID")))
                {
                    var schedList = ((DataView)CountView.SourceCollection).Table.AsEnumerable().ToList();
                    var listIndex = schedList.FindIndex(r => r.Field<string>("CountID") == ((DataRowView)_oldItem).Row.Field<string>("CountID"));
                    CountView.MoveCurrentToPosition(listIndex);
                }
                CountView.Refresh();
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
                var _oldItem = CountView.CurrentItem;
                var _schedData = Count.GetScheduleData(App.AppSqlCon);
                _schedData.Rows.Remove(_schedData.Select($"CountID == '{filter}'")[0]);
                _schedData.AcceptChanges();
                CountView = CollectionViewSource.GetDefaultView(_schedData);
                CountView.GroupDescriptions.Add(new PropertyGroupDescription("CountLoc"));
                OnPropertyChanged(nameof(CountView));
                CountView.Refresh();
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    SearchFilter = SearchFilter;
                }
                if (((DataView)CountView.SourceCollection).Count == 0)
                {
                    WorkSpaceDock.CountDock.Children.RemoveAt(1);
                    WorkSpaceDock.CountDock.Children.Insert(1, new Form_View { DataContext = new Form_ViewModel() });
                }
            }
            catch (Exception)
            { }
        }
    }
}
