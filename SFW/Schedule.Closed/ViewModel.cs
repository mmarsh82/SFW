using SFW.Controls;
using SFW.Converters;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Data;

//Created by Michael Marsh 4-11-19

namespace SFW.Schedule.Closed
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
                    var _wo = new WorkOrder(value.Row, App.AppSqlCon);
                    WorkSpaceDock.ClosedDock.Children.RemoveAt(1);
                    WorkSpaceDock.ClosedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) });
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
                ((DataView)ScheduleView.SourceCollection).Table.Search(value);
                _sFilter = value;
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        public List<Machine> MachineList { get; set; }
        public List<string> MachineGroupList { get; set; }

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
            ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetClosedScheduleData(App.AppSqlCon));
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
    }
}
