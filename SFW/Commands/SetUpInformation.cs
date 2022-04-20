using SFW.Model;
using SFW.Reports;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    class SetUpInformation : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Setup Information ICommand execution
        /// </summary>
        /// <param name="parameter">WorkOrder Object</param>
        public void Execute(object parameter)
        {
            try
            {
                if (!string.IsNullOrEmpty(parameter.ToString()) && !parameter.ToString().Contains("ERR:"))
                {
                    if (parameter.ToString().Contains("|"))
                    {
                        var _woNbr = parameter.ToString().Split('|').FirstOrDefault();
                        var _wo = new WorkOrder(_woNbr);
                        new ProcessSpec_View { DataContext = new ProcessSpec_ViewModel(_wo) }.ShowDialog();
                    }
                    else
                    {
                        Process.Start(parameter.ToString());
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Setup file is currently open.\nPlease contact ME for further assistance."
                        ,"Setup File Open"
                        ,System.Windows.MessageBoxButton.OK
                        ,System.Windows.MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Unable to access the setup up file.\nPlease contact IT for further assistance.\n\nDetails:\n{ex.Message}"
                    ,"Unhandled Exception"
                    ,System.Windows.MessageBoxButton.OK
                    ,System.Windows.MessageBoxImage.Exclamation);
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
