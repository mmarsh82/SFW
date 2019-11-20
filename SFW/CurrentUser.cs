using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;

namespace SFW
{
    /// <summary>
    /// Current User Object
    /// </summary>
    public class CurrentUser
    {
        #region Properties

        private static string _dUserName;
        public static string DomainUserName
        {
            get
            { return _dUserName; }
            private set
            {
                _dUserName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DomainUserName)));
            }
        }

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

        private static string _fName;
        public static string FirstName
        {
            get
            { return _fName; }
            private set
            {
                _fName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(FirstName)));
            }
        }

        private static string _lName;
        public static string LastName
        {
            get
            { return _lName; }
            private set
            {
                _lName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastName)));
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

        private static bool _canShed;
        public static bool CanSchedule
        {
            get
            { return _canShed; }
            private set
            {
                _canShed = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanSchedule)));
            }
        }

        private static bool _canWip;
        public static bool CanWip
        {
            get
            { return _canWip; }
            private set
            {
                _canWip = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanWip)));
            }
        }

        private static bool _isAdmin;
        public static bool IsAdmin
        {
            get
            { return _isAdmin; }
            private set
            {
                _isAdmin = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsAdmin)));
            }
        }

        private static bool _isSuper;
        public static bool IsSupervisor
        {
            get
            { return _isSuper; }
            private set
            {
                _isSuper = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsSupervisor)));
            }
        }

        private static string _uID;
        public static string UserIDNbr
        {
            get
            { return _uID; }
            private set
            {
                _uID = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(UserIDNbr)));
            }
        }

        private static bool _isLocked;
        public static bool IsLocked
        {
            get
            { return _isLocked; }
            set
            {
                _isLocked = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsLocked)));
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
            var _groups = new List<string>();
            try
            {
                _groups = user.GetAuthorizationGroups().Where(o => o.Name.Contains("SFW_")).ToList().ConvertAll(o => o.Name);
            }
            catch
            {
                _groups = user.GetGroups().Where(o => o.Name.Contains("SFW_")).ToList().ConvertAll(o => o.Name);
            }
            DomainName = context.ConnectedServer;
            DomainUserName = user.SamAccountName;
            DisplayName = user.DisplayName;
            Email = user.EmailAddress;
            CanSchedule = _groups.Exists(o => o.ToString().Contains("SFW_Sched"));
            IsAdmin = _groups.Exists(o => o.ToString().Contains("SFW_Admin"));
            IsSupervisor = _groups.Exists(o => o.ToString().Contains("SFW_Super"));
            IsLoggedIn = true;
            CanWip = user.UserPrincipalName.Contains("wcco");
            UserIDNbr = user.EmployeeId;
            FirstName = user.GivenName;
            LastName = user.Surname;
            context.Dispose();
            user.Dispose();
        }

        /// <summary>
        /// SSO log in for a user
        /// </summary>
        public static void LogIn()
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
            }
            using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, _user))
                {
                    if (uPrincipal.GetAuthorizationGroups().ToList().ConvertAll(o => o.Name).Exists(o => o.Contains("SFW_")))
                    {
                        new CurrentUser(pContext, uPrincipal);
                    }
                }
            }
        }

        /// <summary>
        /// App Update log in for a user
        /// </summary>
        /// <param name="userName">User Name</param>
        public static void LogIn(string userName)
        {
            using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, userName))
                {
                    if (uPrincipal.GetAuthorizationGroups().ToList().ConvertAll(o => o.Name).Exists(o => o.Contains("SFW_")))
                    {
                        new CurrentUser(pContext, uPrincipal);
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if the user exists in the current domain
        /// </summary>
        /// <param name="userName">Domain user name</param>
        /// <returns>Pass/Fail check as a boolean</returns>
        public static bool UserExist(string userName)
        {
            try
            {
                using (PrincipalContext pCon = new PrincipalContext(ContextType.Domain))
                {
                    return (UserPrincipal.FindByIdentity(pCon, userName) != null);
                }
            }
            catch(Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Log in method for the current user
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="pwd">User password</param>
        /// <returns>
        /// IReadOnlyDictionary
        /// Key is Pass/Fail check passed back as an int value of error
        /// Value is Error that was encountered as a string, will return empty string on 0 key
        /// </returns>
        public static IReadOnlyDictionary<int, string> LogIn(string userName, string pwd)
        {
            var _result = new Dictionary<int, string>();
            var _resultKey = 0;
            var _resultVal = string.Empty;
            try
            {
                using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, userName))
                    {
                        using (DirectoryEntry dEntry = uPrincipal.GetUnderlyingObject() as DirectoryEntry)
                        {
                            var _expireDate = !uPrincipal.PasswordNeverExpires ?  Convert.ToDateTime(dEntry.InvokeGet("PasswordExpirationDate")) : DateTime.Today.AddDays(1);
                            if(_expireDate <= DateTime.Today && _expireDate != new DateTime(1970,1,1))
                            {
                                _resultKey = 1;
                                _resultVal = "Expired Password.";
                            }
                            else if (uPrincipal.IsAccountLockedOut())
                            {
                                _resultKey = 2;
                                _resultVal = "Your account is currently locked out.\nPlease contact IT for assistance.";
                            }
                            else if (uPrincipal.Enabled == false)
                            {
                                _resultKey = 3;
                                _resultVal = "Your account is currently disabled.\nPlease contact IT for assistance.";
                            }
                            else if (!pContext.ValidateCredentials(userName, pwd))
                            {
                                _resultKey = 4;
                                _resultVal =  "Invalid credentials.\nPlease check your user name and password and try again.\nIf you feel you have reached this message in error,\nplease contact IT for further assistance.";
                            }
                            if (!string.IsNullOrEmpty(_resultVal))
                            {
                                _result.Add(_resultKey, _resultVal);
                                return _result;
                            }
                            else
                            {
                                new CurrentUser(pContext, uPrincipal);
                                _result.Add(_resultKey, _resultVal);
                                return _result;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                _resultKey = -1;
                _resultVal = "Your account does not exist on the domain.\nPlease contact IT for assistance.";
                _result.Add(_resultKey, _resultVal);
                return _result;
            }
        }

        /// <summary>
        /// Log off the current user
        /// </summary>
        public static void LogOff()
        {
            DomainName = null;
            DomainUserName = null;
            DisplayName = null;
            Email = null;
            IsLoggedIn = false;
            CanSchedule = false;
            CanWip = false;
            UserIDNbr = string.Empty;
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
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, DomainUserName))
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
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                try
                {
                    using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
                        
                        user.ChangePassword(oldPwd, newPwd);
                        user.Save();
                    }
                }
                catch(PasswordException e)
                {
                    if (e.Message == "The specified network password is not correct. (Exception from HRESULT: 0x80070056)")
                        return "The old password is incorrect";
                    else if (e.Message == "The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements. (Exception from HRESULT: 0x800708C5)")
                        return "The password does not meet the password policy requirements.";
                    else
                        return e.Message;
                }
                catch(Exception e)
                {
                    
                    return "Unknown error";
                }
            }


            return null;
        }

    }
}
