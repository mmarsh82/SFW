using M2kClient;
using SFW.Controls;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

//Created by Michael Marsh 4-19-18

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        private static List<Machine> _mList;
        public static List<Machine> MachineList
        {
            get { return _mList; }
            set { _mList = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList))); }
        }

        public static string MachineFilter;
        private static Machine mach;
        public static Machine SelectedMachine
        {
            get { return mach; }
            set
            {
                var _dock = App.SiteNumber == 0
                            ? WorkSpaceDock.CsiDock
                            : WorkSpaceDock.WccoDock;
                if (value == null)
                {
                    value = MachineList?.FirstOrDefault(o => o.MachineName == "All");
                }
                if (mach != value && !IsChanging)
                {
                    IsChanging = true;
                    if (value.MachineGroup != SelectedMachineGroup)
                    {
                        SelectedMachineGroup = value.MachineGroup;
                    }
                    MachineFilter = value.MachineName == "All" ? "" : $"MachineNumber = '{value.MachineNumber}'";
                    if (WorkSpaceDock.ClosedView)
                    {
                        WorkSpaceDock.ClosedDock.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ClosedScheduleView != null)
                            {
                                if (((DataView)((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ClosedScheduleView.SourceCollection).Table.Select($"MachineNumber = '{value.MachineNumber}'").Length == 0)
                                {
                                    ((ShopRoute.ViewModel)((ShopRoute.View)WorkSpaceDock.ClosedDock.Children[1]).DataContext).ShopOrder = new WorkOrder();
                                }
                                ((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).SearchFilter = null;
                                ((DataView)((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ClosedScheduleView.SourceCollection).RowFilter = string.IsNullOrEmpty(MachineFilter) 
                                    ? App.ViewFilter[App.SiteNumber]
                                    : MachineFilter;
                                ((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ClosedScheduleView.Refresh();
                            }
                        }));
                    }
                    else
                    {
                        if (((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView != null)
                        {
                            if (((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).Table.Select($"MachineNumber = '{value.MachineNumber}'").Length == 0)
                            {
                                ((ShopRoute.ViewModel)((ShopRoute.View)_dock.Children[1]).DataContext).ShopOrder = new WorkOrder();
                            }
                            ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).SearchFilter = null;
                            ((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = string.IsNullOrEmpty(MachineFilter)
                                ? App.ViewFilter[App.SiteNumber]
                                : MachineFilter;
                            ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.Refresh();
                        }
                    }
                    IsChanging = false;
                }
                //Schedule.SalesOrder.ViewModel.FilterSchedule(MachineFilter, 3);
                mach = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SelectedMachine)));
            }
        }

        private static List<string> _mGrpList;
        public static List<string> MachineGroupList
        {
            get { return _mGrpList; }
            set { _mGrpList = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineGroupList))); }
        }

        private static string machGrp;
        public static string SelectedMachineGroup
        {
            get { return machGrp; }
            set
            {
                if (machGrp != value && !IsChanging)
                {
                    IsChanging = true;
                    var _temp = value == "All" ? "" : $"MachineGroup = '{value}'";
                    if (WorkSpaceDock.ClosedView)
                    {
                        WorkSpaceDock.ClosedDock.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ScheduleView != null)
                            {
                                ((DataView)((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ClosedScheduleView.SourceCollection).RowFilter = _temp;
                                ((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).SearchFilter = null;
                            }
                        }));
                    }
                    else
                    {
                        var _dock = App.SiteNumber == 0
                            ? WorkSpaceDock.CsiDock
                            : WorkSpaceDock.WccoDock;
                        if (((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView != null)
                        {
                            ((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = _temp;
                            ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).SearchFilter = null;
                        }
                    }
                    SelectedMachine = MachineList.FirstOrDefault(o => o.MachineName == "All");
                    IsChanging = false;
                }
                machGrp = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SelectedMachineGroup)));
            }
        }

        public string Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        private bool cUpdate;
        public bool CanUpdate
        {
            get { return cUpdate; }
            set { cUpdate = value; OnPropertyChanged(nameof(CanUpdate)); }
        }

        private static bool iTraining;
        public static bool InTraining
        {
            get { return iTraining; }
            set
            {
                if (value)
                {
                    if (Enum.TryParse($"{App.ErpCon.Database}TRAIN", out Database db))
                    {
                        App.ErpCon.DatabaseChange(db);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("There is currently no train account set up for your ERP database.\nPlease contact the IT administrator for further help.", "No Train Database", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        value = false;
                    }
                }
                else if (!value && App.AppSqlCon.Database.Replace('_', '.') != $"{App.ErpCon.Database}.MAIN")
                {
                    if (Enum.TryParse(App.AppSqlCon.Database.Replace('_', '.'), out Database db))
                    {
                        App.ErpCon.DatabaseChange(db);
                    }
                }
                iTraining = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InTraining)));
            }
        }

        private static bool IsChanging;
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public event EventHandler CanExecuteChanged;

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            IsChanging = false;
            CanUpdate = false;
            InTraining = false;
            new WorkSpaceDock();
            RefreshTimer.Add(MainUpdate);
        }

        /// <summary>
        /// Updates all the static properties in the MainWindow View components when other views require new data
        /// </summary>
        public void UpdateProperties()
        {
            MachineList = Machine.GetMachineList(App.AppSqlCon, true, false);
            SelectedMachine = MachineList.First();
            MachineGroupList = Machine.GetMachineGroupList(App.AppSqlCon, true);
            SelectedMachineGroup = MachineGroupList.First();
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList)));
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineGroupList)));
        }

        /// <summary>
        /// Checks to see if there are any application updates available
        /// </summary>
        public void MainUpdate()
        {
            try
            {
                var _ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var _dirList = Directory.GetDirectories($"{App.AppFilePath}Application Files\\");
                foreach (var d in _dirList)
                {
                    if (_ver < new Version(Path.GetFileName(d).Remove(0, 4).Replace('_', '.')))
                    {
                        CanUpdate = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
