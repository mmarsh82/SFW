using SFW.Commands;
using SFW.Helpers;
using System.Windows.Input;

namespace SFW.Tools
{
    public class DiamondEntry_ViewModel : ViewModelBase
    {
        #region Properties

        private string _entry;
        public string UserEntry
        {
            get { return _entry; }
            set
            {
                _entry = value;
                OnPropertyChanged(nameof(UserEntry));
            }
        }

        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DiamondEntry_ViewModel()
        { }

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
            DiamondEntry.DiamondNumber = UserEntry;
            App.CloseWindow<DiamondEntry_View>();
        }
        private bool SubmitCanExecute(object parameter) => !string.IsNullOrEmpty(UserEntry);

        #endregion

    }
}
