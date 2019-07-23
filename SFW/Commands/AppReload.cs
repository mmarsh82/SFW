using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class AppReload : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var p = new Process();
            //TODO: remove hard code out to the global config file for the application global location
            p.StartInfo.FileName = $"\\\\manage2\\FSW\\ShopFloorWorkbench\\SFW.application";
            if (CurrentUser.IsLoggedIn)
            {
                p.StartInfo.Arguments = $"1_{CurrentUser.DomainUserName}";
            }
            p.Start();
            Application.Current.Shutdown();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
