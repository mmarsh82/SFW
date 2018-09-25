using SFW.Controls;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

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

        private static Machine mach;
        public static Machine SelectedMachine
        {
            get { return mach; }
            set
            {
                if (value == null && MachineList != null)
                {
                    mach = MachineList.First();
                }
                else
                {
                    mach = value;
                    if (SelectedMachineGroup != null && SelectedMachineGroup != Machine.GetMachineGroup(App.AppSqlCon, value.MachineNumber))
                    {
                        SelectedMachineGroup = null;
                    }
                    var _temp = value.MachineName == "All" ? "" : $"MachineName = '{value.MachineName}'";
                    ((DataView)((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.MainDock.Children[App.SiteNumber]).DataContext).ScheduleView.SourceCollection).RowFilter = _temp;
                }
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
                if (value == null && MachineGroupList != null)
                {
                    machGrp = value = MachineGroupList.First(o => o.Contains(Machine.GetMachineGroup(App.AppSqlCon, SelectedMachine.MachineNumber)));
                }
                else if (machGrp != value)
                {
                    var _temp = value == "All" ? "" : $"MachineGroup = '{value}'";
                    ((DataView)((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.MainDock.Children[App.SiteNumber]).DataContext).ScheduleView.SourceCollection).RowFilter = _temp;
                    if (SelectedMachine != null &&  value != Machine.GetMachineGroup(App.AppSqlCon, SelectedMachine.MachineNumber))
                    {
                        SelectedMachine = null;
                    }
                    machGrp = value;
                }
                else
                {
                    machGrp = value;
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SelectedMachineGroup)));
            }
        }

        public static string CurrentSite
        {
            get
            { return App.Site; }
            set
            {
                value = App.Site;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentSite)));
            }
        }

        public static int CurrentSiteNbr
        {
            get
            { return App.SiteNumber; }
            set
            {
                value = App.SiteNumber;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentSiteNbr)));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            new WorkSpaceDock();
            if (MachineList == null)
            {
                MachineList = Machine.GetMachineList(App.AppSqlCon, true);
                SelectedMachine = MachineList.First();
            }
            if (MachineGroupList == null)
            {
                MachineGroupList = Machine.GetMachineGroupList(App.AppSqlCon, true);
                SelectedMachineGroup = MachineGroupList.First();
            }
        }

        /// <summary>
        /// Updates all the static properties in the MainWindow View components when other views require new data
        /// </summary>
        public void UpdateProperties()
        {
            MachineList = Machine.GetMachineList(App.AppSqlCon, true);
            SelectedMachine = MachineList.First();
            MachineGroupList = Machine.GetMachineGroupList(App.AppSqlCon, true);
            SelectedMachineGroup = MachineGroupList.First();
            CurrentSite = App.Site;
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList)));
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineGroupList)));
        }
    }
}
