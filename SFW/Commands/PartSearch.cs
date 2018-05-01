using SFW.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SFW.Commands
{
    public class PartSearch : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Part Search ICommand execution
        /// </summary>
        /// <param name="parameter">Skew object</param>
        public void Execute(object parameter)
        {
            try
            {
                var _skew = parameter as Sku;
                if (string.IsNullOrEmpty(_skew.MasterPrint))
                {
                    ///TODO: Remove hard coded print location
                    Process.Start($"\\\\manage2\\server\\Engineering\\Product\\Prints\\Controlled Production Prints\\{_skew.Number}.pdf");
                }
                else
                {
                    Process.Start($"\\\\manage2\\server\\Engineering\\Product\\Prints\\Controlled Production Prints\\{_skew.MasterPrint}.pdf");
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
