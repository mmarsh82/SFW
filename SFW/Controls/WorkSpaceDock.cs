using SFW.Queries;
using System.Windows;
using System.Windows.Controls;

//Created by Michael Marsh 06-20-18

namespace SFW.Controls
{
    public class WorkSpaceDock
    {
        #region Properties

        public static Grid MainDock { get; set; }
        public static DockPanel SchedDock { get; set; }
        public static DockPanel ClosedDock { get; set; }
        public static DockPanel CountDock { get; set; }
        public static DockPanel SalesDock { get; set; }
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
            SchedDock = new DockPanel();
            ClosedDock = new DockPanel();
            CountDock = new DockPanel();
            SalesDock = new DockPanel();

            //Add the Site Schedule View to [0]
            SchedDock.Children.Insert(0, new Schedule.View());
            SchedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            MainDock.Children.Insert(0, SchedDock);

            //Add a spacer to [1]
            MainDock.Children.Insert(1, new UserControl());

            //Add the Part Info View to [2]
            MainDock.Children.Insert(2, new PartInfo_View());

            //Add the Cycle Count View to [3]
            if(CurrentUser.IsInventoryControl)
            {
                CountDock.Children.Insert(0, new CycleCount.Sched_View());
                CountDock.Children.Insert(1, new CycleCount.Form_View { DataContext = new CycleCount.Form_ViewModel() });
            }
            MainDock.Children.Insert(3, CountDock);

            //Add the Admin View to [4]
            MainDock.Children.Insert(4, new Admin.View { DataContext = new Admin.ViewModel() });

            //Add the Closed Schedule View to [5]
            ClosedDock.Children.Insert(0, new Schedule.Closed.View());
            ClosedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            MainDock.Children.Insert(5, ClosedDock);

            //Add the Part Detail View to [6]
            MainDock.Children.Insert(6, new UserControl());

            //Add the Part Trace View to [7]
            MainDock.Children.Insert(7, new PartTrace_View());

            //Add the Sales Order Schedule View to [8]
            if (CurrentUser.HasSalesOrderModule)
            {
                SalesDock.Children.Insert(0, new Schedule.SalesOrder.View());
                SalesDock.Children.Insert(1, new ShopRoute.SalesOrder.View { DataContext = new ShopRoute.SalesOrder.ViewModel() });
            }
            MainDock.Children.Insert(8, SalesDock);

            SwitchView(App.SiteNumber, null);
            RefreshTimer.IsRefreshing = false;
        }

        /// <summary>
        /// Switch to a different view inside of the MainDock's children
        /// </summary>
        /// <param name="index">Child object index</param>
        /// <param name="dataContext">DataContext to attached to the loaded child object</param>
        /// <param name="refresh">Optional: triggers a refresh on the docked views</param>
        public static void SwitchView(int index, object dataContext, bool refreshDock = true)
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
            ClosedView = index == 5;
            var _tempDock = new DockPanel();
            switch(index)
            {
                case 0:
                case 1:
                    _tempDock = SchedDock;
                    index = 0;
                    break;
                case 3:
                    _tempDock = CountDock;
                    break;
                case 5:
                    _tempDock = ClosedDock;
                    break;
                case 8:
                    _tempDock = SalesDock;
                    break;
            }
            if (index <= 1 && refreshDock)
            {
                ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).RefreshSchedule();
                MainWindowViewModel.MachineList = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineList;
                if (MainWindowViewModel.SelectedMachine == null && !App.IsFocused)
                {
                    MainWindowViewModel.SelectedMachine = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineList[0];
                }
                MainWindowViewModel.MachineGroupList = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineGroupList;
                if (MainWindowViewModel.SelectedMachineGroup == null && !App.IsFocused)
                {
                    MainWindowViewModel.SelectedMachineGroup = ((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).MachineGroupList[0];
                }
                MainDock.Children.RemoveAt(3);
                MainDock.Children.Insert(3, new Admin.View { DataContext = new Admin.ViewModel() });
                MainDock.Children[3].Visibility = Visibility.Collapsed;
            }
            else if (index == 5)
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
            MainDock.Children[index].Visibility = Visibility.Visible;
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

        /// <summary>
        /// Update a child component of an internal work space dockpanel
        /// </summary>
        /// <param name="parentUCIndex">Parent UserControl Index</param>
        /// <param name="childUCIndex">Child Usercontrol Index</param>
        /// <param name="viewModel">Child DataContext as a ViewModelBase object</param>
        public static void UpdateChildDock(int parentUCIndex, int childUCIndex, ViewModelBase viewModel)
        {
            if (MainDock.Children[parentUCIndex].GetType() == typeof(DockPanel))
            {
                try
                {
                    ((UserControl)((DockPanel)MainDock.Children[parentUCIndex]).Children[childUCIndex]).DataContext = viewModel;
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Update a child component of an internal work space dockpanel
        /// </summary>
        /// <param name="parentUCIndex">Parent UserControl Index</param>
        /// <param name="childUCIndex">Child Usercontrol Index</param>
        /// <param name="userCtrl">Child View to load</param>
        public static void UpdateChildDock(int parentUCIndex, int childUCIndex, UserControl userCtrl)
        {
            if (MainDock.Children[parentUCIndex].GetType() == typeof(DockPanel))
            {
                try
                {
                    ((DockPanel)MainDock.Children[parentUCIndex]).Children.RemoveAt(childUCIndex);
                    ((DockPanel)MainDock.Children[parentUCIndex]).Children.Insert(childUCIndex, userCtrl);

                }
                catch
                {

                }
            }
        }
    }
}
