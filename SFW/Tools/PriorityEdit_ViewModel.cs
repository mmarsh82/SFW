using M2kClient;
using SFW.Commands;
using System.Windows;
using System.Windows.Input;

namespace SFW.Tools
{
    public class PriorityEdit_ViewModel : ViewModelBase
    {
        #region Properties

        public string OrderNumber { get; set; }

        private int _shift;
        public int Shift
        {
            get { return _shift; }
            set
            {
                _shift = value;
                OnPropertyChanged(nameof(Shift));
            }
        }

        private int _pri;
        public int Priority
        {
            get { return _pri; }
            set
            {
                _pri = value;
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
            Shift = shift;
            Priority = pri;
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
            var _s = Shift.ToString().Length == 1 ? $"0{Shift}" : Shift.ToString();
            var _p = Priority.ToString().Length == 1 ? $"0{Priority}" : Priority.ToString();
            var _changeRequest = M2kCommand.EditRecord("WP", OrderNumber, 195, $"{_s}:{_p}:00", App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
            }
            var _wind = Application.Current.Windows;
            foreach (var w in _wind)
            {
                if (((Window)w).Name == "PriWindow")
                {
                    ((Window)w).Close();
                }
            }
        }
        private bool PriorityChangeCanExecute(object parameter) => Priority > 0;

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
