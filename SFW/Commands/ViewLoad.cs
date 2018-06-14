using SFW.Controls;
using System;
using System.Windows;
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
                var _temp = App.AppSqlCon.Database;
                switch (parameter.ToString())
                {
                    case "Schedule":
                        WorkSpaceDock.SwitchView(0, null);
                        break;
                    case "Scheduler":
                        WorkSpaceDock.SwitchView(2, new Scheduler.ViewModel());
                        break;
                    case "SiteCsi":
                        if (!App.SqlCon_DataBaseChange("CSI_MAIN"))
                        {
                            App.SqlCon_DataBaseChange(_temp);
                        }
                        MainWindowViewModel.UpdateProperties();
                        WorkSpaceDock.RefreshMainDock();
                        break;
                    case "SiteWcco":
                        if (!App.SqlCon_DataBaseChange("WCCO_MAIN"))
                        {
                            App.SqlCon_DataBaseChange(_temp);
                        }
                        MainWindowViewModel.UpdateProperties();
                        WorkSpaceDock.RefreshMainDock();
                        break;
                    default:
                        WorkSpaceDock.SwitchView(0, new Schedule.ViewModel(parameter.ToString()));
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
