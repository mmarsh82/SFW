using SFW.Helpers;
using SFW.Controls;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using M2kClient;

//Created by Michael Marsh 4-19-18

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties
       
        private static List<Machine> dMach;
        public static List<Machine> DefaultMachineList
        {
            get { return dMach; }
            set { dMach = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DefaultMachineList))); }
        }

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
                    var _filter = value.MachineName == "All" ? "" : $"MachineName = '{value.MachineName}'";
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
                                ((DataView)((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).ClosedScheduleView.SourceCollection).RowFilter = _filter;
                                ((Schedule.Closed.ViewModel)((Schedule.Closed.View)WorkSpaceDock.ClosedDock.Children[0]).DataContext).SearchFilter = null;
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
                            ((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = _filter;
                            ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).SearchFilter = null;
                            ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.Refresh();
                        }
                    }
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
                else if (!value && App.AppSqlCon.Database.Replace('_', '.') != $"{App.ErpCon.Database.ToString()}.MAIN")
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

        private static bool _lowView;
        public static bool LimitedView
        {
            get { return _lowView; }
            set 
            {
                _lowView = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LimitedView)));
            }
        }

        private static bool IsChanging;
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public event EventHandler CanExecuteChanged;
        RelayCommand _selectedDefault;
        RelayCommand _setFocus;

        #endregion

        /// <summary>
        /// Main Window ViewModel Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            DefaultMachineList = Machine.GetMachineList(App.AppSqlCon, false, true);
            if (App.DefualtWorkCenter.Count > 0)
            {
                foreach (var m in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)))
                {
                    DefaultMachineList.FirstOrDefault(o => o.MachineNumber == m.MachineNumber).IsLoaded = true;
                }
            }
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
            DefaultMachineList = Machine.GetMachineList(App.AppSqlCon, false, true);
            SelectedMachine = MachineList.First();
            MachineGroupList = Machine.GetMachineGroupList(App.AppSqlCon, true);
            SelectedMachineGroup = MachineGroupList.First();
            CurrentSite = App.Site;
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

        #region Selected Default Machine ICommand

        public ICommand SelectedDefaultICommand
        {
            get
            {
                if (_selectedDefault == null)
                {
                    _selectedDefault = new RelayCommand(SelectedDefaultExecute, SelectedDefaultCanExecute);
                }
                return _selectedDefault;
            }
        }

        private void SelectedDefaultExecute(object parameter)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var xDoc = XDocument.Load($"{folder}\\SFW\\SfwConfig.xml");
            var xEle = xDoc.Descendants($"Default_WC").Single();
            var _machine = (Machine)parameter;
            if (_machine.MachineName == "None")
            {
                foreach (var x in xEle.Elements($"Site_{App.SiteNumber}"))
                {
                    x.Remove();
                }
                xEle.Add(new XElement($"Site_{App.SiteNumber}", new XAttribute("WC_Nbr", ""), new XAttribute("Position", "1")));
                App.DefualtWorkCenter.Clear();
                foreach (var m in DefaultMachineList.Where(o => o.MachineNumber == "0"))
                {
                    m.IsLoaded = false;
                }
            }
            else
            {
                if (App.DefualtWorkCenter.Count(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)) == 0)
                {
                    var siteEle = xEle.Element($"Site_{App.SiteNumber}");
                    siteEle.Attribute("WC_Nbr").Value = _machine.MachineNumber;
                    siteEle.Attribute("Position").Value = "1";
                    App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber = _machine.MachineNumber;
                    SelectedMachine = MachineList.FirstOrDefault(o => o.MachineNumber == _machine.MachineNumber);
                }
                else
                {
                    if (App.DefualtWorkCenter.Count(o => o.MachineNumber == _machine.MachineNumber && o.SiteNumber == App.SiteNumber) > 0)
                    {
                        foreach (var x in xEle.Elements($"Site_{App.SiteNumber}"))
                        {
                            if (x.Attribute("WC_Nbr").Value == _machine.MachineNumber)
                            {
                                x.Remove();
                            }
                        }
                        App.DefualtWorkCenter.Remove(App.DefualtWorkCenter.FirstOrDefault(o => o.MachineNumber == _machine.MachineNumber));
                    }
                    else
                    {
                        xEle.Add(new XElement($"Site_{App.SiteNumber}", new XAttribute("WC_Nbr", _machine.MachineNumber), new XAttribute("Position", (App.DefualtWorkCenter.Count(o => o.SiteNumber == App.SiteNumber) + 1).ToString())));
                        App.DefualtWorkCenter.Add(new UserConfig { MachineNumber = _machine.MachineNumber, Position = App.DefualtWorkCenter.Count(o => o.SiteNumber == App.SiteNumber) + 1, SiteNumber = App.SiteNumber });
                    }
                    var _filter = string.Empty;
                    foreach (var m in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber))
                    {
                        _filter += string.IsNullOrEmpty(_filter) ? $"MachineNumber = {m.MachineNumber}" : $" OR MachineNumber = {m.MachineNumber}";
                    }
                    if (App.DefualtWorkCenter.Count(o => o.SiteNumber == App.SiteNumber) > 1)
                    {
                        _filter = _filter.Insert(0, "(");
                        _filter += ")";
                    }
                    if (App.IsFocused)
                    {
                        _filter += " AND (WO_Priority = 'A' OR WO_Priority = 'B')";
                    }
                    var _dock = App.SiteNumber == 0 ? WorkSpaceDock.CsiDock : WorkSpaceDock.WccoDock;
                    ((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = _filter;
                    MachineList.Insert(0, new Machine { MachineName = "Custom", MachineNumber = "1" });
                    SelectedMachine = MachineList[0];
                    DefaultMachineList.FirstOrDefault(o => o.MachineNumber == "0").IsLoaded = false;
                }
            }
            DefaultMachineList.FirstOrDefault(o => o.MachineNumber == _machine.MachineNumber).IsLoaded = true;
            OnPropertyChanged(nameof(DefaultMachineList));
            xDoc.Save($"{folder}\\SFW\\SfwConfig.xml");
            new Commands.ViewLoad().Execute("Schedule");
        }
        private bool SelectedDefaultCanExecute(object parameter) => true;

        #endregion

        #region Set Focus View ICommand

        public ICommand SetFocusICommand
        {
            get
            {
                if (_setFocus == null)
                {
                    _setFocus = new RelayCommand(SetFocusExecute, SetFocusCanExecute);
                }
                return _setFocus;
            }
        }

        public static void SetFocusExecute(object parameter)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var xDoc = XDocument.Load($"{folder}\\SFW\\SfwConfig.xml");
            var xEle = xDoc.Descendants($"Default_View").Single();
            xEle.Attribute("Focus").Value = App.IsFocused.ToString();
            xDoc.Save($"{folder}\\SFW\\SfwConfig.xml");
        }
        private bool SetFocusCanExecute(object parameter) => true;

        #endregion
    }
}
