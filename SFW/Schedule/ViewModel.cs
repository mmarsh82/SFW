using M2kClient;
using SFW.Commands;
using SFW.Controls;
using SFW.Converters;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    var _wo = new WorkOrder(value.Row, App.AppSqlCon);
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

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        public List<Machine> MachineList { get; set; }
        public List<string> MachineGroupList { get; set; }

        public string VMDataBase { get; set; }

        private RelayCommand _priChange;
        private RelayCommand _orderPri;

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
            //TODO:need to switch to using this on the ICollectionView collection view source
            var test = new ObservableCollection<DataRow>(Machine.GetScheduleData(App.AppSqlCon).AsEnumerable());
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
                    OnPropertyChanged(nameof(ScheduleView));
                    ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
                    if (_oldItem != null && ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().Any(r => r.Field<string>("WO_Number") == ((DataRowView)_oldItem).Row.Field<string>("WO_Number")))
                    {
                        var schedList = ((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().ToList();
                        var listIndex = schedList.FindIndex(r => r.Field<string>("WO_Number") == ((DataRowView)_oldItem).Row.Field<string>("WO_Number"));
                        ScheduleView.MoveCurrentToPosition(listIndex);
                    }
                    else
                    {
                        ScheduleView.MoveCurrentToPosition(-1);
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
                    }
                    RefreshTimer.IsRefreshing = IsLoading = false;
                    ScheduleView.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        #region Priority Change ICommand

        public ICommand PriChangeICommand
        {
            get
            {
                if (_priChange == null)
                {
                    _priChange = new RelayCommand(PriChangeExecute, PriChangeCanExecute);
                }
                return _priChange;
            }
        }

        private void PriChangeExecute(object parameter)
        {
            var _oldPri = SelectedWorkOrder?.Row?.ItemArray[8].ToString();
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
        private bool PriChangeCanExecute(object parameter) => true;

        #endregion

        #region Order Change ICommand

        public ICommand OrderChangeICommand
        {
            get
            {
                if (_orderPri == null)
                {
                    _orderPri = new RelayCommand(OrderChangeExecute, OrderChangeCanExecute);
                }
                return _orderPri;
            }
        }

        private void OrderChangeExecute(object parameter)
        {
            var _oldPri = ((DataRowView)parameter).Row.ItemArray[15].ToString();
            var _woNumber = ((DataRowView)parameter).Row.ItemArray[0].ToString().Split('*')[0];
            var _changeRequest = M2kCommand.EditRecord("WP", _woNumber, 195, parameter.ToString(), App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
                SelectedWorkOrder.BeginEdit();
                SelectedWorkOrder["PriTime"] = _oldPri;
                SelectedWorkOrder.EndEdit();
            }
            else
            {
                SelectedWorkOrder.BeginEdit();
                SelectedWorkOrder["PriTime"] = ((DataRowView)parameter).Row.ItemArray[15].ToString();
                SelectedWorkOrder.EndEdit();
            }
        }
        private bool OrderChangeCanExecute(object parameter) => true;

        #endregion
    }
}
