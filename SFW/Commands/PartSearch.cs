using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
                    var _temp = parameter.ToString().Split('*');
                    parameter = !string.IsNullOrEmpty(_temp[1]) && _temp[1] != DependencyProperty.UnsetValue.ToString() ? _temp[1] : _temp[0];
                    if (!System.IO.File.Exists($"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{parameter}.pdf") && parameter.ToString() == _temp[1])
                    {
                        parameter = _temp[0];
                    }
                }
                if (!string.IsNullOrEmpty(parameter?.ToString()))
                {
                    Process.Start($"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{parameter}.pdf");
                }
                else
                {
                    MessageBox.Show("The part number that you have selected is either invalid or does not exist.", "Invalid or Missing Part Number", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
            catch (Win32Exception wEx)
            {
                switch (wEx.NativeErrorCode)
                {
                    case 53:
                        MessageBox.Show("The share drive is currently not available.\nPlease contact IT for further assistance.", "Unable to Reach Share Drive", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                    case 2:
                        MessageBox.Show("The Print for this part number was not found.\nPlease contact engineering for further support.", "Missing Print Document", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
