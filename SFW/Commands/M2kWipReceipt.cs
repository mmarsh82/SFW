using M2kClient;
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
            //TODO: add in the new WipReceipt object to be populated and passed into the M2kWipCommand
        }

        public bool CanExecute(object parameter) => true;
    }
}
