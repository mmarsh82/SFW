using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class SafeShutdown : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Safe application shutdown ICommand execution
        /// </summary>
        /// <param name="parameter">Empty object</param>
        public void Execute(object parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }
        public bool CanExecute(object parameter) => true;
    }
}
