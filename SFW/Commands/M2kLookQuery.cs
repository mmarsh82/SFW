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
                var _site = App.Site.Replace('_', '.');
                if (parameter.ToString().Contains("*"))
                {
                    var _parSplit = parameter.ToString().Split('*');
                    var test = $"http://waxas003/{_site}/SHR/ItemActivity/ManufacturingDetail.aspx?PartNbr={_parSplit[1]}%7c0{App.SiteNumber}&tabID=970533";
                    switch (_parSplit[0])
                    {
                        case "WO":
                            Process.Start($"http://waxas003/{_site}/SFC/IssuedMaterialDetail/IssuedMaterialDetail.aspx?WorkOrder={_parSplit[1]}");
                            break;
                        case "SO":
                            var _startInfo = _parSplit.Length == 2
                                ? $"http://waxas003/{_site}/SOP/SoLineDetail/SoLineDetail.aspx?SoNbr={_parSplit[1]}&LineNbr=1"
                                : $"http://waxas003/{_site}/SOP/SoLineDetail/SoLineDetail.aspx?SoNbr={_parSplit[1]}&LineNbr={_parSplit[2]}";
                            Process.Start(_startInfo);
                            break;
                        case "AR":
                            Process.Start($"http://waxas003/{_site}/ROIPortals/CustomerPortal/CustomerPortal.aspx?CustNbr={_parSplit[1]}");
                            break;
                        case "ACT":
                            Process.Start($"http://waxas003/{_site}/SHR/ItemActivity/ManufacturingDetail.aspx?PartNbr={_parSplit[1]}|0{App.SiteNumber}");
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
