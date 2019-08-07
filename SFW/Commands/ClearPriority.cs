using System;
using System.Data;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ClearPriority : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            var _row = ((DataRowView)parameter).Row;
            var _wo = _row.ItemArray[0].ToString().Split('*');
            M2kClient.M2kCommand.RemoveRecord("WP", _wo[0], 195, App.ErpCon);
        }
        public bool CanExecute(object parameter) => true;
    }
}
