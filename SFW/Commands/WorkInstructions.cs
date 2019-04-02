using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SFW.Commands
{
    public class WorkInstructions : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //TODO: move the file path to global config file
            Process.Start($"\\\\manage2\\server\\Document Center\\Production\\{parameter}");
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
