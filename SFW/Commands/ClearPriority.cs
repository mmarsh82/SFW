using System;
using System.Data;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ClearPriority : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                var _wo = string.Empty;
                DataRow _row = null;
                if (parameter.GetType() == typeof(DataRowView))
                {
                    _row = ((DataRowView)parameter).Row;
                    _wo = _row.Field<string>("WorkOrder");
                }
                else
                {
                    _wo = parameter.ToString();
                }
                var _changeRequest = M2kClient.M2kCommand.EditRecord("WP", _wo, 195, "", M2kClient.UdArrayCommand.Replace, App.ErpCon);
                if (!string.IsNullOrEmpty(_changeRequest))
                {
                    System.Windows.MessageBox.Show(_changeRequest, "ERP Record Error");
                }
                else
                {
                    if (_row == null)
                    {
                        _row = Model.ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{parameter}'").FirstOrDefault();
                    }
                    var _index = Model.ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row);
                    Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("PriTime", "999");
                    Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Priority", "999");
                }
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
