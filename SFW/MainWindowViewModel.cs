using M2kClient;
using SFW.Controls;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static Machine mach;
        public static Machine SelectedMachine
        {
            get { return mach; }
            set
            {
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
                    Schedule.ViewModel.ScheduleFilter(value.MachineName == "All" ? "" : $"MachineNumber = '{value.MachineNumber}'", 1);
                    Schedule.Closed.ViewModel.ScheduleFilter(value.MachineName == "All" ? "" : $"MachineNumber = '{value.MachineNumber}'", 1);
                    IsChanging = false;
                }
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
                    Schedule.ViewModel.ScheduleFilter(value == "All" ? "" : $"MachineGroup = '{value}'", 2);
                    Schedule.Closed.ViewModel.ScheduleFilter(value == "All" ? "" : $"MachineGroup = '{value}'", 2);
                    SelectedMachine = MachineList.FirstOrDefault(o => o.MachineName == "All");
                    IsChanging = false;
                }
                machGrp = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SelectedMachineGroup)));
            }
        }

        public string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

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

        private static bool dAct;
        public static bool DisplayAction
        {
            get { return dAct; }
            set
            {
                dAct = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DisplayAction)));
            }
        }

        private static bool canFltr;
        public static bool CanFilter
        {
            get { return canFltr; }
            set
            {
                canFltr = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanFilter)));
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
            UpdateProperties();
            IsChanging = false;
            CanUpdate = false;
            InTraining = false;
            CanFilter = !App.IsFocused;
            new WorkSpaceDock();
            RefreshTimer.Add(MainUpdate);
        }

        /// <summary>
        /// Updates all the static properties in the MainWindow View components when other views require new data
        /// </summary>
        public static void UpdateProperties()
        {
            DisplayAction = false;
            InTraining = false;
            MachineList = Machine.GetMachineList(App.AppSqlCon, true, false);
            SelectedMachine = MachineList.First();
            MachineGroupList = Machine.GetMachineGroupList(App.AppSqlCon, true);
            SelectedMachineGroup = MachineGroupList.First();
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList)));
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineGroupList)));
            if (CurrentUser.BasicUser)
            {
                Schedule.ViewModel.ScheduleFilter(UserConfig.BuildMachineFilter(), 1);
                Schedule.ViewModel.ScheduleFilter(UserConfig.BuildMachineFilter(), 3);
                Schedule.Closed.ViewModel.ScheduleFilter(UserConfig.BuildMachineFilter(), 1);
                Schedule.Closed.ViewModel.ScheduleFilter(UserConfig.BuildPriorityFilter(), 3);
                CanFilter = !App.IsFocused;
            }
            else
            {
                Schedule.ViewModel.ClearFilter();
                Schedule.Closed.ViewModel.ClearFilter();
                CanFilter = true;
            }
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
