using M2kClient;
using SFW.Queries;
using System;
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
            RefreshTimer.IsRefreshing = true;
            //Create the Control
            MainDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;

            //Add the CSI Schedule View to [0,1]
            MainDock.Children.Insert(0, new Schedule.View());
            MainDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });

            //Add the Schedule View to [2,3]
            App.DatabaseChange("WCCO_MAIN");
            MainDock.Children.Insert(2, new Schedule.View());
            MainDock.Children.Insert(3, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });


            //Add the Scheduler View to [4]
            MainDock.Children.Insert(4, new Scheduler.View { DataContext = new Scheduler.ViewModel() });

            //Add the Part Info View to [5]
            MainDock.Children.Insert(5, new PartInfo_View());
            switch (Environment.UserDomainName)
            {
                case "AD":
                    App.DatabaseChange("WCCO_MAIN");
                    App.ErpCon.DatabaseChange(Database.WCCO);
                    SwitchView(2, null);
                    break;
                case "CSI":
                    App.DatabaseChange("CSI_MAIN");
                    App.ErpCon.DatabaseChange(Database.CSI);
                    SwitchView(0, null);
                    break;
            }
            RefreshTimer.IsRefreshing = false;
        }

        /// <summary>
        /// Switch to a different view inside of the MainDock's children
        /// </summary>
        /// <param name="index">Child object index</param>
        /// <param name="dataContext">DataContext to attached to the loaded child object</param>
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
