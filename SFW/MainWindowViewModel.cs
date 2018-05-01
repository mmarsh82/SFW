using SFW.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        public static Grid WorkSpaceGrid { get; set; }
        public List<Machine> MachineList { get; set; }

        #endregion

        public MainWindowViewModel()
        {
            if (WorkSpaceGrid == null)
            {
                WorkSpaceGrid = ((MainWindow)Application.Current.Windows[0]).WorkSpaceGrid;
                WorkSpaceGrid.Children.Add(new Schedule.View());
            }
            if (MachineList == null)
            {
                MachineList = Machine.GetMachineList(App.AppSqlCon);
            }
        }
    }
}
