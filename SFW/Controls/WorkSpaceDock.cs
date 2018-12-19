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

        public static Grid MainDock { get; set; }
        public static DockPanel CsiDock { get; set; }
        public static DockPanel WccoDock { get; set; }

        #endregion

        /// <summary>
        /// WorkSpaceDock constructor
        /// </summary>
        public WorkSpaceDock()
        {
            RefreshTimer.IsRefreshing = true;
            //Create the Control
            MainDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;
            CsiDock = new DockPanel();
            WccoDock = new DockPanel();

            //Add the CSI Schedule View to [0]
            CsiDock.Children.Insert(0, new Schedule.View());
            CsiDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            MainDock.Children.Insert(0, CsiDock);

            //Add the Schedule View to [1]
            App.DatabaseChange("WCCO_MAIN");
            WccoDock.Children.Insert(0, new Schedule.View());
            WccoDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            MainDock.Children.Insert(1, WccoDock);


            //Add the Scheduler View to [2]
            MainDock.Children.Insert(2, new Scheduler.View { DataContext = new Scheduler.ViewModel() });

            //Add the Part Info View to [3]
            MainDock.Children.Insert(3, new PartInfo_View());
            //Add the Part Info View to [4]
            MainDock.Children.Insert(4, new WipHist_View());
            switch (Environment.UserDomainName)
            {
                case "AD":
                    App.DatabaseChange("WCCO_MAIN");
                    App.ErpCon.DatabaseChange(Database.WCCO);
                    SwitchView(1, null);
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
            foreach (object o in MainDock.Children)
            {
                if (o.GetType() == typeof(DockPanel))
                {
                    ((DockPanel)o).Visibility = Visibility.Collapsed;
                }
                else
                {
                    ((UserControl)o).Visibility = Visibility.Collapsed;
                }
            }
            MainDock.Children[index].Visibility = Visibility.Visible;
            var _tempDock = index == 0 ? CsiDock : WccoDock;
            if (index <= 1)
            {
                MainWindowViewModel.MachineList = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineList;
                MainWindowViewModel.SelectedMachine = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineList[0];
                MainWindowViewModel.MachineGroupList = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineGroupList;
                MainWindowViewModel.SelectedMachineGroup = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineGroupList[0];
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
