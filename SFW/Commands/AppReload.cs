using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class AppReload : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (CurrentUser.IsLoggedIn)
            {
                File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SSO.txt", CurrentUser.DomainUserName);
            }
            //TODO: remove hard code out to the global config file for the application global location
            Process.Start($"\\\\manage2\\FSW\\ShopFloorWorkbench\\SFW.application");
            Application.Current.Shutdown();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
