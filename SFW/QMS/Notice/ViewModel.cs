using SFW.Controls;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.QMS.Notice
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
                    if (value != null && App.LoadedModule == Enumerations.UsersControls.Quality)
                    {
                        var _ncr = new QmsForm(value.Row.Field<int>("NcrId"));
                        var _action = new Action(delegate { WorkSpaceDock.UpdateChildDock(9, 1, new Form.ViewModel(_ncr, SelectedNcr.Row.Field<int>("NcrRevisionId"))); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                    OnPropertyChanged(nameof(SelectedNcr));
                }
                catch (Exception)
                { }
            }
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
                var _filter = value ? "[FormStatus] = 'Closed'" : "[FormStatus] <> 'Closed'";
                NoticeFilter(_filter, 1);
                _close = value;
                OnPropertyChanged(nameof(ClosedFilter));
            }
        }

        private bool _frmType;
        public bool FormTypeFilter
        {
            get { return _frmType; }
            set
            {
                var _filter = value ? "[FormType] = 'NCR'" : "[FormType] = 'SCAR'";
                NoticeFilter(_filter, 3);
                _frmType = value;
                OnPropertyChanged(nameof(FormTypeFilter));
                OnPropertyChanged(nameof(FormType));
            }
        }
        public string FormType
        {
            get { return FormTypeFilter ? "NCR" : "SCAR"; }
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

        RelayCommand _newFrm;
        RelayCommand _exportQms;

        public delegate void LoadDelegate(string s, int i);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public static IAsyncResult LoadAsyncComplete { get; set; }
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Notice ViewModel Default Constructor
        /// </summary>
        public ViewModel()
        {
            RefreshTimer.Add(RefreshNotice);
            NoticeView = new DataView();
            NoticeViewFilter = new string[7];
            NoticeFilter($"[Site] = {App.SiteNumber}", 2);
            ClosedFilter = false;
            FormTypeFilter = true;
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(NoticeView.RowFilter, 0, new AsyncCallback(ViewLoaded), null);
        }

        /// <summary>
        /// Filter the notice view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Closed Filter
        /// 2 = Site Filter
        /// 3 = Form Type Filter
        /// 4 = Work Center Filter
        /// 5 = Work Center Group Filter
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

        #region New Form input ICommand

        public ICommand NewFormICommand
        {
            get
            {
                if (_newFrm == null)
                {
                    _newFrm = new RelayCommand(NewFromExecute);
                }
                return _newFrm;
            }
        }

        private void NewFromExecute(object parameter)
        {
            RefreshTimer.Stop();
            var _frmType = Enum.TryParse(FormType, out QmsForm.FormType _ft) ? _ft : QmsForm.FormType.NCR;
            WorkSpaceDock.UpdateChildDock(9, 1, new Form.View { DataContext = new Form.ViewModel(null, false, true, _frmType) });
        }

        #endregion

        #region Export QMS Metrics ICommand

        public ICommand ExportQmsICommand
        {
            get
            {
                if (_exportQms == null)
                {
                    _exportQms = new RelayCommand(ExportQmsExecute);
                }
                return _exportQms;
            }
        }

        private void ExportQmsExecute(object parameter)
        {
            ExcelWriter.ExportData(NoticeView.Table);
        }

        #endregion

        #region Loading Async Delegation Implementation

        public void ViewLoading(string filter, int index)
        {
            try
            {
                var _ncrId = NoticeView[index].Row.ItemArray[0];
                NoticeView = new QmsForm.Notice().Table.AsDataView();
                SearchFilter = !string.IsNullOrEmpty(SearchFilter) ? SearchFilter : string.Empty;
                NoticeView.RowFilter = filter;
                if (NoticeView[index].Row.ItemArray[0] == _ncrId)
                {
                    SelectedNcr = NoticeView.Count >= index ? NoticeView[index] : null; SelectedNcr = NoticeView.Count >= index ? NoticeView[index] : null;
                }
                else
                {
                    NoticeView.Sort = "NcrId";
                    index = NoticeView.Find(_ncrId);
                    SelectedNcr = NoticeView.Count >= index ? NoticeView[index] : null;
                }
                NoticeView.Sort = "RevisionDateTime DESC";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QMS Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            OnPropertyChanged(nameof(NoticeView));
            if (App.LoadedModule == Enumerations.UsersControls.Quality)
            {
                MainWindowViewModel.DisplayAction = false;
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
                MainWindowViewModel.DisplayAction = App.LoadedModule == Enumerations.UsersControls.Quality;
                var _filter = NoticeView != null ? NoticeView.RowFilter : string.Empty;
                var _index = 0;
                if (SelectedNcr != null)
                {
                    var _targetId = SelectedNcr.Row.SafeGetField<int>("NcrId").ToString();
                    _index = NoticeView.Cast<DataRowView>().Select((row, idx) => new { row, idx }).FirstOrDefault(o => o.row["NcrId"].ToString() == _targetId)?.idx ?? 0;
                }
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, _index, new AsyncCallback(ViewLoaded), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QMS Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
