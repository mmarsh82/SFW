using M2kClient;
using SFW.Commands;
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

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public ICollectionView ScheduleView { get; set; }

        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                _selectedWO = value;
                if (value != null)
                {
                    var _wo = new WorkOrder(value.Row, App.SiteNumber, App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI, App.AppSqlCon);
                    if (App.SiteNumber == 0)
                    {
                        if (!int.TryParse(_wo.EngStatus, out int i))
                        {
                            Controls.WorkSpaceDock.UpdateChildDock(0, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) });
                        }
                        else
                        {
                            Controls.WorkSpaceDock.UpdateChildDock(0, 1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) });
                        }
                    }
                    else
                    {
                        Controls.WorkSpaceDock.UpdateChildDock(0, 1, new ShopRoute.ViewModel(_wo));
                    }
                }
                else
                {
                    Controls.WorkSpaceDock.UpdateChildDock(0, 1, new ShopRoute.ViewModel());
                }
                OnPropertyChanged(nameof(SelectedWorkOrder));
            }
        }

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
                var _tempFilter = string.Empty;
                if (!string.IsNullOrEmpty(_sFilter))
                {
                    _tempFilter = ((DataView)ScheduleView.SourceCollection).Table.SearchRowFilter(_sFilter);
                    if (!string.IsNullOrEmpty(((DataView)ScheduleView.SourceCollection).RowFilter.Replace(_tempFilter, "")))
                    {
                        _tempFilter = $" AND ({_tempFilter})";
                    }
                }
                if (!string.IsNullOrEmpty(value))
                {
                    var _oldFilter = _sFilter != null && ((DataView)ScheduleView.SourceCollection).RowFilter.Contains(_tempFilter)
                        ? ((DataView)ScheduleView.SourceCollection).RowFilter.Replace(_tempFilter, "")
                        : ((DataView)ScheduleView.SourceCollection).RowFilter;
                    var _sRowFilter = ((DataView)ScheduleView.SourceCollection).Table.SearchRowFilter(value);
                    ((DataView)ScheduleView.SourceCollection).RowFilter = !string.IsNullOrEmpty(_oldFilter)
                        ? $"{_oldFilter} AND ({_sRowFilter})"
                        :_sRowFilter;
                }
                else if ((MainWindowViewModel.SelectedMachine != null && MainWindowViewModel.SelectedMachine.MachineName != "All") || (!string.IsNullOrEmpty(_sFilter) && string.IsNullOrEmpty(value)))
                {
                    if (!string.IsNullOrEmpty(_tempFilter))
                    {
                        ((DataView)ScheduleView.SourceCollection).RowFilter = ((DataView)ScheduleView.SourceCollection).RowFilter.Replace(_tempFilter, "");
                    }
                }
                _sFilter = value == "" ? null : value;
                OnPropertyChanged(nameof(SearchFilter));
                ScheduleView.Refresh();
            }
        }

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        public List<Machine> MachineList { get; set; }
        public List<string> MachineGroupList { get; set; }

        public static bool UserRefresh { get; set; }

        private RelayCommand _stateChange;
        private RelayCommand _priChange;

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            MachineList = Machine.GetMachineList(App.AppSqlCon, true, false);
            MachineGroupList = MachineList.Where(o => !string.IsNullOrEmpty(o.MachineGroup)).Select(o => o.MachineGroup).Distinct().ToList();
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            App.ViewFilter[App.SiteNumber] = BuildFilter();
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(App.ViewFilter[App.SiteNumber], new AsyncCallback(ViewLoaded), null);
            RefreshTimer.Add(RefreshSchedule);
            UserRefresh = false;
        }

        /// <summary>
        /// Schedule ViewModel constructor for loading in a specific workcenter
        /// </summary>
        /// <param name="machineNumber">Machine Number to load into the schedule</param>
        public ViewModel(string machineNumber)
        {
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(machineNumber, new AsyncCallback(ViewLoaded), null);
        }

        /// <summary>
        /// Builds the orginal filter for the Schedule view based on the application user config file
        /// </summary>
        /// <returns>DataTable filter string</returns>
        public string BuildFilter()
        {
            var _filter = string.Empty;
            if (App.DefualtWorkCenter?.Count(o => o.SiteNumber == App.SiteNumber) == 1 && !string.IsNullOrEmpty(App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber))
            {
                _filter = $@"MachineNumber = '{App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber}'";
            }
            else if (App.DefualtWorkCenter?.Count(o => o.SiteNumber == App.SiteNumber) > 1)
            {
                foreach (var m in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber))
                {
                    _filter += string.IsNullOrEmpty(_filter) ? $"(MachineNumber = '{m.MachineNumber}'" : $" OR MachineNumber = '{m.MachineNumber}'";
                }
                _filter += ")";
            }
            if (App.IsFocused && string.IsNullOrEmpty(_filter))
            {
                _filter = "WO_Priority = 'A' OR WO_Priority = 'B'";
            }
            else if (App.IsFocused)
            {
                _filter += " AND (WO_Priority = 'A' OR WO_Priority = 'B')";
            }
            return _filter;
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
            IsLoading = true;
            ViewLoading(filter);
        }

        public void ViewLoading(string filter)
        {
            IsLoading = true;
            ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(UserConfig.GetIROD(), App.AppSqlCon));
            ScheduleView.SortDescriptions.Add(new SortDescription("MachineOrder", ListSortDirection.Ascending));
            ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
            if (!string.IsNullOrEmpty(filter))
            {
                ((DataView)ScheduleView.SourceCollection).RowFilter = filter;
                OnPropertyChanged(nameof(ScheduleView));
            }
        }
        public void ViewLoaded(IAsyncResult r)
        {
            IsLoading = false;
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
                if (!IsLoading)
                {
                    var _oldItem = ScheduleView.CurrentItem;
                    var _oldFilter = ((DataView)ScheduleView.SourceCollection).RowFilter;
                    if (UserRefresh)
                    {
                        _oldItem = null;
                        if (App.DefualtWorkCenter.Count == 0)
                        {
                            _oldFilter = string.Empty;
                        }
                        else
                        {
                            _oldFilter = BuildFilter();
                        }
                        UserRefresh = false;
                    }
                    RefreshTimer.IsRefreshing = IsLoading = true;
                    if (App.Site != $"{CurrentUser.Site}_MAIN")
                    {
                        MachineList = Machine.GetMachineList(App.AppSqlCon, true, false);
                        MachineGroupList = MachineList.Where(o => !string.IsNullOrEmpty(o.MachineGroup)).Select(o => o.MachineGroup).Distinct().ToList();
                    }
                    ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(UserConfig.GetIROD(), App.AppSqlCon));
                    ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
                    if (_oldItem != null && ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().Any(r => r.Field<string>("WO_Number") == ((DataRowView)_oldItem).Row.Field<string>("WO_Number")))
                    {
                        var _index = ScheduleView.IndexOf(_oldItem, "Wo_Number");  
                        ScheduleView.MoveCurrentToPosition(_index);
                    }
                    else
                    {
                        ScheduleView.MoveCurrentToPosition(-1);
                        SelectedWorkOrder = null;
                    }
                    ((DataView)ScheduleView.SourceCollection).RowFilter = _oldFilter;
                    RefreshTimer.IsRefreshing = IsLoading = false;
                    ScheduleView.SortDescriptions.Add(new SortDescription("MachineOrder", ListSortDirection.Ascending));
                    OnPropertyChanged(nameof(ScheduleView));
                    ScheduleView.Refresh();
                    if (!string.IsNullOrEmpty(SearchFilter))
                    {
                        SearchFilter = SearchFilter;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Order State Change ICommand

        public ICommand StateChangeICommand
        {
            get
            {
                if (_stateChange == null)
                {
                    _stateChange = new RelayCommand(StateChangeExecute, StateChangeCanExecute);
                }
                return _stateChange;
            }
        }

        private void StateChangeExecute(object parameter)
        {
            var _oldPri = SelectedWorkOrder?.Row?.SafeGetField<string>("WO_Priority").ToString();
            if (char.TryParse(SelectedWorkOrder?.Row?.SafeGetField<string>("WO_Priority").ToString(), out char _oldPriChar))
            {
                var _oldPriInt = _oldPriChar % 32;
                var _newPriInt = Convert.ToChar(parameter) % 32;
                if (_oldPriInt < _newPriInt && (SelectedWorkOrder?.Row?.SafeGetField<string>("PriTime").ToString() != "999" || SelectedWorkOrder?.Row?.SafeGetField<int>("Sched_Priority").ToString() != "999"))
                {
                    new ClearPriority().Execute(SelectedWorkOrder);
                }
            }
            if (!string.IsNullOrEmpty(parameter?.ToString()))
            {
                var _woNumber = SelectedWorkOrder?.Row?.SafeGetField<string>("WO_Number").ToString().Split('*')[0];
                var _changeRequest = M2kCommand.EditRecord("WP", _woNumber, 40, parameter.ToString(), App.ErpCon);
                if (!string.IsNullOrEmpty(_changeRequest))
                {
                    MessageBox.Show(_changeRequest, "ERP Record Error");
                    SelectedWorkOrder.BeginEdit();
                    SelectedWorkOrder["WO_Priority"] = _oldPri;
                    SelectedWorkOrder.EndEdit();
                }
                else
                {
                    SelectedWorkOrder.BeginEdit();
                    SelectedWorkOrder["WO_Priority"] = parameter.ToString();
                    SelectedWorkOrder.EndEdit();
                }
            }
        }
        private bool StateChangeCanExecute(object parameter) => true;

        #endregion

        #region Priority Change ICommand

        public ICommand PriorityChangeICommand
        {
            get
            {
                if (_priChange == null)
                {
                    _priChange = new RelayCommand(PriorityChangeExecute, PriorityChangeCanExecute);
                }
                return _priChange;
            }
        }

        private void PriorityChangeExecute(object parameter)
        {
            var _shift = ((DataRowView)parameter).Row.SafeGetField<int>("PriTime").ToString() == "9" ? 0 : Convert.ToInt32(((DataRowView)parameter).Row.SafeGetField<int>("PriTime"));
            var _pri = ((DataRowView)parameter).Row.SafeGetField<int>("Sched_Priority").ToString() == "9" ? 0 : Convert.ToInt32(((DataRowView)parameter).Row.SafeGetField<int>("Sched_Priority"));
            var _woNumber = ((DataRowView)parameter).Row.SafeGetField<string>("WO_Number").ToString().Split('*')[0];
            using (var _editPri = new Tools.PriorityEdit_ViewModel(_woNumber, _shift, _pri))
            {
                new Tools.PriorityEdit_View { DataContext = _editPri }.ShowDialog();
            }
        }
        private bool PriorityChangeCanExecute(object parameter) => true;

        #endregion
    }
}
