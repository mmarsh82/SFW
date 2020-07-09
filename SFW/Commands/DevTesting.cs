using SFW.Controls;
using SFW.Model;
using System;
using System.Data;
using System.Windows.Input;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            var _dock = App.SiteNumber == 0
                            ? WorkSpaceDock.CsiDock
                            : WorkSpaceDock.WccoDock;
            _dock.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView != null)
                {
                    if (((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).Table.Select($"WO_Priority = 'A'").Length == 0)
                    {
                        ((ShopRoute.ViewModel)((ShopRoute.View)_dock.Children[1]).DataContext).ShopOrder = new WorkOrder();
                    }
                    ((DataView)((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.SourceCollection).RowFilter = "WO_Priority = 'A'";
                    ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).SearchFilter = null;
                    ((Schedule.ViewModel)((Schedule.View)_dock.Children[0]).DataContext).ScheduleView.Refresh();
                }
            }));
        }
        public bool CanExecute(object parameter) => true;
    }
}
