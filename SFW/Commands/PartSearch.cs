using SFW.Converters;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
            var _isdeviated = false;
            try
            {
                if (parameter.ToString().Contains("*"))
                {
                    var _temp = parameter.ToString().Split('*');
                    if (_temp.Length > 2 && _temp[2] == "Y")
                    {
                        parameter = $"\\\\waxfs001\\WAXG-Wahpeton\\Prints\\Deviations\\{_temp[3]}-1.pdf";
                        _isdeviated = true;
                    }
                    else
                    {
                        parameter = !string.IsNullOrEmpty(_temp[1]) && _temp[1] != DependencyProperty.UnsetValue.ToString() ? _temp[1] : _temp[0];
                        if (!File.Exists($"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{parameter}.pdf") && parameter.ToString() == _temp[1])
                        {
                            parameter = _temp[0];
                        }
                    }
                }
                if (parameter.ToString().Contains("|"))
                {
                    var _result = parameter.ToString().Split('|');
                    var _part = _result[0];
                    var _fac = int.TryParse(_result[1], out int i) ? i == 1 ? "WCCO" : "CSI" : "WCCO";
                    if (!File.Exists($"{App.GlobalConfig.First(o => o.Site == _fac).PartPrint}{_part}.pdf"))
                    {
                        _fac = i == 1 ? "CSI" : "WCCO";
                    }
                    Process.Start($"{App.GlobalConfig.First(o => o.Site == _fac.ToString()).PartPrint}{_part}.pdf");
                }
                else if (!string.IsNullOrEmpty(parameter?.ToString()) && !_isdeviated)
                {
                    Process.Start($"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{parameter}.pdf");
                }
                else if (!string.IsNullOrEmpty(parameter?.ToString()) && _isdeviated)
                {
                    Process.Start($"{parameter}");
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
                    case 5:
                        MessageBox.Show("You do not have access to this folder\nPlease contact IT for further assistance.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The directory or file that you are searching for does not exist.\nPlease contact IT for further assistance.\n\n{ex.Message}", "Incorrect Directory Path", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
