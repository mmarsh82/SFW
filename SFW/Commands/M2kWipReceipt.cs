using M2kClient;
using SFW.Model;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kWipReceipt : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            M2kCommand.ProductionWip((WorkOrder)parameter, App.ErpCon);
        }

        public bool CanExecute(object parameter) => true;
    }
}
