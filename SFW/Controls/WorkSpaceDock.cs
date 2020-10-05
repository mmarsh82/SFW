﻿using DocumentFormat.OpenXml.Drawing.Charts;
using M2kClient;
using SFW.Queries;
using System;
using System.Linq;
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
        public static DockPanel ClosedDock { get; set; }
        public static DockPanel CountDock { get; set; }
        public static bool ClosedView { get; set; }

        #endregion

        /// <summary>
        /// WorkSpaceDock constructor
        /// </summary>
        public WorkSpaceDock()
        {
            RefreshTimer.IsRefreshing = true;
            ClosedView = false;
            //Create the Control
            MainDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;
            CsiDock = new DockPanel();
            WccoDock = new DockPanel();
            ClosedDock = new DockPanel();
            CountDock = new DockPanel();


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

            //Add the Cycle Count View to [4]
            CountDock.Children.Insert(0, new CycleCount.Sched_View());
            CountDock.Children.Insert(1, new CycleCount.Form_View { DataContext = new CycleCount.Form_ViewModel() });
            MainDock.Children.Insert(4, CountDock);

            //Add the Part Detail View to [5]
            MainDock.Children.Insert(5, new UserControl());

            //Add the Schedule View to [6]
            ClosedDock.Children.Insert(0, new Schedule.Closed.View());
            ClosedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            MainDock.Children.Insert(6, ClosedDock);

            //Add the Part Detail View to [7]
            MainDock.Children.Insert(7, new UserControl());

            //Add the Part Trace View to [8]
            MainDock.Children.Insert(8, new PartTrace_View());

            //Set up and display the intial view
            var _siteNbr = CurrentUser.GetSite();
            var _site = !string.IsNullOrEmpty(CurrentUser.Site) ? CurrentUser.Site : "CSI";
            App.DatabaseChange($"{_site}_MAIN");
            App.ErpCon.DatabaseChange(Enum.TryParse(_site, out Database _db) ? _db : Database.CSI);
            SwitchView(_siteNbr, null);
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
            ClosedView = index == 6;
            var _tempDock = new DockPanel();
            switch(index)
            {
                case 0:
                    _tempDock = CsiDock;
                    break;
                case 1:
                    _tempDock = WccoDock;
                    break;
                case 6:
                    _tempDock = ClosedDock;
                    break;
            }
            if (index <= 1)
            {
                MainWindowViewModel.MachineList = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineList;
                MainWindowViewModel.DefaultMachineList = Model.Machine.GetMachineList(App.AppSqlCon, false, true);
                if (App.DefualtWorkCenter.Count > 0)
                {
                    foreach (var m in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)))
                    {
                        MainWindowViewModel.DefaultMachineList.FirstOrDefault(o => o.MachineNumber == m.MachineNumber).IsLoaded = true;
                    }
                    if (MainWindowViewModel.DefaultMachineList.Count(o => o.IsLoaded) == 0)
                    {
                        MainWindowViewModel.DefaultMachineList[0].IsLoaded = true;
                    }
                }
                if (MainWindowViewModel.SelectedMachine == null && !App.IsFocused)
                {
                    MainWindowViewModel.SelectedMachine = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineList[0];
                }
                MainWindowViewModel.MachineGroupList = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineGroupList;
                if (MainWindowViewModel.SelectedMachineGroup == null && !App.IsFocused)
                {
                    MainWindowViewModel.SelectedMachineGroup = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineGroupList[0];
                }
            }
            else if (index == 4)
            {
                ((CycleCount.Sched_View)_tempDock.Children[0]).DataContext = dataContext;
                if (((CycleCount.Form_View)_tempDock.Children[1]).DataContext != null)
                {
                    ((CycleCount.Form_ViewModel)((CycleCount.Form_View)_tempDock.Children[1]).DataContext).CountTran = new Model.Count();
                }
            }
            else if (index == 6)
            {
                ((Schedule.Closed.View)_tempDock.Children[0]).DataContext = dataContext;
                if(((ShopRoute.View)_tempDock.Children[1]).DataContext != null)
                {
                    ((ShopRoute.ViewModel)((ShopRoute.View)_tempDock.Children[1]).DataContext).ShopOrder = new Model.WorkOrder();
                }
            }
            else if (dataContext != null)
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
