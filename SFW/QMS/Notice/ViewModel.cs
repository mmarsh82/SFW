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

        public static string[] ViewFilter;
        public ICollectionView CollectionView { get; set; }
        public KeyValuePair<string, string> SelectedItemFilter;

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : ((DataView)CollectionView.SourceCollection).Table.SearchRowFilter(value);
                Filter(_filter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        public IDictionary<string, object> TypeDictionary { get; set; }
        private KeyValuePair<string, object> _selType;
        public KeyValuePair<string, object> SelectedTypeFilter
        {
            get
            { return _selType; }
            set
            {
                _selType = value;
                if (value.Value.GetType() == typeof(string[]))
                {
                    var _tempFilter = (string[])value.Value;
                    if (_tempFilter.Length == 4)
                    {
                        Filter(_tempFilter[0], int.TryParse(_tempFilter[1], out int f) ? f : 1);
                        Filter(_tempFilter[2], int.TryParse(_tempFilter[3], out int i) ? i : 3);
                    }
                }
                OnPropertyChanged(nameof(SelectedTypeFilter));
            }
        }

        private bool _site;
        public bool SiteFilter
        {
            get { return _site; }
            set
            {
                var _filter = $"[Site] = {App.SiteNumber}";
                Filter(_filter, 2);
                _site = value;
                OnPropertyChanged(nameof(SiteFilter));
            }
        }

        RelayCommand _newFrm;
        RelayCommand _exportQms;

        #endregion

        /// <summary>
        /// Notice ViewModel Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ModelBase.MasterDataSet.Tables.Contains(typeof(Model.Quality.Notice).Name))
            {
                ApplicationTimer.ActionList.Add(Refresh);
                CollectionView = CollectionViewSource.GetDefaultView(new DataView());
                TypeDictionary = new Dictionary<string, object>
                {
                    { "All", new string[4]{ "", "1", "", "3" } }
                    ,{"All NCR", new string[4]{ "", "1", "[FormType] = 'NCR'", "3"} }
                    ,{"Open NCR", new string[4]{ "[FormStatus] <> 'Closed'", "1", "[FormType] = 'NCR'", "3"} }
                    ,{"Closed NCR", new string[4]{ "[FormStatus] = 'Closed'", "1", "[FormType] = 'NCR'", "3"} }
                    ,{"All SCAR", new string[4]{ "", "1", "[FormType] = 'SCAR'", "3"} }
                    ,{"Open SCAR", new string[4]{ "[FormStatus] <> 'Closed'", "1", "[FormType] = 'SCAR'", "3"} }
                    ,{"Closed SCAR", new string[4]{ "[FormStatus] = 'Closed'", "1", "[FormType] = 'SCAR'", "3"} }
                };
                ResetFilter();
                Refresh();
            }
        }

        /// <summary>
        /// Tracks the item selections from the CollectionView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionView_ItemChanged(object sender, EventArgs e)
        {
            try
            {
                if ((App.LoadedModule == Enumerations.UsersControls.Quality))
                {
                    var _dRow = (DataRowView)CollectionView.CurrentItem;
                    if (_dRow != null)
                    {
                        SelectedItemFilter = new KeyValuePair<string, string>(_dRow.Row.Field<int>("NcrId").ToString(), "NcrId");
                        var _ncr = new Model.Quality.QmsForm(_dRow.Row.Field<int>("NcrId"));
                        var _action = new Action(delegate { WorkSpaceDock.UpdateChildDock(9, 1, new Form.ViewModel(_ncr, _dRow.Row.Field<int>("RevisionFilter"))); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                }
            }
            catch (Exception)
            { }
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
        public void Filter(string filter, int index)
        {
            if (ViewFilter == null)
            {
                ViewFilter = new string[7];
            }
            ViewFilter[index] = filter;
            var _filterStr = string.Empty;
            foreach (var s in ViewFilter.Where(o => !string.IsNullOrEmpty(o)))
            {
                _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
            }
            var _tempList = new List<DataView>();
            if (CollectionView != null)
            {
                ((DataView)CollectionView.SourceCollection).RowFilter = _filterStr;
                OnPropertyChanged(nameof(CollectionView));
            }
        }

        /// <summary>
        /// Reset the filter after a log in event
        /// </summary>
        public void ResetFilter()
        {
            SelectedTypeFilter = TypeDictionary.FirstOrDefault();
            Filter($"[Site] = {App.SiteNumber}", 6);
            if (CollectionView != null)
            {
                ((DataView)CollectionView.SourceCollection).RowFilter = GetFilter();
            }
        }

        /// <summary>
        /// Used to get the current filter string
        /// </summary>
        /// <returns></returns>
        public string GetFilter()
        {
            var _filterStr = string.Empty;
            foreach (var s in ViewFilter.Where(o => !string.IsNullOrEmpty(o)))
            {
                _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
            }
            return _filterStr;
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
            ApplicationTimer.Pause();
            WorkSpaceDock.UpdateChildDock(9, 1, new Form.View { DataContext = new Form.ViewModel(null, false, true, Model.Quality.FormType.NCR) });
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
            ExcelWriter.ExportData(((DataView)CollectionView.SourceCollection).ToTable());
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void Refresh()
        {
            try
            {
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.DeferRefresh(); }));
                var _tempTable = ModelBase.MasterDataSet.Tables.Contains(typeof(Model.Quality.Notice).Name)
                    ? ModelBase.MasterDataSet.Tables[typeof(Model.Quality.Notice).Name].Select("[NcrRevisionId] = [RevisionFilter]").CopyToDataTable().AsDataView()
                    : null;
                if (_tempTable != null)
                {
                    _tempTable.Sort = "RevisionDateTime DESC";
                }
                CollectionView = CollectionViewSource.GetDefaultView(_tempTable);
                if (CollectionView.GroupDescriptions.Count() != 0)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
                }
                Application.Current?.Dispatcher.Invoke(new Action(delegate
                {
                    CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("RevisionDateTime", new DateGroupConverter()));
                }));
                if (CollectionView != null)
                {
                    ((DataView)CollectionView.SourceCollection).RowFilter = GetFilter();
                    var _selectedIndex = ((DataView)CollectionView.SourceCollection).Count > 0 && SelectedItemFilter.Key != null
                        ? CollectionView.IndexOf(SelectedItemFilter.Key, SelectedItemFilter.Value)
                        : -1;
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.MoveCurrentToPosition(_selectedIndex); }));
                }
                CollectionView.CurrentChanged += CollectionView_ItemChanged;
                OnPropertyChanged(nameof(CollectionView));
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QMS Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
