using M2kClient;
using SFW.Controls;
using System;
using System.Data;
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
                    case "Back":
                        WorkSpaceDock.SwitchView(App.SiteNumber, null);
                        break;
                    case "Schedule":
                        if (!RefreshTimer.IsRefreshing)
                        {
                            RefreshTimer.RefreshTimerTick();
                        }
                        else
                        {
                            MessageBox.Show("The work load is currently refreshing.");
                        }
                        break;
                    case "ClosedSched":
                        WorkSpaceDock.SwitchView(6, new Schedule.Closed.ViewModel());
                        break;
                    case "Scheduler":
                        WorkSpaceDock.SwitchView(2, new Scheduler.ViewModel());
                        break;
                    case "CycleCount":
                        WorkSpaceDock.SwitchView(4, null);
                        break;
                    case "SalesSched":
                        WorkSpaceDock.SwitchView(9, null);
                        break;
                    case "Admin":
                        WorkSpaceDock.SwitchView(5, null);
                        break;
                    case "SiteCsi":
                        if (!App.DatabaseChange("CSI_MAIN"))
                        {
                            App.DatabaseChange(_temp);
                            return;
                        }
                        App.ErpCon.DatabaseChange(Database.CSI);
                        WorkSpaceDock.SwitchView(0, null);
                        break;
                    case "SiteWcco":
                        if (!App.DatabaseChange("WCCO_MAIN"))
                        {
                            App.DatabaseChange(_temp);
                            return;
                        }
                        App.ErpCon.DatabaseChange(Database.WCCO);
                        WorkSpaceDock.SwitchView(1, null);
                        break;
                    default:
                        WorkSpaceDock.SwitchView(App.SiteNumber, null);
                        var _tempDock = App.SiteNumber == 0 ? WorkSpaceDock.CsiDock : WorkSpaceDock.WccoDock;
                        if (int.TryParse(parameter.ToString(), out int i))
                        {
                            ((DataView)((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = $"MachineNumber = {parameter}";
                        }
                        else
                        {
                            ((DataView)((Schedule.ViewModel)((Schedule.View)_tempDock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = $"MachineGroup = '{parameter}'";
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
