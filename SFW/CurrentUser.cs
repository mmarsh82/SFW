using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace SFW
{
    public class CurrentUser
    {
        #region Properties

        private static string _dName;
        public static string DomainName
        {
            get
            { return _dName; }
            private set
            {
                _dName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DomainName)));
            }
        }

        private static string _disName;
        public static string DisplayName
        {
            get
            { return _disName; }
            private set
            {
                _disName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DisplayName)));
            }
        }

        private static string _email;
        public static string Email
        {
            get
            { return _email; }
            private set
            {
                _email = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Email)));
            }
        }

        private static bool _logged;
        public static bool IsLoggedIn
        {
            get
            { return _logged; }
            private set
            {
                _logged = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
        }

        private static bool _canSites;
        public static bool CanSchedule
        {
            get
            { return _canSites; }
            private set
            {
                _canSites = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanSchedule)));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CurrentUser()
        { }

        /// <summary>
        /// Current User overloaded constructor
        /// </summary>
        /// <param name="context">Domain principal context</param>
        /// <param name="user">User Principal for the active directory</param>
        public CurrentUser(PrincipalContext context, UserPrincipal user)
        {
            var _groups = user.GetAuthorizationGroups().Where(o => o.Name.Contains("SFW_")).ToList().ConvertAll(o => o.Name);
            DomainName = user.SamAccountName;
            DisplayName = user.DisplayName;
            Email = user.EmailAddress;
            CanSchedule = _groups.Exists(o => o.ToString().Contains("SFW_Sched"));
            IsLoggedIn = true;
            user.Dispose();
            context.Dispose();
        }

        /// <summary>
        /// SSO log in for a user
        /// </summary>
        public static void LogIn()
        {
            var _user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain), _user))
                {
                    if (uPrincipal.GetAuthorizationGroups().ToList().ConvertAll(o => o.Name.Contains("SFW_")).Count > 0)
                    {
                        new CurrentUser(pContext, uPrincipal);
                    }
                }
            }
        }

        /// <summary>
        /// Log in method for the current user
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="pwd">User password</param>
        /// <returns>Error that was encountered, will return null is no error was present</returns>
        public static string LogIn(string userName, string pwd)
        {
            try
            {
                using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, userName))
                    {
                        using (DirectoryEntry dEntry = uPrincipal.GetUnderlyingObject() as DirectoryEntry)
                        {
                            var _expireDate = Convert.ToDateTime(dEntry.InvokeGet("PasswordExpirationDate"));
                            if (uPrincipal.IsAccountLockedOut())
                            {
                                return "Your account is currently locked out.\nPlease contact IT for assistance.";
                            }
                            else if (uPrincipal.Enabled == false)
                            {
                                return "Your account is currently disabled.\nPlease contact IT for assistance.";
                            }
                            else if (_expireDate <= DateTime.Today)
                            {
                                return "Expired Password.";
                            }
                            else if (!pContext.ValidateCredentials(userName, pwd))
                            {
                                return "Invalid credentials.\nPlease check your user name and password and try again.\nIf you feel you have reached this message in error,\nplease contact IT for further assistance.";
                            }
                            else if (!uPrincipal.GetAuthorizationGroups().ToList().ConvertAll(o => o.Name).Exists(o => o.Contains("SFW_")))
                            {
                                return "Your account has not been flagged with the ability to log in.\nPlease contact IT for assistance.";
                            }
                            else
                            {
                                new CurrentUser(pContext, uPrincipal);
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "Your account does not exist on the domain.\nPlease contact IT for assistance.";
            }
        }

        /// <summary>
        /// Log off the current user
        /// </summary>
        public static void LogOff()
        {
            DomainName = null;
            DisplayName = null;
            Email = null;
            IsLoggedIn = false;
            CanSchedule = false;
            //TODO: make sure the schedule is on the site of the domain
        }

        /// <summary>
        /// Refresh the current users log in, use this if permissions have been changed
        /// </summary>
        public static void RefreshLogIn()
        {
            if (IsLoggedIn)
            {
                using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain), DomainName))
                    {
                        new CurrentUser(pContext, uPrincipal);
                    }
                }
            }
        }

        /// <summary>
        /// Update the a current users expired password
        /// </summary>
        /// <param name="userName">User name for the current user</param>
        /// <param name="oldPwd">Old password that is currently expired for the current user</param>
        /// <param name="newPwd">New password that the current user wants to change it to, this will be checked against the AD to make sure it meets the security requirements</param>
        /// <returns>Any error that was reflected from the AD, will be null if no errors occured</returns>
        public static string UpdatePassword(string userName, string oldPwd, string newPwd)
        {
            //TODO: add in the logic and arguments for updating the password
            return null;
        }

    }
}
