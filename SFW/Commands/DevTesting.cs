using System;
using System.Windows.Input;

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
            ((MainWindowViewModel)App.Current.MainWindow.DataContext).CheckForUpdate();
        }
        public bool CanExecute(object parameter) => true;
    }
}
