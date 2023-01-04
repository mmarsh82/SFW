using M2kClient;
using SFW.Controls;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

//Created by Michael Marsh 4-19-18

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        private static List<string> _mList;
        public static List<string> MachineList
        {
            get { return _mList; }
            set { _mList = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList))); }
        }

        private static string mach;
        public static string SelectedMachine
        {
            get { return mach; }
            set
            {
                if (value == null)
                {
                    value = MachineList[0];
                }
                if (mach != value && !IsChanging)
                {
                    IsChanging = true;
                    var _mGroup = Machine.GetMachineGroup(value, 'M');
                    if (_mGroup != SelectedMachineGroup)
                    {
                        SelectedMachineGroup = _mGroup;
                    }
                    var _mNbr = Machine.GetMachineNumber(value);
                    Schedule.ViewModel.ScheduleFilter(value == "All" ? "" : $"MachineNumber = '{_mNbr}'", 1);
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
                    Schedule.ViewModel.ScheduleFilter(value == "All" ? "" : $"[MachineGroup] = '{value}'", 2);
                    SelectedMachine = MachineList.FirstOrDefault(o => o == "All");
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
                //TODO: need a training database
                //App.ErpCon.DatabaseChange(Database.WCCOTRAIN, App.SiteNumber);
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

        public static bool Initialization;
        private static bool IsChanging;
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public event EventHandler CanExecuteChanged;

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            try
            {
                UpdateProperties();
                IsChanging = false;
                CanUpdate = false;
                InTraining = false;
                CanFilter = !App.IsFocused;
                new WorkSpaceDock();
                RefreshTimer.Add(MainUpdate);
                Initialization = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Main Window\n{ex.Message}", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                Initialization = false;
            }
        }

        /// <summary>
        /// Updates all the static properties in the MainWindow View components when other views require new data
        /// </summary>
        public static void UpdateProperties()
        {
            try
            {
                DisplayAction = false;
                InTraining = false;
                MachineList = Machine.GetMachineList(true);
                SelectedMachine = MachineList.First();
                MachineGroupList = Machine.GetMachineGroupList(true);
                SelectedMachineGroup = MachineGroupList.First();
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList)));
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineGroupList)));
                if (CurrentUser.BasicUser)
                {
                    Schedule.ViewModel.ScheduleFilter(UserConfig.BuildMachineFilter(), 1);
                    Schedule.ViewModel.ScheduleFilter(UserConfig.BuildPriorityFilter(), 3);
                    CanFilter = !App.IsFocused;
                }
                else
                {
                    if (!Initialization)
                    {
                        Schedule.ViewModel.ClearFilter();
                        CanFilter = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"UpdateProperties\n{ex.Message}\n{ex.StackTrace}", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
