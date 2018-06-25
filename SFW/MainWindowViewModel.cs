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

        public static List<Machine> MachineList { get; set; }
        private static Machine mach;
        public static Machine SelectedMachine
        {
            get { return mach; }
            set
            {
                if (value == null)
                {
                    mach = value = MachineList.First();
                }
                else
                {
                    mach = value;
                    if (SelectedMachineGroup != Machine.GetMachineGroup(App.AppSqlCon, value.MachineNumber))
                    {
                        SelectedMachineGroup = null;
                    }
                    var _temp = value.MachineName == "All" ? "" : $"MachineName = '{value.MachineName}'";
                    ((DataView)((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.MainDock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = _temp;
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SelectedMachine)));
            }
        }

        public static List<string> MachineGroupList { get; set; }
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
                    SelectedMachine = null;
                    var _temp = value == "All" ? "" : $"MachineGroup = '{value}'";
                    ((DataView)((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.MainDock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = _temp;
                }
                else
                {
                    machGrp = value;
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SelectedMachineGroup)));
            }
        }

        public static string CurrentSite { get { return App.Site; } set { value = App.Site; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentSite))); } }

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
        public static void UpdateProperties()
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
