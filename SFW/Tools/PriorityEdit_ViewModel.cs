using M2kClient;
using SFW.Helpers;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Tools
{
    public class PriorityEdit_ViewModel : ViewModelBase
    {
        #region Properties

        public string OrderNumber { get; set; }

        private string _oldShift;
        private int? _shift;
        public string Shift
        {
            get { return _shift.ToString(); }
            set
            {
                if (value == "999")
                {
                    value = null;
                }
                _shift = int.TryParse(value, out int i) ? i : 0;
                if (_shift == 0)
                {
                    _shift = null;
                }
                OnPropertyChanged(nameof(Shift));
            }
        }

        private string _oldPri;
        private int? _pri;
        public string Priority
        {
            get { return _pri.ToString(); }
            set
            {
                if (value == "999")
                {
                    value = null;
                }
                _pri = int.TryParse(value, out int i) ? i : 0;
                if (_pri == 0)
                {
                    _pri = null;
                }
                OnPropertyChanged(nameof(Priority));
            }
        }

        private RelayCommand _changePri;

        #endregion

        /// <summary>
        /// Overloaded Constructor
        /// Allows the changing of already populated data
        /// </summary>
        /// <param name="orderNbr">Work Order number to edit the priority</param>
        /// <param name="shift">Current shift assignment</param>
        /// <param name="pri">Current priority assignment</param>
        public PriorityEdit_ViewModel(string orderNbr, int shift, int pri)
        {
            OrderNumber = orderNbr;
            Shift = _oldShift = shift.ToString();
            Priority = _oldPri = pri.ToString();
        }

        #region Priority Change ICommand

        public ICommand PriorityChangeICommand
        {
            get
            {
                if (_changePri == null)
                {
                    _changePri = new RelayCommand(PriorityChangeExecute, PriorityChangeCanExecute);
                }
                return _changePri;
            }
        }

        private void PriorityChangeExecute(object parameter)
        {
            var _changeRequest = string.Empty;
            if (_oldPri != Priority)
            {
                _changeRequest = M2kCommand.EditRecord("WP", OrderNumber, 89, Priority, UdArrayCommand.Replace, App.ErpCon);
            }
            if (string.IsNullOrEmpty(_changeRequest) && _oldShift != Shift)
            {
                _changeRequest = M2kCommand.EditRecord("WP", OrderNumber, 90, Shift, UdArrayCommand.Replace, App.ErpCon);
            }
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
            }
            else
            {
                var _row = Model.ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{OrderNumber}'");
                var _index = Model.ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row.FirstOrDefault());
                Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Shift", string.IsNullOrEmpty(Shift) ? 999 : int.Parse(Shift));
                Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Priority", Priority);
            }
            App.CloseWindow<PriorityEdit_View>();
        }
        private bool PriorityChangeCanExecute(object parameter)
        {
            return (_pri > 0 && _pri < 60) || (_shift > 0 && _shift <= 23 && _pri > 0 && _pri < 60);
        }

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _changePri = null;
            }
        }
    }
}
