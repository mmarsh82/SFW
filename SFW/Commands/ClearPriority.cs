using SFW.Model.Production;
using System;
using System.Data;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ClearPriority : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add {  }
            remove { }
        }
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
                var _changeRequest = M2kClient.M2kCommand.EditRecord("WP", _wo, 89, "",M2kClient.UdArrayCommand.Replace, App.ErpCon);
                if (string.IsNullOrEmpty(_changeRequest))
                {
                    _changeRequest = M2kClient.M2kCommand.EditRecord("WP", _wo, 90, "", M2kClient.UdArrayCommand.Replace, App.ErpCon);
                }
                if (!string.IsNullOrEmpty(_changeRequest))
                {
                    System.Windows.MessageBox.Show(_changeRequest, "ERP Record Error");
                }
                else
                {
                    if (_row == null)
                    {
                        _row = Model.ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{parameter}'").FirstOrDefault();
                    }
                    var _index = Model.ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name].Rows.IndexOf(_row);
                    Model.ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name].Rows[_index].SetField("Sched_Shift", "999");
                    Model.ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name].Rows[_index].SetField("Sched_Priority", "999");
                }
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
