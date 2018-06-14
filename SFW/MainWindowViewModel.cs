using SFW.Controls;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        public static List<Machine> MachineList { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        public MainWindowViewModel()
        {
            new WorkSpaceDock();
            if (MachineList == null)
            {
                MachineList = Machine.GetMachineList(App.AppSqlCon);
            }
        }

        /// <summary>
        /// Updates all the static properties in the MainWindow View components when other views require new data
        /// </summary>
        public static void UpdateProperties()
        {
            MachineList = Machine.GetMachineList(App.AppSqlCon);
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(MachineList)));
        }
    }
}
