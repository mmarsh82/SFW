using System;
using System.Windows.Input;

//Created by Michael Marsh 9-25-18

namespace SFW.Commands
{
    public sealed class M2kRecordChange : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Manage 2000 Look queries ICommand execution
        /// </summary>
        /// <param name="parameter">Order number to use to query</param>
        public void Execute(object parameter)
        {
            try
            {
                if(parameter != null)
                {
                    switch(parameter.ToString())
                    {
                        case "Priority":
                            //TODO: Write in the M2k record change for mgt_priority will need to refresh view after
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
