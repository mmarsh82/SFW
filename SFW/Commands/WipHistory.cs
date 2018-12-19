using SFW.Controls;
using SFW.Queries;
using System;
using System.Windows.Input;

//Created by Michael Marsh 12-13-18

namespace SFW.Commands
{
    public class WipHistory : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Part Information ICommand execution
        /// </summary>
        /// <param name="parameter">Skew object or Skew Number</param>
        public void Execute(object parameter)
        {
            try
            {
                if (parameter != null)
                {
                    if (parameter.GetType() == typeof(Model.WorkOrder))
                    {
                        WorkSpaceDock.SwitchView(6, new WipHist_ViewModel());
                    }
                    else
                    {
                        WorkSpaceDock.SwitchView(6, new WipHist_ViewModel());
                    }
                }
                else
                {
                    WorkSpaceDock.SwitchView(6, null);
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
