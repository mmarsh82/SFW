using SFW.Model;
using SFW.Reports;
using System;
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
                if (parameter != null)
                {
                   switch (App.SiteNumber)
                    {
                        case 0:
                            new ProcessSpec_View { DataContext = new ProcessSpec_ViewModel((WorkOrder)parameter) }.ShowDialog();
                            break;
                        case 2:
                            //TODO: Add in the WCCO setup information window
                            break;
                    }
                }
                else
                {
                    //TODO: add in the interface to handle the 
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
