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
            }
        }

        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DownReason_ViewModel()
        {
            DownReason = Machine.GetDownReasonDictionary(App.AppSqlCon);
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
            
            App.CloseWindow<DownReason_View>();
        }
        private bool SubmitCanExecute(object parameter) => !string.IsNullOrEmpty(SelectedReason.Value);

        #endregion

    }
}
