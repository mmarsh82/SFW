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
            p.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}SFW.exe";
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
