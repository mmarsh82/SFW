using SFW.Controls;
using SFW.Converters;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SFW.QMS.Notice
{
    public class ViewModel : ScheduleBase
    {
        #region Properties

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
                CollectionView = new ListCollectionView(new DataView());
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
                Initialize();
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
        /// Reset the filter after a log in event
        /// </summary>
        public override void ResetFilter()
        {
            SelectedTypeFilter = TypeDictionary.FirstOrDefault();
            Filter($"[Site] = {App.SiteNumber}", 6);
            if (CollectionView != null)
            {
                ((DataView)CollectionView.SourceCollection).RowFilter = GetFilter();
            }
            Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
        }

        /// <summary>
        /// Intialization for the schedule data
        /// </summary>
        public override void Initialize()
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
                CollectionView = new ListCollectionView(_tempTable);
                if (CollectionView.GroupDescriptions.Count() != 0)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
                }
                Application.Current?.Dispatcher.Invoke(new Action(delegate
                {
                    CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("RevisionDateTime", new DateGroupConverter()));
                }));
                CollectionView.CurrentChanged += CollectionView_ItemChanged;
                OnPropertyChanged(nameof(CollectionView));
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QMS Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public override void Refresh()
        {
            try
            {
                var _index = -1;
                var _ncrId = 0;
                if (CollectionView.CurrentItem != null)
                {
                    _ncrId = (int)((DataRowView)CollectionView.CurrentItem).Row.ItemArray[0];
                }
                Initialize();
                if (_ncrId > 0)
                {
                    foreach (DataRowView item in CollectionView)
                    {
                        if ((int)item.Row.ItemArray[0] == _ncrId)
                        {
                            _index = CollectionView.IndexOf(item);
                            break;
                        }
                    }
                }
                if (CollectionView != null && _index > 0)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.MoveCurrentToPosition(_index); }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\nPlease contact IT with this error.", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
