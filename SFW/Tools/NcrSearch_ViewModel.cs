using SFW.Commands;
using SFW.Controls;
using SFW.Helpers;
using System.Windows.Input;

namespace SFW.Tools
{
    public class NcrSearch_ViewModel : ViewModelBase
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
        public NcrSearch_ViewModel()
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
            if (int.TryParse(UserEntry, out int i))
            {
                if (Model.Ncr.IsValid(i))
                {
                    if (App.LoadedModule != Enumerations.UsersControls.Schedule)
                    {
                        new ViewLoad().Execute(1);
                    }
                    new LoadNcr().Execute(UserEntry);
                    App.CloseWindow<NcrSearch_View>();
                }
            }
        }
        private bool SubmitCanExecute(object parameter) => !string.IsNullOrEmpty(UserEntry);

        #endregion

    }
}
