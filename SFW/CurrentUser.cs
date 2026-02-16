using SFW.Commands;
using SFW.Controls;
using SFW.Model;
using SFW.Model.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Windows.Controls;

namespace SFW
{
    /// <summary>
    /// Current User Object
    /// </summary>
    public class CurrentUser
    {
        public class ValidUser
        {
            #region Properties

            public bool Validated;
            public int ErrorKey;
            public string ErrorMessage;
            public List<string> Groups;
            public string DomainName;
            public string DomainUserName;
            public string DisplayName;
            public string Email;
            public string Site;
            public int Facility;
            public IReadOnlyDictionary<int, string> DirectReports;
            public int SapId;
            public string ErpId;
            public string GivenName;
            public string SurName;

            #endregion

            public ValidUser()
            { }

            /// <summary>
            /// Current User overloaded constructor
            /// </summary>
            /// <param name="context">Domain principal context</param>
            /// <param name="user">User Principal for the active directory</param>
            public ValidUser(PrincipalContext context, UserPrincipal user)
            {
                try
                {
                    Groups = GetGroupMembership(user);
                    DomainName = context.ConnectedServer;
                    DomainUserName = user.SamAccountName;
                    DisplayName = user.DisplayName;
                    Email = user.EmailAddress;
                    Site = "Wahpeton";
                    Facility = 1;
                    DirectReports = IsSupervisor && App.SiteNumber == 1 ? user.GetDirectReports() : new Dictionary<int, string>();
                    SapId = int.TryParse(((DirectoryEntry)user.GetUnderlyingObject()).Properties["global-ExtensionAttribute1"]?.Value.ToString(), out int i) ? i : 0;
                    ErpId = ModelBase.MasterDataSet == null || !ModelBase.MasterDataSet.Tables.Contains(typeof(Employee).Name) ? Employee.GetErpID(SapId, App.AppSqlCon) : Employee.GetErpID(SapId);
                    GivenName = user.GivenName;
                    SurName = user.Surname;
                }
                catch (Exception)
                {

                }
            }
        }

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

        private static bool _isManager;
        public static bool IsManager
        {
            get
            { return _isManager; }
            private set
            {
                _isManager = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsManager)));
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

        private static bool _hasNotice;
        public static bool HasNotice
        {
            get
            { return _hasNotice; }
            private set
            {
                _hasNotice = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(HasNotice)));
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

        private static bool _planner;
        public static bool Planner
        {
            get
            { return _planner; }
            private set
            {
                _planner = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Planner)));
            }
        }

        private static bool _hasCon;
        public static bool HasContainers
        {
            get
            { return _hasCon; }
            set
            {
                _hasCon = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(HasContainers)));
            }
        }

        private static string _erp;
        public static string ErpId
        {
            get
            { return _erp; }
            private set
            {
                _erp = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ErpId)));
            }
        }

        private static int _sap;
        public static int SapId
        {
            get
            { return _sap; }
            private set
            {
                _sap = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SapId)));
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

        private static IReadOnlyDictionary<int, string> _reports;
        public static IReadOnlyDictionary<int, string> DirectReports
        {
            get
            { return _reports; }
            set
            {
                _reports = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DirectReports)));
            }
        }

        private static List<ModuleType> _mods;
        public static List<ModuleType> Modules
        {
            get
            { return _mods; }
            set
            {
                _mods = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Modules)));
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
        /// <param name="user">Valid user object</param>
        public CurrentUser(ValidUser user)
        {
            try
            {
                DomainName = user.DomainName;
                DomainUserName = user.DomainUserName;
                DisplayName = user.DisplayName;
                Email = user.Email;
                Site = user.Site;
                Facility = user.Facility;
                App.IsFocused = BasicUser = AssignPermissions(user.Groups);
                Modules = GetModulesList();
                DirectReports = user.DirectReports;
                IsLoggedIn = true;
                CanWip = true;
                CanLabor = App.SiteNumber == 2 || IsAdmin;
                SapId = user.SapId;
                ErpId = user.ErpId;
                FirstName = user.GivenName;
                LastName = user.SurName;
                MainWindowViewModel.UpdateProperties(false);
                if (WorkSpaceDock.MainDock != null)
                {
                    ((Schedule.ViewModel)((Schedule.View)((DockPanel)WorkSpaceDock.MainDock.Children[1]).Children[0]).DataContext).ResetFilter();
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Current User overloaded constructor
        /// </summary>
        /// <param name="context">Domain principal context</param>
        /// <param name="user">User Principal for the active directory</param>
        public CurrentUser(PrincipalContext context, UserPrincipal user)
        {
            try
            {
                var _groups = GetGroupMembership(user);
                App.IsFocused = BasicUser = AssignPermissions(_groups);
                DomainName = context.ConnectedServer;
                DomainUserName = user.SamAccountName;
                DisplayName = user.DisplayName;
                Email = user.EmailAddress;
                Site = "Wahpeton";
                Facility = 1;
                Modules = GetModulesList();
                DirectReports = IsSupervisor && App.SiteNumber == 1 ? user.GetDirectReports() : new Dictionary<int, string>();
                IsLoggedIn = true;
                CanWip = true;
                CanLabor = IsAdmin;
                SapId = int.TryParse(((DirectoryEntry)user.GetUnderlyingObject()).Properties["global-ExtensionAttribute1"]?.Value.ToString(), out int i) ? i : 0;
                ErpId = ModelBase.MasterDataSet == null || !ModelBase.MasterDataSet.Tables.Contains(typeof(Employee).Name) ? Employee.GetErpID(SapId, App.AppSqlCon) : Employee.GetErpID(SapId);
                FirstName = user.GivenName;
                LastName = user.Surname;
                if (WorkSpaceDock.MainDock != null)
                {
                    ((Schedule.ViewModel)((Schedule.View)((DockPanel)WorkSpaceDock.MainDock.Children[1]).Children[0]).DataContext).ResetFilter();
                }
            }
            catch (PrincipalServerDownException)
            {

            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Assign all application permissions to the current user based on AD groups
        /// </summary>
        /// <param name="groups">Signed in user principal</param>
        /// <returns>Basic user status</returns>
        public bool AssignPermissions(List<string> groups)
        {
            try
            {
                if (groups.Count() > 0)
                {
                    foreach (var _group in groups)
                    {
                        switch (_group)
                        {
                            case "Admin":
                                CanTrain = CanSchedule = IsSupervisor = IsManager = IsInventoryControl = IsAccountsReceivable = IsAdmin = HasSalesOrderModule = IsQuality = IsEngineer = CanSplit = CanDeviate = HasNotice = Planner = HasContainers = true;
                                return false;
                            case "Scheduler":
                                CanSchedule = true;
                                break;
                            case "Supervisor":
                                IsSupervisor = true;
                                break;
                            case "Manager":
                                IsManager = true;
                                break;
                            case "Inventory":
                                IsInventoryControl = HasContainers = true;
                                break;
                            case "AR":
                                IsAccountsReceivable = true;
                                break;
                            case "Sales":
                                HasSalesOrderModule = true;
                                break;
                            case "Train":
                                CanTrain = true;
                                break;
                            case "Quality":
                                IsQuality = HasNotice = true;
                                break;
                            case "QNotice":
                                HasNotice = true;
                                break;
                            case "Engineer":
                                IsEngineer = true;
                                break;
                            case "Adjust":
                                CanSplit = true;
                                break;
                            case "Deviate":
                                CanDeviate = true;
                                break;
                            case "Planner":
                                Planner = true;
                                break;
                            case "Containers":
                                HasContainers = true;
                                break;
                        }
                    }
                    if (groups.Count == 1)
                    {
                        return groups.Count(o => o == "Containers") == 1;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// App Update log in for a user
        /// </summary>
        /// <param name="userName">User Name</param>
        public static void LogIn(string userName)
        {
            //userName = "UIF89547";
            using (PrincipalContext pContext = GetPrincipal(userName))
            {
                if (pContext != null)
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, userName))
                    {
                        if (uPrincipal != null && !uPrincipal.DisplayName.Contains("_FA"))
                        {
                            new CurrentUser(pContext, uPrincipal);
                        }
                        else
                        {
                            App.IsFocused = BasicUser = true;
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
                                WorkSpaceDock.RefreshMainDock(true);
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
        /// Get the site associated with the currently logged in user
        /// </summary>
        /// <returns>Site as string</returns>
        public static int GetSite() => 1;

        /// <summary>
        /// Get the group memebership of a User from SQL
        /// </summary>
        /// <param name="userName">User identity</param>
        /// <returns>List of group memebership names</returns>
        public static List<string> GetGroupMembership(UserPrincipal userPrincipal)
        {
            var _groups = userPrincipal.GetAuthorizationGroups();
            var _rtnList = new List<string>();
            foreach (var _group in _groups.Where(o => o.Name.Contains("-SFW-")))
            {
                _rtnList.Add(_group.Name.Replace("WAXSG-SFW-", ""));
            }
            return _rtnList;

            /*var _ou = App.Facility == "Wahpeton" ? "WAK1" : "ARX1";
            var _group = App.Facility == "Wahpeton" ? "WAX" : "ARX";
            var _rtnList = new List<string>();
            var _cmdString = $@"SELECT
	CASE WHEN adj.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Adjust'
	,CASE WHEN adm.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Admin'
	,CASE WHEN dev.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Deviate'
	,CASE WHEN eng.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Engineer'
	,CASE WHEN inv.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Inventory'
	,CASE WHEN mgr.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Manager'
	,CASE WHEN qnot.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'QNotice'
	,CASE WHEN qlt.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Quality'
	,CASE WHEN sale.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Sales'
	,CASE WHEN schd.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Scheduler'
	,CASE WHEN super.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Supervisor'
	,CASE WHEN train.[SAMAccountName] IS NOT NULL
		THEN 1
		ELSE 0 END as 'Train'
FROM
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' WHERE SAMAccountName = ''{userName}'' ') main
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Adjust,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') adj
	ON main.[SAMAccountName] = adj.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Admin,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') adm
	ON main.[SAMAccountName] = adm.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Deviate,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') dev
	ON main.[SAMAccountName] = dev.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Engineer,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') eng
	ON main.[SAMAccountName] = eng.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Inventory,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') inv
	ON main.[SAMAccountName] = inv.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Manager,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') mgr
	ON main.[SAMAccountName] = mgr.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-QNotice,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') qnot
	ON main.[SAMAccountName] = qnot.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Quality,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') qlt
	ON main.[SAMAccountName] = qlt.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Sales,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') sale
	ON main.[SAMAccountName] = sale.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Scheduler,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') schd
	ON main.[SAMAccountName] = schd.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Supervisor,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') super
	ON main.[SAMAccountName] = super.[SAMAccountName]
LEFT JOIN
	OPENQUERY(ADSI, 'Select SAMAccountName from ''LDAP://OU={_ou},OU=US,OU=LDA,DC=TIRETECH2,DC=CONTIWAN,DC=COM'' 
	WHERE memberof= ''CN={_group}SG-SFW-Train,OU=SoftwareDistributionLocal,OU=Groups,OU={_ou},OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com'' ') train
	ON main.[SAMAccountName] = train.[SAMAccountName]";
            if (App.AppSqlCon != null && App.AppSqlCon.State != ConnectionState.Closed && App.AppSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(_cmdString, App.AppSqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _colList = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                                    foreach (var _col in _colList)
                                    {
                                        if (reader.SafeGetBoolean(_col))
                                        {
                                            _rtnList.Add(_col);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return _rtnList;
                }
                catch (SqlException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }*/
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
            IsSupervisor = IsManager = false;
            ErpId = string.Empty;
            SapId = 0;
            IsAccountsReceivable = false;
            HasSalesOrderModule = false;
            CanTrain = false;
            App.IsFocused = BasicUser = true;
            IsEngineer = false;
            IsQuality = HasNotice = false;
            Planner = false;
            IsManager = false;
            Modules = GetModulesList();
            ModelBase.LoadedModules = Module.GetModuleList(Modules);
            ModelBase.ModelFacility = Facility;
            WorkSpaceDock.RefreshMainDock(false);
            MainWindowViewModel.UpdateProperties(false);
            ((Schedule.ViewModel)((Schedule.View)((DockPanel)WorkSpaceDock.MainDock.Children[1]).Children[0]).DataContext).ResetFilter();
            if (App.LoadedModule != Enumerations.UsersControls.Schedule)
            {
                new ViewLoad().Execute(App.SiteNumber);
                App.LoadedModule = Enumerations.UsersControls.Schedule;
            }
        }

        /// <summary>
        /// Check to see if the user exists in the current domain and the credentials are valid
        /// </summary>
        /// <param name="userName">Domain user name</param>
        /// <param name="password">User password</param>
        /// <returns>Pass/Fail check as a boolean</returns>
        public static ValidUser ValidateCredentials(string userName, string password)
        {
            var _user = new ValidUser() { Validated = false, ErrorKey = 0, ErrorMessage = string.Empty };
            try
            {
                using (PrincipalContext pContext = GetPrincipal(userName))
                {
                    using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, userName))
                    {
                        if (uPrincipal != null && uPrincipal.DisplayName.Contains("_FA"))
                        {
                            _user.ErrorKey = 5;
                            _user.ErrorMessage = "Service accounts are denied.";
                            return _user;
                        }
                        using (DirectoryEntry dEntry = uPrincipal.GetUnderlyingObject() as DirectoryEntry)
                        {
                            var _expireDate = !uPrincipal.PasswordNeverExpires ? Convert.ToDateTime(dEntry.InvokeGet("PasswordExpirationDate")) : DateTime.Today.AddDays(1);
                            if (_expireDate <= DateTime.Today && _expireDate != new DateTime(1970, 1, 1))
                            {
                                _user.ErrorKey = 1;
                                _user.ErrorMessage = "Expired Password.";
                            }
                            else if (uPrincipal.IsAccountLockedOut())
                            {
                                _user.ErrorKey = 2;
                                _user.ErrorMessage = "Your account is currently locked out.\nPlease contact IT for assistance.";
                            }
                            else if (uPrincipal.Enabled == false)
                            {
                                _user.ErrorKey = 3;
                                _user.ErrorMessage = "Your account is currently disabled.\nPlease contact IT for assistance.";
                            }
                            else if (!pContext.ValidateCredentials(userName, password, ContextOptions.Negotiate))
                            {
                                _user.ErrorKey = 4;
                                _user.ErrorMessage = "Invalid credentials.\nPlease check your user name and password and try again.\nIf you feel you have reached this message in error,\nplease contact IT for further assistance.";
                            }
                            if (string.IsNullOrEmpty(_user.ErrorMessage))
                            {
                                _user = new ValidUser(pContext, uPrincipal)
                                {
                                    Validated = true
                                };

                            }
                            return _user;
                        }
                    }
                }
            }
            catch (Exception)
            {
                _user.ErrorKey = -1;
                _user.ErrorMessage = "Your account does not exist on the domain.\nPlease contact IT for assistance.";
                return _user;
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
            return new PrincipalContext(ContextType.Domain, username.Contains("\\") ? username.Split('\\')[0] : "TIRETECH2");
        }

        /// <summary>
        /// Load the model master data set based on user permissions
        /// </summary>
        /// <returns></returns>
        public static List<ModuleType> GetModulesList()
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                var _tempList = new List<ModuleType>
                {
                    ModuleType.Production
                    ,ModuleType.Product
                    ,ModuleType.Sales
                    ,ModuleType.Management
                };
                if (App.SiteNumber == 1)
                {
                    _tempList.Add(ModuleType.Quality);
                }
                return _tempList;
            }
            else if (IsAdmin)
            {
                return Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>().ToList();
            }
            else
            {
                var _tempList = new List<ModuleType>
                {
                    ModuleType.Production
                    ,ModuleType.Product
                    ,ModuleType.Sales
                    ,ModuleType.Management
                };
                if (Facility == 1)
                {
                    _tempList.Add(ModuleType.Quality);
                    if (CanSchedule || Planner)
                    {
                        _tempList.Add(ModuleType.SupplyChain);
                    }
                    if (IsInventoryControl)
                    {
                        _tempList.Add(ModuleType.Containers);
                        _tempList.Add(ModuleType.InventoryControl);
                        _tempList.Add(ModuleType.CycleCount);
                    }
                }
                return _tempList;
            }
        }
    }
}
