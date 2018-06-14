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
            //Create the Control
            MainDock = ((MainWindow)Application.Current.Windows[0]).WorkSpaceDock;

            //Add the Schedule View to [0]
            MainDock.Children.Add(new Schedule.View());
            MainDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });

            //Add the Scheduler View to [2]
            MainDock.Children.Add(new Scheduler.View { DataContext = new Scheduler.ViewModel() });
            MainDock.Children[2].Visibility = Visibility.Collapsed;

            //Add the Part Info View to [3]
            MainDock.Children.Add(new PartInfo_View());
            MainDock.Children[3].Visibility = Visibility.Collapsed;
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
            if (index == 0)
            {
                MainDock.Children[index + 1].Visibility = Visibility.Visible;
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
