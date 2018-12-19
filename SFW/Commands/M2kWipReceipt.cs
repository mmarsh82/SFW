using SFW.Model;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kWipReceipt : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var _wipWindow = new WIP.View { DataContext = new WIP.ViewModel((WorkOrder)parameter) };
            _wipWindow.ShowDialog();
        }
        public bool CanExecute(object parameter) => CurrentUser.IsLoggedIn && CurrentUser.DomainName.Contains("wcco");
    }
}
