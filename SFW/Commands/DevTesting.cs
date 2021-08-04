using System;
using System.Windows.Input;
using System.Linq;

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
            var test = Model.Machine.ScheduleDataSet(UserConfig.GetIROD(), CurrentUser.Site, App.SiteNumber, App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI, App.AppSqlCon);
        }
        public bool CanExecute(object parameter) => true;
    }
}
