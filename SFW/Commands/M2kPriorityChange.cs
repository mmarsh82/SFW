using System;
using System.Windows.Input;

//Created by Michael Marsh 9-25-18

namespace SFW.Commands
{
    public sealed class M2kPriorityChange : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Manage 2000 Priority Change ICommand execution
        /// </summary>
        /// <param name="parameter">Order number to use to query</param>
        public void Execute(object parameter)
        {
            try
            {
                if(parameter != null)
                {
                   
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
