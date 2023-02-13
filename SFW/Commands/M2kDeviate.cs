using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kDeviate : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                var _wpNbr = ((DataRowView)parameter).Row.Field<string>("WorkOrder");
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = ".pdf";
                ofd.Filter = "Part Prints (.pdf)|*.pdf";
                var _result = ofd.ShowDialog();
                if (_result == true)
                {
                    var _newPath = $"\\\\fs-wcco\\WCCO-Prints\\Deviations\\{_wpNbr}-1.pdf";
                    File.Move(ofd.FileName, _newPath);
                }
                M2kClient.M2kCommand.EditRecord("WP", _wpNbr, 47, "Y", M2kClient.UdArrayCommand.Replace, App.ErpCon);
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
