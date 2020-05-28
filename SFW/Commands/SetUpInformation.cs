using SFW.Helpers;
using SFW.Model;
using SFW.Reports;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    class SetUpInformation : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Setup Information ICommand execution
        /// </summary>
        /// <param name="parameter">WorkOrder Object</param>
        public void Execute(object parameter)
        {
            try
            {
                if (parameter != null && parameter.GetType() == typeof(WorkOrder))
                {
                    var _wo = (WorkOrder)parameter;
                    switch (App.SiteNumber)
                    {
                        case 0:
                            try
                            {
                                Process.Start($"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PressSetup}{_wo.SkuNumber}.pdf");
                            }
                            catch (Exception)
                            {
                                new ProcessSpec_View { DataContext = new ProcessSpec_ViewModel(_wo) }.ShowDialog();
                            }
                            break;
                        case 1:
                            var _machGroup = Machine.GetMachineGroup(App.AppSqlCon, _wo);
                            var _fileName = string.Empty;
                            var _filePath = string.Empty;
                            switch (_machGroup)
                            {
                                case "PRESS":
                                case "ENG":
                                    _fileName = ExcelReader.GetSetupPrintNumber(_wo.SkuNumber, Machine.GetMachineName(App.AppSqlCon, _wo), App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PressSetup, "Production");
                                    if (!string.IsNullOrEmpty(_fileName) && !_fileName.Contains("ERR:"))
                                    {
                                        var _fileheader = string.Empty;
                                        for (int i = 0; i < 8 - _fileName.Length; i++)
                                        {
                                            _fileheader += "0";
                                        }
                                        _fileName = _fileheader + _fileName;
                                        _filePath = $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{_fileName}.PDF";
                                    }
                                    else if (_fileName.Contains("ERR:"))
                                    {
                                        System.Windows.MessageBox.Show("The origin file is currently open by an administrator,\nplease contact ME for further assistance.", "File Lock", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                                    }
                                    else
                                    {
                                        System.Windows.MessageBox.Show("No setup file exists.", "No File Exists", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                                    }
                                    break;
                                case "FABE":
                                    _fileName = ExcelReader.GetSetupPrintNumber(_wo.SkuNumber, Machine.GetMachineName(App.AppSqlCon, _wo), App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).SyscoSetup, "PRODUCTION");
                                    _filePath = $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{_fileName}.PDF";
                                    break;
                            }             
                            if (!string.IsNullOrEmpty(_filePath))
                            {
                                Process.Start(_filePath);
                            }
                            break;
                    }
                }
                else
                {
                    //TODO: add in the interface to handle a null parameter, will require a new usercontrol similiar to part search
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
