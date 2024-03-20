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

        private static bool _canLabor;
        public static bool CanLabor
        {
            get
            { return _canLabor; }
            private set
            {
                _canLabor = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanLabor)));
            }
        }

        private static bool _canTrain;
        public static bool CanTrain
        {
            get
            { return _canTrain; }
            private set
            {
                _canTrain = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanTrain)));
            }
        }

        private static bool _bUser;
        public static bool BasicUser
        {
            get
            { return _bUser; }
            private set
            {
                _bUser = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(BasicUser)));
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

        private static bool _canSplit;
        public static bool CanSplit
        {
            get
            { return _canSplit; }
            private set
            {
                _canSplit = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanSplit)));
            }
        }

        private static bool _isInvCtrl;
        public static bool IsInventoryControl
        {
            get
            { return _isInvCtrl; }
            private set
            {
                _isInvCtrl = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsInventoryControl)));
            }
        }

        private static bool _isQuality;
        public static bool IsQuality
        {
            get
            { return _isQuality; }
            private set
            {
                _isQuality = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsQuality)));
            }
        }

        private static bool _isAcctRec;
        public static bool IsAccountsReceivable
        {
            get
            { return _isAcctRec; }
            private set
            {
                _isAcctRec = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsAccountsReceivable)));
            }
        }

        private static bool _hasSOM;
        public static bool HasSalesOrderModule
        {
            get
            { return _hasSOM; }
            private set
            {
                _hasSOM = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(HasSalesOrderModule)));
            }
        }

        private static bool _isEng;
        public static bool IsEngineer
        {
            get
            { return _isEng; }
            private set
            {
                _isEng = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsEngineer)));
            }
        }

        private static bool _canDev;
        public static bool CanDeviate
        {
            get
            { return _canDev; }
            private set
            {
                _canDev = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CanDeviate)));
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

        private static string _site;
        public static string Site
        {
            get
            { return _site; }
            set
            {
                _site = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Site)));
            }
        }

        private static int _fac;
        public static int Facility
        {
            get
            { return _fac; }
            set
            {
                _fac = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Facility)));
            }
        }

        public static bool IsNamedUser { get; set; }

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
            var _aGroups = user.GetAuthorizationGroups().Where(o => o.Name.Contains("SFW-")).Select(o => o.Name).ToList();
            DomainName = context.ConnectedServer;
            DomainUserName = user.SamAccountName;
            DisplayName = user.DisplayName;
            Email = user.EmailAddress;
            Site = user.DistinguishedName.Contains("wak1") ? "WCCO" : "CSI";
            Facility = user.DistinguishedName.Contains("wak1") ? 1 : 2;
            if (_aGroups.Count() > 0)
            {
                if (_aGroups.Count(o => o.Contains("SFW-Admin")) > 0)
                {
                    CanTrain = CanSchedule = IsSupervisor = IsInventoryControl = IsAccountsReceivable = IsAdmin = HasSalesOrderModule = IsQuality = IsEngineer = CanSplit = CanDeviate = true;
                    BasicUser = false;
                }
                else
                {
                    CanSchedule = _aGroups.Count(o => o.Contains("SFW-Scheduler")) > 0;
                    IsSupervisor = _aGroups.Count(o => o.Contains("SFW-Supervisor")) > 0;
                    IsInventoryControl = _aGroups.Count(o => o.Contains("SFW-Inventory")) > 0;
                    IsAccountsReceivable = _aGroups.Count(o => o.Contains("SFW-AR")) > 0;
                    HasSalesOrderModule = _aGroups.Count(o => o.Contains("SFW-Sales")) > 0;
                    CanTrain = _aGroups.Count(o => o.Contains("SFW-Train")) > 0;
                    IsQuality = _aGroups.Count(o => o.Contains("SFW-Quality")) > 0;
                    IsEngineer = _aGroups.Count(o => o.Contains("SFW-Engineer")) > 0;
                    CanSplit = _aGroups.Count(o => o.Contains("SFW-Adjust")) > 0;
                    CanDeviate = _aGroups.Count(o => o.Contains("SFW-Deviate")) > 0;
                    BasicUser = false;
                }
            }
            else
            {
                BasicUser = true;
            }
            IsLoggedIn = true;
            CanWip = true;
            CanLabor = App.SiteNumber == 2;
            UserIDNbr = user.EmployeeId;
            FirstName = user.GivenName;
            LastName = user.Surname;
        }

        /// <summary>
        /// SSO log in for a user
        /// </summary>
        public static void LogIn()
        {
            if (!App.AppLock)
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
                    }
                    _user = _user.Contains("\\") ? _user.Split('\\')[1] : _user;
                    using (PrincipalContext pContext = GetPrincipal(_user))
                    {
                        using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, _user))
                        {
                            if (uPrincipal != null && !uPrincipal.DisplayName.Contains("_FA"))
                            {
                                if (!string.IsNullOrEmpty(uPrincipal.EmployeeId))
                                {
                                    IsNamedUser = true;
                                    new CurrentUser(pContext, uPrincipal);
                                }
                                else
                                {
                                    IsNamedUser = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// App Update log in for a user
        /// </summary>
        /// <param name="userName">User Name</param>
        public static void LogIn(string userName)
        {
            using (PrincipalContext pContext = GetPrincipal(userName))
            {
                using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext,userName))
                {
                    if (!uPrincipal.DisplayName.Contains("_FA"))
                    {
                        if (uPrincipal.GetAuthorizationGroups().ToList().ConvertAll(o => o.Name).Exists(o => o.Contains("SFW-")))
                        {
                            new CurrentUser(pContext, uPrincipal);
                            MainWindowViewModel.UpdateProperties(false);
                        }
                    }
                }
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
                using (PrincipalContext pContext = GetPrincipal(userName))
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, userName))
                    {
                        if (uPrincipal != null && uPrincipal.DisplayName.Contains("_FA"))
                        {
                            _resultKey = 5;
                            _resultVal = "Service accounts are denied.";
                            return _result;
                        }
                        using (DirectoryEntry dEntry = uPrincipal.GetUnderlyingObject() as DirectoryEntry)
                        {
                            var _expireDate = !uPrincipal.PasswordNeverExpires ? Convert.ToDateTime(dEntry.InvokeGet("PasswordExpirationDate")) : DateTime.Today.AddDays(1);
                            if (_expireDate <= DateTime.Today && _expireDate != new DateTime(1970, 1, 1))
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
                            else if (!pContext.ValidateCredentials(userName, pwd, ContextOptions.Negotiate))
                            {
                                _resultKey = 4;
                                _resultVal = "Invalid credentials.\nPlease check your user name and password and try again.\nIf you feel you have reached this message in error,\nplease contact IT for further assistance.";
                            }
                            if (!string.IsNullOrEmpty(_resultVal))
                            {
                                _result.Add(_resultKey, _resultVal);
                                return _result;
                            }
                            else
                            {
                                new CurrentUser(pContext, uPrincipal);
                                Controls.WorkSpaceDock.RefreshMainDock();
                                MainWindowViewModel.UpdateProperties(false);
                            }
                            _result.Add(_resultKey, _resultVal);
                            return _result;
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
        /// Check to see if the user exists in the current domain
        /// </summary>
        /// <param name="userName">Domain user name</param>
        /// <returns>Pass/Fail check as a boolean</returns>
        public static bool UserExist(string userName)
        {
            try
            {
                using (PrincipalContext pCon = GetPrincipal(userName))
                {
                    var _uPrincipal = UserPrincipal.FindByIdentity(pCon, userName);
                    return (_uPrincipal != null && !_uPrincipal.DisplayName.Contains("_FA"));
                }
            }
            catch(Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Get the site associated with the currently logged in user
        /// </summary>
        /// <returns>Site as string</returns>
        public static int GetSite()
        {
            var _user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            using (PrincipalContext pContext = GetPrincipal(_user))
            {
                using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, _user))
                {
                    if (uPrincipal.DistinguishedName.Contains("arx1"))
                    {
                        Site = "CSI";
                        return 2;
                    }
                    else if (uPrincipal.DistinguishedName.Contains("wak1"))
                    {
                        Site = "WCCO";
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// Set the site for the application to run
        /// </summary>
        /// <param name="siteNbr">Site number</param>
        /// <returns>site number</returns>
        public static int SetSite(int siteNbr)
        {
            if (siteNbr == 2)
            {
                Site = "CSI";
                return 2;
            }
            else if (siteNbr == 1)
            {
                Site = "WCCO";
                return 1;
            }
            else
            {
                return -1;
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
            IsAdmin = false;
            IsInventoryControl = false;
            IsSupervisor = false;
            UserIDNbr = string.Empty;
            IsAccountsReceivable = false;
            HasSalesOrderModule = false;
            CanTrain = false;
            BasicUser = true;
            IsEngineer = false;
            Controls.WorkSpaceDock.RefreshMainDock();
            MainWindowViewModel.UpdateProperties(false);
        }

        /// <summary>
        /// Refresh the current users log in, use this if permissions have been changed
        /// </summary>
        public static void RefreshLogIn()
        {
            if (IsLoggedIn)
            {
                using (PrincipalContext pContext = GetPrincipal(DomainUserName))
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, DomainUserName))
                    {
                        if (!uPrincipal.DisplayName.Contains("_FA"))
                        {
                            new CurrentUser(pContext, uPrincipal);
                            MainWindowViewModel.UpdateProperties(true);
                        }
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
            using (PrincipalContext context = GetPrincipal(userName))
            {
                try
                {
                    using (UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
                        user.ChangePassword(oldPwd, newPwd);
                        user.Save();
                    }
                }
                catch(PasswordException passEx)
                {
                    if (passEx.Message.Contains("HRESULT: 0x80070056"))
                        return "The old password is incorrect";
                    else if (passEx.Message.Contains("HRESULT: 0x800708C5"))
                        return "The password does not meet the password policy requirements.";
                    else
                        return passEx.Message;
                }
                catch(PrincipalOperationException poEx)
                {
                    if (poEx.Message.Contains("HRESULT: 0x80070056"))
                        return "Account is locked out.";
                    else
                        return poEx.Message;
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// Get a Dynamic PrincipalContext based on the username submitted
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>Dynamic PrincipalContext</returns>
        public static PrincipalContext GetPrincipal(string username)
        {
            if (username.Contains("\\"))
            {
                var uSplit = username.Split('\\');
                return new PrincipalContext(ContextType.Domain, uSplit[0]);
            }
            else
            {
                var _site = App.SiteNumber == 1 ? "wak1" : "arx1";
                return new PrincipalContext(ContextType.Domain);
            }
        }
    }
}
