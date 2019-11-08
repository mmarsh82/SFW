using SFW.Helpers;
using System.Windows.Controls;
using System.Windows.Input;

namespace SFW.PassReset
{
    class ViewModel : ViewModelBase
    {
        #region Properties

        public string UserName { get; set; }
        public string NewPwd { get; set; }
        public string ConfirmPwd { get; set; }
        public string OldPwd { get; set; }

        private string _error;
        public string ResetError
        {
            get
            { return _error; }
            set
            {
                _error = value;
                OnPropertyChanged(nameof(ResetError));
            }
        }

        public RelayCommand _passResetCommand;

        #endregion

        public ViewModel()
        {
            UserName = string.Empty;
        }
        public ViewModel(string userName, string oldPwd)
        {
            UserName = userName;
            OldPwd = oldPwd;
        }

        public ICommand PassResetCommand
        {
            get
            {
                if (_passResetCommand == null)
                {
                    _passResetCommand = new RelayCommand(PassResetExecute, PassResetCanExecute);
                }
                return _passResetCommand;
            }
        }

        /// <summary>
        /// Password Reset ICommand Execution
        /// </summary>
        /// <param name="parameter">Will contain a secure password object</param>
        public void PassResetExecute(object parameter)
        {
            if (parameter != null && parameter.GetType() == typeof(PasswordBox[]))
            {
                ResetError = CurrentUser.UpdatePassword(UserName, ((PasswordBox[])parameter)[0].Password, ((PasswordBox[])parameter)[1].Password);
                if (string.IsNullOrEmpty(ResetError))
                {


                    foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                    {
                        if (w.Title == "Password Reset")
                        {
                            w.Close();
                            if ((ShopRoute.ViewModel)Controls.WorkSpaceDock.WccoDock.GetChildOfType<ShopRoute.View>().DataContext != null)
                            {
                                ((ShopRoute.ViewModel)Controls.WorkSpaceDock.WccoDock.GetChildOfType<ShopRoute.View>().DataContext).UpdateView();
                            }
                        }
                    }
                }
            }
         }

        public bool PassResetCanExecute(object parameter)
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


        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                NewPwd = OldPwd = ConfirmPwd = null;
                _passResetCommand = null;
            }
        }

    }
}
