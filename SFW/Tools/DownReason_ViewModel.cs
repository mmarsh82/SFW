using SFW.Helpers;
using SFW.Model.Production;
using System.Collections.Generic;
using System.Windows.Input;

namespace SFW.Tools
{
    public class DownReason_ViewModel : ViewModelBase
    {
        #region Properties

        private string _userNote;
        public string UserNote
        {
            get { return _userNote; }
            set
            {
                _userNote = value;
                OnPropertyChanged(nameof(UserNote));
            }
        }

        public IReadOnlyDictionary<int, string> DownReason { get; set; }

        private KeyValuePair<int, string> _selReason;
        public KeyValuePair<int, string> SelectedReason
        {
            get
            { return _selReason; }
            set
            {
                _selReason = value;
                OnPropertyChanged(nameof(SelectedReason));
                OnPropertyChanged(nameof(IsDefect));
            }
        }

        public bool IsDefect { get { return SelectedReason.Key == 7; } }


        private string _ref;
        public string Reference
        {
            get { return _ref; }
            set
            {
                _ref = value;
                OnPropertyChanged(nameof(Reference));
            }
        }

        public string MachineID;
        public string OrderID;

        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DownReason_ViewModel(string workCenter, string orderId)
        {
            DownReason = Machine.GetDownReasonDictionary(App.AppSqlCon);
            MachineID = workCenter;
            OrderID = orderId;
        }

        #region Submit ICommand

        public ICommand SubmitICommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SubmitExecute, SubmitCanExecute);
                }
                return _submit;
            }
        }

        private void SubmitExecute(object parameter)
        {
            var _reference = 0;
            if (!string.IsNullOrEmpty(Reference))
            {
                _reference = int.TryParse(Reference, out int i) ? i : 0;
            }
            Machine.SubmitDownReason(MachineID, SelectedReason.Key, UserNote, CurrentUser.ErpId, OrderID, _reference, App.AppSqlCon);
            App.CloseWindow<DownReason_View>();
        }
        private bool SubmitCanExecute(object parameter)
        {
            if (!string.IsNullOrEmpty(SelectedReason.Value))
            {
                if (SelectedReason.Key == 7)
                {
                    if (!string.IsNullOrEmpty(Reference) && int.TryParse(Reference, out int i))
                    {
                        return Model.Quality.QmsForm.IsValid(i);
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        #endregion

    }
}
