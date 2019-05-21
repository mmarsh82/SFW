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
            try
            {
                Process.Start($"\\\\manage2\\server\\Document Center\\Production\\{parameter}");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                System.Windows.MessageBox.Show("This work instruction is either under construction or not available.\nPlease contact IT for further assistance.", "File Not Found", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
