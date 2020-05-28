using M2kClient;
using SFW.Commands;
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
                    var _tempDock = App.SiteNumber == 0 ? WorkSpaceDock.CsiDock : WorkSpaceDock.WccoDock;
                    var _wo = new WorkOrder(value.Row, App.SiteNumber, App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI, App.AppSqlCon);
                    _tempDock.Children.RemoveAt(1);
                    if (!int.TryParse(_wo.EngStatus, out int i))
                    {
                        _tempDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) });
                    }
                    else
                    {
                        _tempDock.Children.Insert(1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) });
                    }
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
                        :_sRowFilter;
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

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        public List<Machine> MachineList { get; set; }
        public List<string> MachineGroupList { get; set; }

        public string VMDataBase { get; set; }

        private RelayCommand _stateChange;
        private RelayCommand _priChange;

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            MachineList = Machine.GetMachineList(App.AppSqlCon, true);
            MachineGroupList = MachineList.Where(o => !string.IsNullOrEmpty(o.MachineGroup)).Select(o => o.MachineGroup).Distinct().ToList();
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            var _filter = App.DefualtWorkCenter?.Count > 0 ? App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber : null;
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, new AsyncCallback(ViewLoaded), null);
            RefreshTimer.Add(RefreshSchedule);
            VMDataBase = App.AppSqlCon.Database;
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
            VMDataBase = App.AppSqlCon.Database;
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
            if (string.IsNullOrEmpty(filter))
            {
                ViewLoading(string.Empty);
            }
            else
            {
                ((DataView)ScheduleView.SourceCollection).RowFilter = $"MachineNumber = '{filter}'";
                OnPropertyChanged(nameof(ScheduleView));
            }
        }

        public void ViewLoading(string machineNbr)
        {
            IsLoading = true;

            ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(App.AppSqlCon));
            ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
            if (!string.IsNullOrEmpty(machineNbr))
            {
                MainWindowViewModel.SelectedMachine = MachineList.FirstOrDefault(o => o.MachineNumber == machineNbr);
                ((DataView)ScheduleView.SourceCollection).RowFilter = $"MachineName = '{MainWindowViewModel.SelectedMachine.MachineName}'";
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
                    var _db = string.Empty;
                    var _oldItem = ScheduleView.CurrentItem;
                    RefreshTimer.IsRefreshing = IsLoading = true;
                    if (App.AppSqlCon.Database != VMDataBase)
                    {
                        _db = App.AppSqlCon.Database;
                        App.DatabaseChange(VMDataBase);
                    }
                    ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(App.AppSqlCon));
                    ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
                    OnPropertyChanged(nameof(ScheduleView));
                    if (_oldItem != null && ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().Any(r => r.Field<string>("WO_Number") == ((DataRowView)_oldItem).Row.Field<string>("WO_Number")))
                    {
                        var schedList = ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().ToList();
                        var listIndex = schedList.FindIndex(r => r.Field<string>("WO_Number") == ((DataRowView)_oldItem).Row.Field<string>("WO_Number"));
                        ScheduleView.MoveCurrentToPosition(listIndex);
                    }
                    if (MainWindowViewModel.SelectedMachine.MachineName != "All" && string.IsNullOrEmpty(_db))
                    {
                        ((DataView)ScheduleView.SourceCollection).RowFilter = $"MachineName = '{MainWindowViewModel.SelectedMachine.MachineName}'";
                    }
                    else if (MainWindowViewModel.SelectedMachineGroup != "All" && string.IsNullOrEmpty(_db))
                    {
                        ((DataView)ScheduleView.SourceCollection).RowFilter = $"MachineGroup = '{MainWindowViewModel.SelectedMachineGroup}'";
                    }
                    if (!string.IsNullOrEmpty(_db))
                    {
                        App.DatabaseChange(_db);
                        MainWindowViewModel.InTraining = MainWindowViewModel.InTraining;
                    }
                    RefreshTimer.IsRefreshing = IsLoading = false;
                    ScheduleView.Refresh();
                    if (!string.IsNullOrEmpty(SearchFilter))
                    {
                        SearchFilter = SearchFilter;
                    }
                }
            }
            catch (Exception)
            { }
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
            var _oldPri = SelectedWorkOrder?.Row?.ItemArray[10].ToString();
            if (char.TryParse(SelectedWorkOrder?.Row?.ItemArray[10].ToString(), out char _oldPriChar))
            {
                var _oldPriInt = _oldPriChar % 32;
                var _newPriInt = Convert.ToChar(parameter) % 32;
                if (_oldPriInt < _newPriInt && (SelectedWorkOrder?.Row?.ItemArray[15].ToString() != "999" || SelectedWorkOrder?.Row?.ItemArray[16].ToString() != "999"))
                {
                    new ClearPriority().Execute(SelectedWorkOrder);
                }
            }
            if (!string.IsNullOrEmpty(parameter?.ToString()))
            {
                var _woNumber = SelectedWorkOrder?.Row?.ItemArray[0].ToString().Split('*')[0];
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
            var _shift = ((DataRowView)parameter).Row.ItemArray[16].ToString() == "9" ? 0 : Convert.ToInt32(((DataRowView)parameter).Row.ItemArray[16]);
            var _pri = ((DataRowView)parameter).Row.ItemArray[17].ToString() == "9" ? 0 : Convert.ToInt32(((DataRowView)parameter).Row.ItemArray[17]);
            var _woNumber = ((DataRowView)parameter).Row.ItemArray[0].ToString().Split('*')[0];
            using (var _editPri = new Tools.PriorityEdit_ViewModel(_woNumber, _shift, _pri))
            {
                new Tools.PriorityEdit_View { DataContext = _editPri }.ShowDialog();
            }
        }
        private bool PriorityChangeCanExecute(object parameter) => true;

        #endregion
    }
}
