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
                var _site = App.Site.Split('_')[0];
                if (parameter.ToString().Contains("*"))
                {
                    var _soDetail = parameter.ToString().Split('*');
                    Process.Start($"http://intranet-wcco-1/{_site}.MAIN/SOP/SoLineDetail/SoLineDetail.aspx?SoNbr={_soDetail[0]}&LineNbr={_soDetail[1]}");
                }
                else
                {
                    Process.Start($"http://intranet-wcco-1/{_site}.MAIN/SFC/IssuedMaterialDetail/IssuedMaterialDetail.aspx?WorkOrder={parameter}");
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
