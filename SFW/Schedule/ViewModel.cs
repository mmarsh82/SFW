using SFW.Controls;
using SFW.Converters;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;

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
                    ((ShopRoute.ViewModel)((ShopRoute.View)WorkSpaceDock.MainDock.Children[App.SiteNumber + 1]).DataContext).ShopOrder = new WorkOrder(value.Row, App.AppSqlCon);
                }
                OnPropertyChanged(nameof(SelectedWorkOrder));
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        public List<Machine> MachineList { get; set; }
        public List<string> MachineGroupList { get; set; }

        public string VMDataBase { get; set; }

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(null, new AsyncCallback(ViewLoaded), null);
            MachineList = Machine.GetMachineList(App.AppSqlCon, true);
            MachineGroupList = Machine.GetMachineGroupList(App.AppSqlCon, true);
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
        }
        public void ViewLoaded(IAsyncResult r)
        {
            IsLoading = false;
            OnPropertyChanged(nameof(ScheduleView));
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            if (!IsLoading)
            {
                var _db = string.Empty;
                IsLoading = true;
                if (App.AppSqlCon.Database != VMDataBase)
                {
                    _db = App.AppSqlCon.Database;
                    App.SqlCon_DataBaseChange(VMDataBase);
                }
                var _selection = SelectedWorkOrder;
                ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(App.AppSqlCon));
                ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
                if (MainWindowViewModel.SelectedMachine?.MachineName != "All" && string.IsNullOrEmpty(_db))
                {
                    MainWindowViewModel.SelectedMachine = MainWindowViewModel.SelectedMachine;
                    MainWindowViewModel.SelectedMachineGroup = MainWindowViewModel.SelectedMachineGroup;
                    ((DataView)ScheduleView.SourceCollection).RowFilter = $"MachineName = '{MainWindowViewModel.SelectedMachine.MachineName}'";
                }
                else if (MainWindowViewModel.SelectedMachineGroup != "All" && string.IsNullOrEmpty(_db))
                {
                    MainWindowViewModel.SelectedMachine = MainWindowViewModel.MachineList[0];
                    MainWindowViewModel.SelectedMachineGroup = MainWindowViewModel.SelectedMachineGroup;
                    ((DataView)ScheduleView.SourceCollection).RowFilter = $"MachineGroup = '{MainWindowViewModel.SelectedMachineGroup}'";
                }
                OnPropertyChanged(nameof(ScheduleView));
                SelectedWorkOrder = _selection;
                if (!string.IsNullOrEmpty(_db))
                {
                    App.SqlCon_DataBaseChange(_db);
                }
                IsLoading = false;
            }
        }
    }
}
