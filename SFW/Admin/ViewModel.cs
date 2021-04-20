using SFW.Helpers;
using SFW.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SFW.Admin
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private BindingList<UserConfig> mCon;
        public BindingList<UserConfig> MachineConfig
        {
            get { return mCon; }
            set { mCon = value; OnPropertyChanged(nameof(MachineConfig)); }
        }

        public ObservableCollection<Machine> MachineCollection { get; set; }

        RelayCommand _listCom;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewModel()
        {
            MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(App.AppSqlCon, false, false));
            MachineCollection.Insert(0, new Machine { MachineName = "" });
            MachineConfig = new BindingList<UserConfig>(App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber).ToList());
            MachineConfig.ListChanged += MachineConfig_ListChanged;
        }

        /// <summary>
        /// Triggers anytime the MachineConfig list changes
        /// </summary>
        /// <param name="sender">Values in the change</param>
        /// <param name="e">All the change informtion</param>
        private void MachineConfig_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.NewIndex == 0)
            {
                MachineCollection.Remove(MachineCollection.FirstOrDefault(o => o.MachineNumber == ((UserConfig)sender).MachineNumber));
            }
        }

        #region List ICommands

        public ICommand ListICommand
        {
            get
            {
                if (_listCom == null)
                {
                    _listCom = new RelayCommand(ListExecute, ListCanExecute);
                }
                return _listCom;
            }
        }

        private void ListExecute(object parameter)
        {
            if (parameter.GetType() == typeof(UserConfig))
            {
                MachineConfig.Remove((UserConfig)parameter);
            }
            else
            {
                switch (parameter.ToString())
                {
                    case "Add":
                        MachineConfig.Add(new UserConfig { SiteNumber = App.SiteNumber, Position = MachineConfig.Count + 1 });
                        break;
                    case "Save":
                        foreach (var m in MachineConfig.Where(o => o.SiteNumber == App.SiteNumber))
                        {

                        }
                        break;
                }
            }
        }
        private bool ListCanExecute(object parameter)
        {
            if (parameter == null)
            {
                return true;
            }
            else
            {
                switch (parameter.ToString())
                {
                    case "Add":
                        return MachineConfig.Count(o => string.IsNullOrEmpty(o.MachineNumber)) == 0;
                    case "Save":
                        return MachineConfig.Count(o => string.IsNullOrEmpty(o.MachineNumber)) == 0
                            && MachineConfig.GroupBy(x => x.MachineNumber).All(y => y.Count() == 1)
                            && MachineConfig.Count(o => o.Position == 0) == 0
                            && MachineConfig.GroupBy(x => x.Position).All(y => y.Count() == 1);
                    default:
                        return true;
                }
            }
        }

        #endregion

        /*#region Selected Default Machine ICommand

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

        #endregion*/
    }
}
