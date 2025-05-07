using SFW.Helpers;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SFW.UserLogIn
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public string UserName { get; set; }
        public string OldPwdText { get { return CurrentUser.IsLocked ? "Old Password:" : "Password:"; } }
        public string NewPwd { get; set; }
        public string ConfirmPwd { get; set; }
        public string OldPwd { get; set; }

        private bool _forceReset;
        public bool ForceReset
        {
            get
            {
                return _forceReset;
            }
            set
            {
                _forceReset = value;
                OnPropertyChanged(nameof(ForceReset));
            }
        }


        private bool _viewType;
        public bool ViewType
        {
            get
            {
                return _viewType;
            }
            set
            {
                _viewType = value;
                OnPropertyChanged(nameof(ViewType));
            }
        }

        private string _error;
        public string Error
        {
            get
            { return _error; }
            set
            {
                _error = value;

                OnPropertyChanged(nameof(Error));
            }
        }

        private bool _working;
        public bool LogInThreadIsWorking
        {
            get
            { return _working; }
            set
            {
                _working = value;
                OnPropertyChanged(nameof(LogInThreadIsWorking));
            }
        }
        private object _bgParam;

        public CurrentUser.ValidUser User;

        public BackgroundWorker LogInThread;

        public RelayCommand _loginCommand;

        public RelayCommand _passResetCommand;

        #endregion

        /// <summary>
        /// Log In ViewModel Default Constructor
        /// </summary>
        public ViewModel()
        {
            UserName = string.Empty;
            ViewType = false;
            ForceReset = false;
            User = new CurrentUser.ValidUser();
            LogInThread = new BackgroundWorker();
            LogInThread.DoWork += LogInThread_DoWork;
            LogInThread.RunWorkerCompleted += LogInThread_RunWorkerCompleted;
            LogInThreadIsWorking = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isDiff"></param>
        public ViewModel(bool vType)
        {
            if(CurrentUser.IsLoggedIn)
            {
                UserName = CurrentUser.DomainUserName;
            }
            ViewType = vType;
            ForceReset = false;
            LogInThread = new BackgroundWorker();
            LogInThread.DoWork += LogInThread_DoWork;
            LogInThread.RunWorkerCompleted += LogInThread_RunWorkerCompleted;
            LogInThreadIsWorking = false;
        }

        private void LogInThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_bgParam != null)
            {
                ((PasswordBox[])_bgParam)[0].Password = null;
                _bgParam = null;
            }
            if (User.Validated)
            {
                new CurrentUser(User);
                Application.Current.Windows.OfType<Window>().FirstOrDefault(o => o.Name == "LogIn_Window").Close();
            }
            LogInThreadIsWorking = false;
        }

        /// <summary>
        /// The thread for background log in
        /// </summary>
        /// <param name="sender">Calling method</param>
        /// <param name="e">Log in command parameter</param>
        private void LogInThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ViewType)
            {
                if (e.Argument != null && e.Argument.GetType() == typeof(PasswordBox[]))
                {
                    Error = CurrentUser.UpdatePassword(UserName, ((PasswordBox[])e.Argument)[0].Password, ((PasswordBox[])e.Argument)[1].Password);
                    if (string.IsNullOrEmpty(Error))
                    {
                        if (ForceReset)
                        {
                            var _result = CurrentUser.LogIn(UserName, ((PasswordBox[])e.Argument)[0].Password);
                        }
                    }
                }
            }
            else
            {
                User = CurrentUser.ValidateCredentials(UserName, ((PasswordBox[])e.Argument)[0].Password);
                if (User.Validated)
                {
                    Error = string.Empty;
                }
                else
                {
                    if (User.ErrorKey == 1)
                    {
                        Error = User.ErrorMessage;
                        ViewType = true;
                        ForceReset = true;
                    }
                    else
                    {
                        Error = User.ErrorMessage;
                    }
                }
            }
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
            LogInThreadIsWorking = true;
            Error = string.Empty;
            _bgParam = parameter;
            LogInThread.RunWorkerAsync(parameter);
        }

        public bool LogInCanExecute(object parameter)
        {
            if (ViewType)
            {
                if (parameter != null && parameter.GetType() == typeof(PasswordBox[]))
                {

                    return !string.IsNullOrEmpty(((PasswordBox[])parameter)[0].Password)
                        && !string.IsNullOrEmpty(((PasswordBox[])parameter)[1].Password)
                        && !string.IsNullOrEmpty(((PasswordBox[])parameter)[2].Password)
                        && ((PasswordBox[])parameter)[1].Password == ((PasswordBox[])parameter)[2].Password
                        && ((PasswordBox[])parameter)[0].Password != ((PasswordBox[])parameter)[1].Password
                        && !string.IsNullOrEmpty(UserName)
                        && CurrentUser.UserExist(UserName);
                }
                return false;
            }
            else
            {
                return !string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(((PasswordBox[])parameter)[0].Password);
            }
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
                LogInThread.Dispose();
            }
        }
    }
}
