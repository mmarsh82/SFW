using SFW.Helpers;
using SFW.Model;
using SFW.Reports;
using System;
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
                            new ProcessSpec_View { DataContext = new ProcessSpec_ViewModel(_wo) }.ShowDialog();
                            break;
                        case 1:
                            ExcelReader.ReadSetup(_wo.SkuNumber, Machine.GetMachineName(App.AppSqlCon, _wo));
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
