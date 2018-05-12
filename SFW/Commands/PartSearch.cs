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
                if (parameter.ToString().Contains("*"))
                {
                    parameter = parameter.ToString().Substring(parameter.ToString().IndexOf('*') + 1);
                }
                if (!string.IsNullOrEmpty(parameter?.ToString()))
                {
                    ///TODO: Remove hard coded print location
                    Process.Start($"\\\\manage2\\server\\Engineering\\Product\\Prints\\Controlled Production Prints\\{parameter}.pdf");
                }
                else
                {
                    MessageBox.Show("The part number that you have selected is either invalid or does not exist.", "Invalid or Missing Part Number", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
            catch (Win32Exception)
            {
                throw new Win32Exception(404, "The Print for this part number was not found.\nPlease contact engineering for further support.");
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
