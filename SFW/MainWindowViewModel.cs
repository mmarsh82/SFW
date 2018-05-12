using SFW.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        public static DockPanel WorkSpaceDock { get; set; }
        public List<Machine> MachineList { get; set; }

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
    }
}
