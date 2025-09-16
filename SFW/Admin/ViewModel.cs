using SFW.Controls;
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

        public ObservableCollection<string> MachineCollection { get; set; }

        RelayCommand _listCom;
        RelayCommand _listOrder;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewModel()
        {
            var _noNameList = App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)).OrderBy(o => o.Position).ToList();
            foreach (var _noNameMach in _noNameList)
            {
                _noNameMach.MachineName = Model.Production.Machine.GetName(_noNameMach.MachineNumber, 'M');
            }
            MachineConfig = new BindingList<UserConfig>(_noNameList);
            var _filteredMachList = Model.Production.Machine.GetNameList(false, 1).Except(MachineConfig.Select(o => o.MachineName).ToList());
            MachineCollection = new ObservableCollection<string>(_filteredMachList);
            MachineConfig.ListChanged += MachineConfig_ListChanged;
        }

        /// <summary>
        /// Triggers anytime the MachineConfig list changes
        /// </summary>
        /// <param name="sender">Values in the change</param>
        /// <param name="e">All the change informtion</param>
        private void MachineConfig_ListChanged(object sender, ListChangedEventArgs e)
        {
            ((BindingList<UserConfig>)sender).RaiseListChangedEvents = false;
            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                var _counter = 1;
                foreach (var _item in (BindingList<UserConfig>)sender)
                {
                    _item.Position = _counter;
                    _counter++;
                }
            }
            ((BindingList<UserConfig>)sender).RaiseListChangedEvents = true;
            OnPropertyChanged(nameof(MachineConfig));
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
                    case "Save":
                        UserConfig.UpdateFile(MachineConfig.ToList(), App.IsFocused);
                        App.DefualtWorkCenter = UserConfig.GetList(true);
                        var _userName = CurrentUser.DomainUserName;
                        CurrentUser.LogOff();
                        CurrentUser.LogIn(_userName);
                        System.Windows.MessageBox.Show($"All changes have been saved to the User config file located at;\n{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SfwConfig.xml", "Saved Changes");
                        break;
                    case "Reset":
                        ((View)WorkSpaceDock.MainDock.Children[4]).DataContext = new ViewModel();
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

        #region List Order ICommands

        public ICommand ListOrderICommand
        {
            get
            {
                if (_listOrder == null)
                {
                    _listOrder = new RelayCommand(ListOrderExecute);
                }
                return _listOrder;
            }
        }

        private void ListOrderExecute(object parameter)
        {
            if (parameter != null && parameter.ToString().Contains('*'))
            {
                var _machCon = MachineConfig.FirstOrDefault(o => o.MachineName == parameter.ToString().Split('*')[0]);
                var _newPos = parameter.ToString().Split('*')[1] == "U" ? _machCon.Position-1 : _machCon.Position+1;
                MachineConfig.Remove(_machCon);
                MachineConfig.Insert((int)_newPos-1, _machCon);
                foreach (var _item in MachineConfig)
                {
                    MachineConfig[MachineConfig.IndexOf(_item)].Position = MachineConfig.IndexOf(_item)+1;
                }
            }
        }

        #endregion
    }
}
