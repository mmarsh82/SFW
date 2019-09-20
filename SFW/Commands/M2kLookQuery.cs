using System;
using System.Diagnostics;
using System.Windows.Input;

//Created by Michael Marsh 5-1-18

namespace SFW.Commands
{
    public class M2kLookQuery : ICommand
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
                if (parameter.ToString().Contains("*"))
                {
                    var _soDetail = parameter.ToString().Split('*');
                    var _loc = App.Site.Split('_')[0];
                    Process.Start($"http://m2k/WCCO.MAIN/SOP/SoLineDetail/SoLineDetail.aspx?SoNbr={_soDetail[0]}&LineNbr={_soDetail[1]}");
                }
                else
                {
                    //TODO write in the call for the wo.status call
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
