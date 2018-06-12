﻿using SFW.Converters;
using SFW.Model;
using System;
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
                    ((ShopRoute.ViewModel)((ShopRoute.View)MainWindowViewModel.WorkSpaceDock.Children[1]).DataContext).ShopOrder = new WorkOrder(value.Row, App.AppSqlCon);
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
        public IAsyncResult LoadAsyncComplete { get; set; }

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(null, new AsyncCallback(ViewLoaded), null);
        }

        /// <summary>
        /// Schedule ViewModel constructor for loading in a specific workcenter
        /// </summary>
        /// <param name="machineNumber">Machine Number to load into the schedule</param>
        public ViewModel(string machineNumber)
        {
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(machineNumber, new AsyncCallback(ViewLoaded), null);
        }

        #region Loading Async Delegation Implementation

        public void ViewLoading(string machineNbr)
        {
            IsLoading = true;
            ScheduleView = string.IsNullOrEmpty(machineNbr) 
                ? CollectionViewSource.GetDefaultView(Machine.GetScheduleData(App.AppSqlCon))
                : CollectionViewSource.GetDefaultView(Machine.GetScheduleData(App.AppSqlCon, machineNbr));
            ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(Machine.GetMachineList(App.AppSqlCon))));
        }
        public void ViewLoaded(IAsyncResult r)
        {
            IsLoading = false;
            OnPropertyChanged(nameof(ScheduleView));
        }

        #endregion
    }
}
