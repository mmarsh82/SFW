using System;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class ViewLoad : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// View Load Execution
        /// </summary>
        /// <param name="parameter">View to Load</param>
        public void Execute(object parameter)
        {
            try
            {
                MainWindowViewModel.WorkSpaceDock.Children.Clear();
                var _temp = App.AppSqlCon.Database;
                switch (parameter.ToString())
                {
                    case "Schedule":
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View());
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                        break;
                    case "Scheduler":
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Scheduler.View { DataContext = new Scheduler.ViewModel() });
                        break;
                    case "SiteCsi":
                        if (!App.SqlCon_DataBaseChange("CSI_MAIN"))
                        {
                            App.SqlCon_DataBaseChange(_temp);
                        }
                        MainWindowViewModel.UpdateProperties();
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View());
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                        break;
                    case "SiteWcco":
                        if (!App.SqlCon_DataBaseChange("WCCO_MAIN"))
                        {
                            App.SqlCon_DataBaseChange(_temp);
                        }
                        MainWindowViewModel.UpdateProperties();
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View());
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                        break;
                    default:
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View { DataContext = new Schedule.ViewModel(parameter.ToString()) });
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
