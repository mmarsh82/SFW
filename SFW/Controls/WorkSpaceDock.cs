using SFW.Queries;
using System;
using System.ComponentModel;
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
        public static DockPanel PurchaseDock { get; set; }
        public static DockPanel CountDock { get; set; }
        public static DockPanel SalesDock { get; set; }
        public static int Module => (int)App.LoadedModule;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public event EventHandler CanExecuteChanged;

        #endregion

        /// <summary>
        /// WorkSpaceDock constructor
        /// </summary>
        public WorkSpaceDock()
        {
            try
            {
                RefreshTimer.IsRefreshing = true;
                //Create the Control
                MainDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;
                SchedDock = new DockPanel();
                PurchaseDock = new DockPanel();
                CountDock = new DockPanel();
                SalesDock = new DockPanel();

                //Add the Part Info View to [0]
                MainDock.Children.Insert(0, new PartInfo_View());

                //Add the Site Schedule View to [1]
                SchedDock.Children.Insert(0, new Schedule.View());
                SchedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                MainDock.Children.Insert(1, SchedDock);
                while (!Schedule.ViewModel.LoadAsyncComplete.IsCompleted) { }

                //Add a spacer to [2]
                MainDock.Children.Insert(2, new UserControl());

                //Add the Cycle Count View to [3]
                CountDock.Children.Insert(0, new CycleCount.Sched_View());
                CountDock.Children.Insert(1, new CycleCount.Form_View { DataContext = new CycleCount.Form_ViewModel() });
                MainDock.Children.Insert(3, CountDock);
                if (CycleCount.Sched_ViewModel.LoadAsyncComplete != null)
                {
                    while (!CycleCount.Sched_ViewModel.LoadAsyncComplete.IsCompleted) { }
                }

                //Add the Admin View to [4]
                MainDock.Children.Insert(4, new Admin.View { DataContext = new Admin.ViewModel() });

                //Add the Purchase Order Schedule View to [5]
                MainDock.Children.Insert(5, new UserControl());
                /*PurchaseDock.Children.Insert(0, new Schedule.PurchaseOrder.View());
                PurchaseDock.Children.Insert(1, new ShopRoute.PurchaseOrder.View { DataContext = new ShopRoute.PurchaseOrder.ViewModel() });
                MainDock.Children.Insert(5, PurchaseDock);
                while (!Schedule.PurchaseOrder.ViewModel.LoadAsyncComplete.IsCompleted) { }*/

                //Add the Part Detail View to [6]
                MainDock.Children.Insert(6, new UserControl());

                //Add the Part Trace View to [7]
                MainDock.Children.Insert(7, new PartTrace_View());

                //Add the Sales Order Schedule View to [8]
                SalesDock.Children.Insert(0, new Schedule.SalesOrder.View());
                SalesDock.Children.Insert(1, new ShopRoute.SalesOrder.View { DataContext = new ShopRoute.SalesOrder.ViewModel() });
                MainDock.Children.Insert(8, SalesDock);
                if (Schedule.SalesOrder.ViewModel.LoadAsyncComplete != null)
                {
                    while (!Schedule.SalesOrder.ViewModel.LoadAsyncComplete.IsCompleted) { }
                }

                SwitchView(App.SiteNumber, null, false);
                RefreshTimer.IsRefreshing = false;
                App.LoadedModule = Enumerations.UsersControls.Schedule;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Workspacedock\n{ex.Message}", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            var _tempDock = new DockPanel();
            switch(index)
            {
                case 1:
                case 2:
                    _tempDock = SchedDock;
                    index = refreshDock ? index : 1;
                    break;
                case 3:
                    _tempDock = CountDock;
                    break;
                case 5:
                    _tempDock = PurchaseDock;
                    break;
                case 8:
                    _tempDock = SalesDock;
                    break;
            }
            if (refreshDock)
            {
                App.SiteNumber = index;
                Schedule.ViewModel.ScheduleFilter($"[Site] = {App.SiteNumber}", 6);
                RefreshTimer.RefreshTimerTick();
                MainDock.Children.RemoveAt(4);
                MainDock.Children.Insert(4, new Admin.View { DataContext = new Admin.ViewModel() });
                MainDock.Children[4].Visibility = Visibility.Collapsed;
                index = 1;
                refreshDock = false;
            }
            else if (dataContext != null)
            {
                ((UserControl)MainDock.Children[index]).DataContext = dataContext;
            }
            MainDock.Children[index].Visibility = Visibility.Visible;
            if (index == 0)
            {
                ((PartInfo_View)MainDock.Children[index]).SearchTextBox.Focus();
            }
            App.LoadedModule = Enum.TryParse(index.ToString(), out Enumerations.UsersControls eUC) ? eUC : Enumerations.UsersControls.Schedule;
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Module)));
        }

        /// <summary>
        /// Refresh the views in the MainDock
        /// </summary>
        public static void RefreshMainDock()
        {
            if (CurrentUser.IsLoggedIn)
            {
                if (CurrentUser.HasSalesOrderModule)
                {
                    ((Schedule.SalesOrder.View)SalesDock.Children[0]).DataContext = new Schedule.SalesOrder.ViewModel();
                }
                if (CurrentUser.IsInventoryControl)
                {
                    ((CycleCount.Sched_View)CountDock.Children[0]).DataContext = new CycleCount.Sched_ViewModel();
                }
            }
            else
            {
                RefreshTimer.Clear();
                RefreshTimer.Add(((Schedule.ViewModel)((Schedule.View)SchedDock.Children[0]).DataContext).RefreshSchedule);
            }
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
                    ((UserControl)((DockPanel)MainDock.Children[parentUCIndex]).Children[childUCIndex]).DataContext = null;
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
