using SFW.Controls;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class PartTrace : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Part Information ICommand execution
        /// </summary>
        /// <param name="parameter">Sku object or Sku Number</param>
        public void Execute(object parameter)
        {
            try
            {
                WorkSpaceDock.SwitchView(8, null);
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
