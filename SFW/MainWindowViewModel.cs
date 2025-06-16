using SFW.Controls;
using SFW.Model;
using SFW.Model.Production;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                if (value == null && MachineList.Count() > 0)
                {
                    value = MachineList[0];
                }
                if (mach != value && !IsChanging)
                {
                    IsChanging = true;
                    var _mGroup = Machine.GetGroup(value, 'M');
                    if (_mGroup != SelectedMachineGroup)
                    {
                        SelectedMachineGroup = _mGroup;
                    }
                    var _mNbr = Machine.GetNumber(value);
                    WorkSpaceDock.UpdateChildDockMachineFilter(1, 1, value == "All" ? "" : $"[MachineNumber] = '{_mNbr}'");
                    WorkSpaceDock.UpdateChildDockMachineFilter(11, 1, value == "All" ? "" : $"[MachineNumber] = '{_mNbr}'");
                    WorkSpaceDock.UpdateChildDockMachineFilter(9, 1, value == "All" ? "" : $"[FoundWorkCenterId] = '{_mNbr}'");
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
                    WorkSpaceDock.UpdateChildDockMachineFilter(1, 2, value == "All" ? "" : $"[MachineGroup] = '{value}'");
                    WorkSpaceDock.UpdateChildDockMachineFilter(11, 2, value == "All" ? "" : $"[MachineGroup] = '{value}'");
                    WorkSpaceDock.UpdateChildDockMachineFilter(9, 2, value == "All" ? "" : $"[FoundWorkCenterGroup] = '{value}'");
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
        public event EventHandler CanExecuteChanged
        {
            add {  }
            remove { }
        }

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            try
            {
                UpdateProperties(false);
                IsChanging = false;
                CanUpdate = false;
                new WorkSpaceDock();
                ApplicationTimer.ActionList.Add(MainUpdate);
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
        /// <param name="isRefresh">Standard refresh function</param>
        public static void UpdateProperties(bool isRefresh)
        {
            try
            {
                if (!isRefresh && SelectedMachine == null && SelectedMachineGroup == null)
                {
                    MachineList = Machine.GetNameList(true, App.SiteNumber);
                    if (MachineList.Count() > 0)
                    {
                        SelectedMachine = MachineList.First();
                    }
                    MachineGroupList = Machine.GetGroupList(true, App.SiteNumber);
                    SelectedMachineGroup = MachineGroupList.First();
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList)));
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineGroupList)));
                CanFilter = !App.IsFocused;
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
                var _pubVer = new Version(ModelBase.DatabaseVersion);
                if(_ver.Major != _pubVer.Major || _ver.Minor != _pubVer.Minor || _ver.Build != _pubVer.Build || _ver.Revision != _pubVer.Revision)
                {
                    CanUpdate = true;
                }
                else
                {
                    CanUpdate = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
