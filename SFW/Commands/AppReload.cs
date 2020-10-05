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
            Process.Start($"{App.AppFilePath}SFW.application");
            Application.Current.Shutdown();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
