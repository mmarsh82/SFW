using System;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            try
            {
                var _user = string.Empty;
                if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SSO.txt"))
                {
                    _user = File.ReadAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SSO.txt");
                    File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\SSO.txt");
                }
                else
                {
                    _user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    MessageBox.Show(_user, "Window User", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                _user = _user.Length <= 20 ? _user : _user.Substring(0, 20);
                using (PrincipalContext pContext = CurrentUser.GetPrincipal(_user))
                {
                    MessageBox.Show("Was able to get the context");
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, _user))
                    {
                        MessageBox.Show("Was able to find the principal");
                        if (uPrincipal.GetAuthorizationGroups().ToList().ConvertAll(o => o.Name).Exists(o => o.Contains("SFW_")))
                        {
                            MessageBox.Show("Was able to access the user groups");
                            new CurrentUser(pContext, uPrincipal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(ex.StackTrace, "Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
