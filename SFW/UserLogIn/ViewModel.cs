using SFW.Helpers;
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
            if (ViewType)
            {
                if (parameter != null && parameter.GetType() == typeof(PasswordBox[]))
                {
                    Error = CurrentUser.UpdatePassword(UserName, ((PasswordBox[])parameter)[0].Password, ((PasswordBox[])parameter)[1].Password);
                    if (string.IsNullOrEmpty(Error))
                    {
                        if (ForceReset)
                        {
                            var _result = CurrentUser.LogIn(UserName, ((PasswordBox[])parameter)[0].Password);
                        }
                        foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                        {
                            if (w.Title == "Password Reset")
                            {
                                w.Close();
                            }
                        }
                    }
                }
            }
            else
            {
                var _result = CurrentUser.LogIn(UserName, ((PasswordBox[])parameter)[0].Password);
                if (!_result.ContainsKey(0) && _result.TryGetValue(1, out string s))
                {
                    Error = s;
                    //TODO: add in the viewmodel change representation
                    ViewType = true;
                    ForceReset = true;
                    //CurrentUser.UpdatePassword(UserName, ((PasswordBox)parameter).Password, NewPwd);
                }
                else if (!_result.ContainsKey(0))
                {
                    foreach (var v in _result)
                    {
                        Error = v.Value;
                    }
                    ((PasswordBox[])parameter)[0].Password = null;
                }
                else
                {
                    if (CurrentUser.IsLoggedIn)
                    {
                        foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                        {
                            if (w.Title == "User Log In")
                            {
                                w.Close();
                            }
                        }
                    }
                }
            }
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
                
            }
        }
    }
}
