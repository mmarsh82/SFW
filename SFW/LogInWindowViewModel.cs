using SFW.Commands;
using System.Windows.Controls;
using System.Windows.Input;

namespace SFW
{
    public class LogInWindowViewModel : ViewModelBase
    {
        #region Properties

        public string UserName { get; set; }
        public string NewPwd { get; set; }

        private string _error;
        public string LogInError
        {
            get
            { return _error; }
            set
            {
                _error = value;
                OnPropertyChanged(nameof(LogInError));
            }
        }

        public RelayCommand _loginCommand;

        #endregion

        /// <summary>
        /// Log In ViewModel Default Constructor
        /// </summary>
        public LogInWindowViewModel()
        {
            UserName = string.Empty;
        }

        #region Log In ICommand

        public ICommand LogInCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(LogInExecute, LogInCanExecute);
                }
                return _loginCommand;
            }
        }

        /// <summary>
        /// Log In ICommand Execution
        /// </summary>
        /// <param name="parameter">Will contain a secure password object</param>
        public void LogInExecute(object parameter)
        {
            LogInError = CurrentUser.LogIn(UserName, ((PasswordBox)parameter).Password);
            if (LogInError == "Expired Password.")
            {
                CurrentUser.UpdatePassword(UserName, ((PasswordBox)parameter).Password, NewPwd);
            }
            if (CurrentUser.IsLoggedIn)
            {
                foreach(System.Windows.Window w in System.Windows.Application.Current.Windows)
                {
                    if (w.Title == "User Log In")
                    {
                        w.Close();
                    }
                }
            }
        }

        public bool LogInCanExecute(object parameter) => !string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(((PasswordBox)parameter).Password);

        #endregion
    }
}
