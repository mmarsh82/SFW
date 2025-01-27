using SFW.Controls;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.QMS.NcrNotice
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static string[] NoticeViewFilter;
        public DataView NoticeView { get; set; }

        private DataRowView _selectedNcr;
        public DataRowView SelectedNcr
        {
            get { return _selectedNcr; }
            set
            {
                try
                {
                    _selectedNcr = value;
                    WorkSpaceDock.UpdateChildDock(9, 1, new NcrForm.View { DataContext = new NcrForm.ViewModel(false) });
                    if (value != null)
                    {
                        var _ncr = new Ncr(value.Row.Field<int>("NcrId"));
                        WorkSpaceDock.UpdateChildDock(9, 1, new NcrForm.View { DataContext = new NcrForm.ViewModel(_ncr, SelectedNcr.Row.Field<int>("NcrRevisionId")) });
                    }
                    OnPropertyChanged(nameof(SelectedNcr));
                }
                catch (Exception)
                { }
            }
        }
        private DataRowView _oldSelectedNcr;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : NoticeView.Table.SearchRowFilter(value);
                NoticeFilter(_filter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        private bool _close;
        public bool ClosedFilter
        {
            get { return _close; }
            set
            {
                var _filter = value ? "[LinkedStatus] = 'Closed'" : "[LinkedStatus] <> 'Closed'";
                NoticeFilter(_filter, 1);
                _close = value;
                OnPropertyChanged(nameof(ClosedFilter));
            }
        }

        private bool _site;
        public bool SiteFilter
        {
            get { return _site; }
            set
            {
                var _filter = $"[Site] = {App.SiteNumber}";
                NoticeFilter(_filter, 2);
                _site = value;
                OnPropertyChanged(nameof(SiteFilter));
            }
        }

        RelayCommand _newNcr;

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public static IAsyncResult LoadAsyncComplete { get; set; }
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Notice ViewModel Default Constructor
        /// </summary>
        public ViewModel()
        {
            NoticeViewFilter = new string[7];
            NoticeFilter($"[Site] = {App.SiteNumber}", 2);
            ClosedFilter = false;
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(App.ViewFilter[App.SiteNumber], new AsyncCallback(ViewLoaded), null);
            RefreshTimer.Add(RefreshNotice);
        }

        /// <summary>
        /// Filter the notice view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Closed Filter
        /// 2 = Site Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public void NoticeFilter(string filter, int index)
        {
            if (NoticeViewFilter != null)
            {
                NoticeViewFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in NoticeViewFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                var _tempList = new List<DataView>();
                if (NoticeView != null)
                {
                    NoticeView.RowFilter = _filterStr;
                    OnPropertyChanged(nameof(NoticeView));
                }
            }
            else
            {
                NoticeViewFilter = new string[6];
            }
        }

        /// <summary>
        /// Get the current notice filter
        /// </summary>
        /// <returns>notice filter as string</returns>
        public static string NoticeFilter()
        {
            var _filterStr = string.Empty;
            foreach (var s in NoticeViewFilter.Where(o => !string.IsNullOrEmpty(o)))
            {
                _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
            }
            return _filterStr;
        }

        /// <summary>
        /// Clears the notice filter string array
        /// </summary>
        public void ClearFilter()
        {
            if (NoticeViewFilter != null)
            {
                NoticeViewFilter = new string[7];
                if (NoticeView != null && NoticeView != null && NoticeView.RowFilter != null)
                {
                    NoticeView.RowFilter = "";
                }
                NoticeFilter("[Status] <> 'C'", 5);
                NoticeFilter($"[Site] = {App.SiteNumber}", 6);
                OnPropertyChanged(nameof(NoticeView));
            }
        }

        #region New NCR input ICommand

        public ICommand NewNcrICommand
        {
            get
            {
                if (_newNcr == null)
                {
                    _newNcr = new RelayCommand(NewNcrExecute);
                }
                return _newNcr;
            }
        }

        private void NewNcrExecute(object parameter)
        {
            RefreshTimer.Stop();
            WorkSpaceDock.UpdateChildDock(9, 1, new NcrForm.View { DataContext = new NcrForm.ViewModel(true) });
        }

        #endregion

        #region Loading Async Delegation Implementation

        /// <summary>
        /// Async filter the schedule view
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        public void FilterNotice(string filter)
        {
            LoadAsyncComplete = FilterAsyncDelegate.BeginInvoke(filter, new AsyncCallback(ViewLoaded), null);
        }

        public void FilterView(string filter)
        {
            IsLoading = true;
            ViewLoading(filter);
        }

        public void ViewLoading(string filter)
        {
            try
            {
                NoticeView = new Ncr.Notice().Table.AsDataView();
                var _oldfilter = string.Empty;
                if (NoticeView != null && CurrentUser.IsLoggedIn)
                {
                    _oldfilter = NoticeView.RowFilter;
                }
                SelectedNcr = _oldSelectedNcr != null && NoticeView.Table.AsEnumerable().Any(row => row.Field<int>("NcrId") == _oldSelectedNcr.Row.Field<int>("NcrId"))
                    ? _oldSelectedNcr
                    : null;
                NoticeView.RowFilter = !string.IsNullOrEmpty(_oldfilter)
                    ? _oldfilter
                    : NoticeFilter();
                SearchFilter = !string.IsNullOrEmpty(SearchFilter)
                    ? SearchFilter
                    : string.Empty;
                NoticeView.Sort = "RevisionDateTime DESC";
                OnPropertyChanged(nameof(SelectedNcr));
                OnPropertyChanged(nameof(NoticeView));
                RefreshTimer.IsRefreshing = IsLoading = false;
                MainWindowViewModel.DisplayAction = false;
            }
            catch
            {
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            IsLoading = false;
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshNotice()
        {
            try
            {
                if (!IsLoading)
                {
                    RefreshTimer.IsRefreshing = IsLoading = true;
                    MainWindowViewModel.DisplayAction = App.LoadedModule == Enumerations.UsersControls.Quality;
                    _oldSelectedNcr = SelectedNcr != null
                        ? SelectedNcr
                        : null;
                    SelectedNcr = null;
                    LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(NoticeView.RowFilter, new AsyncCallback(ViewLoaded), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
