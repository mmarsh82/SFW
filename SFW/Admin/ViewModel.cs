using SFW.Helpers;
using System;
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

        public ObservableCollection<Model.Production.Machine> MachineCollection { get; set; }

        public ObservableCollection<string> MachineGroupCollection { get; set; }

        private string _selMachGrp;
        public string SelectedMachineGroup
        {
            get
            { return _selMachGrp; }
            set
            {
                if (value != null && !string.IsNullOrEmpty(value))
                {
                    var _groupList = Model.Production.Machine.GetList(false, false, 1).Where(o => o.MachineGroup == value);
                    var _tranList = MachineConfig.Where(o => !string.IsNullOrEmpty(o.MachineNumber)).ToList();
                    MachineConfig.Clear();
                    foreach (var _item in _tranList)
                    {
                        MachineConfig.Add(_item);
                    }
                    foreach (var _mach in _groupList)
                    {
                        if (MachineConfig.Count(o => o.MachineNumber == _mach.MachineNumber) == 0)
                        {
                            MachineConfig.Add(new UserConfig { MachineNumber = _mach.MachineNumber, Position = MachineConfig.Count()+1, SiteNumber = App.SiteNumber });
                        }
                    }
                }
                _selMachGrp = MachineGroupCollection[0];
                OnPropertyChanged(nameof(SelectedMachineGroup));
                OnPropertyChanged(nameof(MachineGroupCollection));
            }
        }

        RelayCommand _listCom;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewModel()
        {
            MachineCollection = new ObservableCollection<Model.Production.Machine>(Model.Production.Machine.GetList(false, false, App.SiteNumber).OrderBy(o => o.MachineName));
            MachineCollection.Insert(0, new Model.Production.Machine { MachineName = "" });
            MachineGroupCollection = new ObservableCollection<string>(Model.Production.Machine.GetGroupList(false, 1));
            MachineGroupCollection.Insert(0, "");
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
            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                var _counter = 1;
                foreach (var _item in ((BindingList<UserConfig>)sender))
                {
                    _item.Position = _counter;
                    _counter++;
                }
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
                        UserConfig.UpdateFile(MachineConfig.ToList(), App.IsFocused);
                        App.DefualtWorkCenter = UserConfig.GetList();
                        var _userName = CurrentUser.DomainUserName;
                        CurrentUser.LogOff();
                        CurrentUser.LogIn(_userName);
                        System.Windows.MessageBox.Show($"All changes have been saved to the User config file located at;\n{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SfwConfig.xml", "Saved Changes");
                        break;
                    case "Default":

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
    }
}
