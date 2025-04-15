using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Identity { get; set; }
            public string ErpNumber { get; set; }
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            var _tempList = new List<User>();
            try
            {
                using (PrincipalContext _context = new PrincipalContext(ContextType.Domain, "TIRETECH2", "OU=Users01,OU=Users,OU=arx1,OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com"))
                {
                    using (PrincipalSearcher _search = new PrincipalSearcher(new UserPrincipal(_context)))
                    {
                        foreach (UserPrincipal _result in _search.FindAll())
                        {
                            var _empNbr = string.Empty;
                            using (DirectoryEntry _entry = _result.GetUnderlyingObject() as DirectoryEntry)
                            {
                                if (_entry != null && _entry.Properties.Contains("global-ExtensionAttribute1"))
                                {
                                    _empNbr = _entry.Properties["global-ExtensionAttribute1"].Value.ToString();
                                }
                            }
                            _tempList.Add(new User
                            {
                                FirstName = _result.GivenName
                                ,LastName = _result.Surname
                                ,Identity = _result.EmployeeId
                                ,ErpNumber = _empNbr
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Message:\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            }
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
