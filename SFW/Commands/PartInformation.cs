using SFW.Controls;
using SFW.Queries;
using System;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class PartInformation : ICommand
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
                    if(parameter.GetType() == typeof(Model.WorkOrder))
                    {
                        WorkSpaceDock.SwitchView(5, new PartInfo_ViewModel((Model.WorkOrder)parameter));
                    }
                    else
                    {
                        WorkSpaceDock.SwitchView(5, new PartInfo_ViewModel(parameter.ToString()));
                    }
                }
                else
                {
                    WorkSpaceDock.SwitchView(5, null);
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
