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
            var _changeRequest = M2kClient.M2kCommand.EditRecord("WP", _wo[0], 195, "", M2kClient.UdArrayCommand.Replace, App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                System.Windows.MessageBox.Show(_changeRequest, "ERP Record Error");
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
