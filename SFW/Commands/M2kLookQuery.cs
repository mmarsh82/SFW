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
                    var _parSplit = parameter.ToString().Split('*');
                    switch (_parSplit[0])
                    {
                        case "WO":
                            Process.Start($"http://intranet-wcco-1/CONTI.MAIN/SFC/IssuedMaterialDetail/IssuedMaterialDetail.aspx?WorkOrder={_parSplit[1]}");
                            break;
                        case "SO":
                            var _startInfo = _parSplit.Length == 2
                                ? $"http://intranet-wcco-1/CONTI.MAIN/SOP/SoLineDetail/SoLineDetail.aspx?SoNbr={_parSplit[1]}&LineNbr=1"
                                : $"http://intranet-wcco-1/CONTI.MAIN/SOP/SoLineDetail/SoLineDetail.aspx?SoNbr={_parSplit[1]}&LineNbr={_parSplit[2]}";
                            Process.Start(_startInfo);
                            break;
                        case "AR":
                            Process.Start($"http://intranet-wcco-1/CONTI.MAIN/ROIPortals/CustomerPortal/CustomerPortal.aspx?CustNbr={_parSplit[1]}");
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
