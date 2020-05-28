using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    public class WorkInstructions : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                if (parameter != null)
                {
                    if (parameter.ToString().IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        parameter += ".pdf";
                    }
                    Process.Start($"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI}{parameter}");
                }
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
