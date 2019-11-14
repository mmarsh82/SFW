using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class PartSearch : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Part Search ICommand execution
        /// </summary>
        /// <param name="parameter">SkuNumber or SkuNumber*MasterPrint</param>
        public void Execute(object parameter)
        {
            try
            {
                //CSI Part search
                if (App.SiteNumber == 0)
                {
                    if (parameter.ToString().Contains("*"))
                    {
                        parameter = parameter.ToString().Split('*')[0];
                    }
                    Process.Start($"\\\\csi-prime\\prints\\part\\{parameter}.pdf");
                }
                //WCCO Part search
                else
                {
                    if (parameter.ToString().Contains("*"))
                    {
                        var _temp = parameter.ToString().Split('*');
                        parameter = !string.IsNullOrEmpty(_temp[1]) && _temp[1] != DependencyProperty.UnsetValue.ToString() ? _temp[1] : _temp[0];
                    }
                    if (!string.IsNullOrEmpty(parameter?.ToString()))
                    {
                        ///TODO: Remove hard coded print location
                        Process.Start($"\\\\manage2\\Prints\\{parameter}.pdf");
                    }
                    else
                    {
                        MessageBox.Show("The part number that you have selected is either invalid or does not exist.", "Invalid or Missing Part Number", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                }
            }
            catch (Win32Exception)
            {
                MessageBox.Show("The Print for this part number was not found.\nPlease contact engineering for further support.", "Missing Print Document", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
