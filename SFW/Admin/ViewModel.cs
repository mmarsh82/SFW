using SFW.Helpers;
using SFW.Model;
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
                        UserConfig.UpdateConfigFile(MachineConfig.ToList(), App.IsFocused);
                        System.Windows.MessageBox.Show($"All changes have been saved to the User config file located at;\n{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SfwConfig.xml", "Saved Changes");
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
