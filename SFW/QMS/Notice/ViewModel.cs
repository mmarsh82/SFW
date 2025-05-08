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

namespace SFW.QMS.Notice
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static string[] NoticeViewFilter;
        public ICollectionView NoticeView { get; set; }

        private DataRowView _selectedNcr;
        public DataRowView SelectedNcr
        {
            get { return _selectedNcr; }
            set
            {
                try
                {
                    var _intial = App.SiteNumber == 1 && _selectedNcr == null;
                    _selectedNcr = value;
                    SelectedIndex = NoticeView.IndexOf(value, $"[NcrId] = {value.Row.SafeGetField<int>("NcrId")}");
                    if ((value != null && App.LoadedModule == Enumerations.UsersControls.Quality) || _intial)
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
        public int SelectedIndex;

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

        public delegate void LoadDelegate(string s, bool b);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsync { get; private set; }

        #endregion

        /// <summary>
        /// Notice ViewModel Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ModelBase.MasterDataSet.Tables.Contains("QmsNotice"))
            {
                RefreshTimer.Add(RefreshNotice);
                NoticeView = CollectionViewSource.GetDefaultView(new QmsForm.Notice().NoticeDataView);
                NoticeViewFilter = new string[7];
                ClosedFilter = false;
                FormTypeFilter = true;
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                LoadAsync = LoadAsyncDelegate.BeginInvoke(((DataView)NoticeView.SourceCollection).RowFilter, false, new AsyncCallback(ViewLoaded), null);
            }
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
                    ((DataView)NoticeView.SourceCollection).RowFilter = _filterStr;
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
            ExcelWriter.ExportData(((DataView)NoticeView.SourceCollection).ToTable());
        }

        #endregion

        #region Loading Async Delegation Implementation

        public void ViewLoading(string filter, bool refresh)
        {
            try
            {
                if (refresh)
                {
                    NoticeView = CollectionViewSource.GetDefaultView(new QmsForm.Notice().NoticeDataView);
                }
                SearchFilter = !string.IsNullOrEmpty(SearchFilter) ? SearchFilter : string.Empty;
                ((DataView)NoticeView.SourceCollection).RowFilter = filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QMS Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            NoticeView.GroupDescriptions.Clear();
            NoticeView.GroupDescriptions.Add(new PropertyGroupDescription("RevisionDateTime", new DateGroupConverter()));
            if (NoticeView != null && ((DataView)NoticeView.SourceCollection).Count > 0)
            {
                var _temp = ((DataView)NoticeView.SourceCollection).Count >= SelectedIndex ? NoticeView.MoveCurrentToPosition(SelectedIndex) : NoticeView.MoveCurrentToFirst();
                SelectedNcr = _temp ? (DataRowView)NoticeView.CurrentItem : null;
            }
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
                var _filter = NoticeView != null ? ((DataView)NoticeView.SourceCollection).RowFilter : string.Empty;
                LoadAsync = LoadAsyncDelegate.BeginInvoke(_filter, true, new AsyncCallback(ViewLoaded), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QMS Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
