using SFW.Controls;
using SFW.Converters;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SFW.QMS.NcrNotice
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static string[] NoticeViewFilter;
        public static ICollectionView NoticeView { get; set; }

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
                        Controls.WorkSpaceDock.UpdateChildDock(9, 1, new NcrForm.View { DataContext = new NcrForm.ViewModel(_ncr, SelectedNcr.Row.Field<int>("NcrRevisionId")) });
                    }
                    OnPropertyChanged(nameof(SelectedNcr));
                }
                catch (Exception)
                { }
            }
        }
        private object _oldSelectedNcr;

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
                var _filter = string.IsNullOrEmpty(value) ? "" : ((DataView)NoticeView.SourceCollection).Table.SearchRowFilter(value);
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

        private bool Refresh { get; set; }

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
            Refresh = false;
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(App.ViewFilter[App.SiteNumber], new AsyncCallback(ViewLoaded), null);
            RefreshTimer.Add(RefreshNotice);
            NoticeViewFilter = new string[7];
            NoticeFilter($"[Site] = {App.SiteNumber}", 2);
            ClosedFilter = false;
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
        public static void NoticeFilter(string filter, int index)
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
                    ((DataView)NoticeView.SourceCollection).RowFilter = _filterStr;
                    NoticeView.Refresh();
                }
            }
            else
            {
                NoticeViewFilter = new string[6];
            }
        }

        /// <summary>
        /// Clears the notice filter string array
        /// </summary>
        public static void ClearFilter()
        {
            if (NoticeViewFilter != null)
            {
                NoticeViewFilter = new string[7];
                if (NoticeView != null && NoticeView.SourceCollection != null && ((DataView)NoticeView.SourceCollection).RowFilter != null)
                {
                    ((DataView)NoticeView.SourceCollection).RowFilter = "";
                }
                NoticeFilter("[Status] <> 'C'", 5);
                NoticeFilter($"[Site] = {App.SiteNumber}", 6);
                if (NoticeView != null)
                {
                    NoticeView.Refresh();
                }
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
                IsLoading = true;
                if (Refresh)
                {
                    ModelBase.BuildMasterDataSet(UserConfig.GetIROD(), App.SiteNumber, App.AppSqlCon);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            try
            {
                RefreshTimer.IsRefreshing = IsLoading = Refresh = false;
                MainWindowViewModel.DisplayAction = false;
                var _oldfilter = string.Empty;
                if (NoticeView != null && CurrentUser.IsLoggedIn)
                {
                    _oldfilter = ((DataView)NoticeView.SourceCollection).RowFilter;
                }
                var _notice = new Ncr.Notice();
                NoticeView = CollectionViewSource.GetDefaultView(_notice.Table);
                NoticeView.GroupDescriptions.Add(new PropertyGroupDescription("RevisionDateTime", new DateGroupConverter()));
                if (_oldSelectedNcr != null)
                {
                    if (((DataView)NoticeView.SourceCollection).Table.AsEnumerable().Any(row => row.Field<int>("NcrId") == ((DataRowView)_oldSelectedNcr).Row.Field<int>("NcrId")))
                    {
                        var _index = NoticeView.IndexOf(_oldSelectedNcr, "NcrId");
                        NoticeView.MoveCurrentToPosition(_index);
                        SelectedNcr = (DataRowView)_oldSelectedNcr;
                    }
                    else
                    {
                        NoticeView.MoveCurrentToPosition(-1);
                        SelectedNcr = null;
                    }
                }
                if (!string.IsNullOrEmpty(_oldfilter))
                {
                    ((DataView)NoticeView.SourceCollection).RowFilter = _oldfilter;
                }
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    SearchFilter = SearchFilter;
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(NoticeView)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    RefreshTimer.IsRefreshing = IsLoading = Refresh = true;
                    MainWindowViewModel.DisplayAction = App.LoadedModule == Enumerations.UsersControls.Quality;
                    if (NoticeView?.CurrentItem != null)
                    {
                        _oldSelectedNcr = NoticeView.CurrentItem;
                    }
                    SelectedNcr = null;
                    LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(((DataView)NoticeView.SourceCollection).RowFilter, new AsyncCallback(ViewLoaded), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
