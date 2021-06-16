using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kValueEdit : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {

        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
