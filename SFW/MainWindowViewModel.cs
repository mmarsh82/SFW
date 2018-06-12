using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        public static DockPanel WorkSpaceDock { get; set; }
        public static List<Machine> MachineList { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        public MainWindowViewModel()
        {
            if (WorkSpaceDock == null)
            {
                WorkSpaceDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;
                WorkSpaceDock.Children.Add(new Schedule.View());
                WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            }
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
