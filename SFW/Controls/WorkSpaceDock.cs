using SFW.Queries;
using System.Windows;
using System.Windows.Controls;

//Created by Michael Marsh 06-20-18

namespace SFW.Controls
{
    public class WorkSpaceDock
    {
        #region Properties

        public static DockPanel MainDock { get; set; }

        #endregion

        /// <summary>
        /// WorkSpaceDock constructor
        /// </summary>
        /// <param name="dp"></param>
        public WorkSpaceDock()
        {
            var s = App.AppSqlCon.Database;
            //Create the Control
            MainDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;

            //Add the CSI Schedule View to [0,1]
            if (s != "CSI_MAIN")
            {
                App.AppSqlCon.ChangeDatabase("CSI_MAIN");
            }
            MainDock.Children.Insert(0, new Schedule.View());
            MainDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });

            //Add the Schedule View to [2,3]
            App.AppSqlCon.ChangeDatabase("WCCO_MAIN");
            MainDock.Children.Insert(2, new Schedule.View());
            MainDock.Children.Insert(3, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });


            //Add the Scheduler View to [4]
            MainDock.Children.Insert(4, new Scheduler.View { DataContext = new Scheduler.ViewModel() });
            MainDock.Children[4].Visibility = Visibility.Collapsed;

            //Add the Part Info View to [5]
            MainDock.Children.Insert(5, new PartInfo_View());
            MainDock.Children[5].Visibility = Visibility.Collapsed;

            App.AppSqlCon.ChangeDatabase(s);
            if (s == "CSI_MAIN")
            {
                MainDock.Children[2].Visibility = Visibility.Hidden;
                MainDock.Children[3].Visibility = Visibility.Hidden;
            }
            else
            {
                MainDock.Children[0].Visibility = Visibility.Hidden;
                MainDock.Children[1].Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Switch to a different view inside of the MainDock's children
        /// </summary>
        /// <param name="index">Child object index</param>
        /// <param name="dataContext">DataContext to attached to the loaded chile object</param>
        public static void SwitchView(int index, object dataContext)
        {
            foreach (UserControl uc in MainDock.Children)
            {
                uc.Visibility = Visibility.Collapsed;
            }
            MainDock.Children[index].Visibility = Visibility.Visible;
            if (index == 0 || index == 2)
            {
                MainDock.Children[index + 1].Visibility = Visibility.Visible;
                MainWindowViewModel.MachineList = ((Schedule.ViewModel)((Schedule.View)MainDock.Children[index]).DataContext).MachineList;
                MainWindowViewModel.MachineGroupList = ((Schedule.ViewModel)((Schedule.View)MainDock.Children[index]).DataContext).MachineGroupList;
            }
            if (dataContext != null)
            {
                ((UserControl)MainDock.Children[index]).DataContext = dataContext;
            }
        }

        /// <summary>
        /// Refresh the views in the MainDock
        /// </summary>
        public static void RefreshMainDock()
        {
            ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock.Children.Clear();
            MainDock = null;
            new WorkSpaceDock();
        }
    }
}
