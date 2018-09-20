using SFW.Controls;
using System;
using System.Data;
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
                    case "Back":
                        WorkSpaceDock.SwitchView(App.SiteNumber, null);
                        break;
                    case "Schedule":
                        RefreshTimer.RefreshTimerTick();
                        break;
                    case "Scheduler":
                        WorkSpaceDock.SwitchView(4, new Scheduler.ViewModel());
                        break;
                    case "SiteCsi":
                        if (!App.SqlCon_DataBaseChange("CSI_MAIN"))
                        {
                            App.SqlCon_DataBaseChange(_temp);
                        }
                        WorkSpaceDock.SwitchView(0, null);
                        MainWindowViewModel.CurrentSite = "";
                        break;
                    case "SiteWcco":
                        if (!App.SqlCon_DataBaseChange("WCCO_MAIN"))
                        {
                            App.SqlCon_DataBaseChange(_temp);
                        }
                        WorkSpaceDock.SwitchView(2, null);
                        MainWindowViewModel.CurrentSite = "";
                        break;
                    default:
                        WorkSpaceDock.SwitchView(App.SiteNumber, null);
                        if (int.TryParse(parameter.ToString(), out int i))
                        {
                            ((DataView)((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.MainDock.Children[App.SiteNumber]).DataContext).ScheduleView.SourceCollection).RowFilter = $"MachineNumber = {parameter.ToString()}";
                        }
                        else
                        {
                            ((DataView)((Schedule.ViewModel)((Schedule.View)WorkSpaceDock.MainDock.Children[App.SiteNumber]).DataContext).ScheduleView.SourceCollection).RowFilter = $"MachineGroup = '{parameter.ToString()}'";
                        }
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
