using M2kClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SFW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        public static bool _aLock;
        public static bool AppLock
        {
            get { return _aLock; }
            set { _aLock = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(AppLock))); }
        }
        public static string _site;
        public static string Site
        {
            get { return _site; }
            set { _site = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Site))); }
        }
        public static string Facility
        {
            get { return SiteNumber == 1 ? "WCCO" : "CSI"; }
        }
        public static int _siteNbr;
        public static int SiteNumber
        {
            get { return _siteNbr; }
            set 
            {
                _siteNbr = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SiteNumber)));
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Facility)));
            }
        }
        public static bool _focused;
        public static bool IsFocused
        {
            get { return _focused; }
            set { _focused = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsFocused))); }
        }
        public static IDictionary<int, string> _filter;
        public static IDictionary<int, string> ViewFilter
        {
            get { return _filter; }
            set { _filter = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ViewFilter))); }
        }
        public static string _msg;
        public static string SplashMessage
        {
            get { return _msg; }
            set { _msg = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SplashMessage))); }
        }
        public static bool _inTrain;
        public static bool InTraining
        {
            get { return _inTrain; }
            set 
            {
                if (value)
                {
                    ErpCon.DatabaseChange(Database.CONTITRAIN, SiteNumber);
                }
                _inTrain = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InTraining)));
            }
        }

        //Hardcoded application location will need to be changed based on actual file path
        public static string AppFilePath { get { return "\\\\WAXFS001\\WAXG-SFW\\ShopFloorWorkbench\\"; } }

        public static IList<AppGlobal> GlobalConfig { get; set; }

        public static SqlConnection AppSqlCon { get; set; }
        public static M2kConnection ErpCon { get; set; }

        public static List<UserConfig> DefualtWorkCenter { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static ISplashScreen splashScreen;

        private readonly ManualResetEvent ResetSplashCreated;
        private readonly Thread SplashThread;

        public static Enumerations.UsersControls _loadMod;
        public static Enumerations.UsersControls LoadedModule 
        {
            get { return _loadMod; }
            set
            {
                _loadMod = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LoadedModule)));
            }
        }

        #endregion

        public App()
        {
            try
            {
                SiteNumber = CurrentUser.GetSite();
                //Setting up the splash screen
                ResetSplashCreated = new ManualResetEvent(false);
                SplashThread = new Thread(ShowSplash);
                SplashThread.SetApartmentState(ApartmentState.STA);
                SplashThread.IsBackground = true;
                SplashThread.Name = "Splash Screen";
                SplashThread.Start();
                ResetSplashCreated.WaitOne();

                //Initialization of default application properties
                SplashMessage = "Customizing your experience.";
                Site = "CONTI_MAIN";
                GlobalConfig = AppGlobal.Load($"{AppFilePath}GlobalConfig.xml");
                if (!CurrentUser.IsLoggedIn)
                {
                    CurrentUser.LogIn();
                }
                DefualtWorkCenter = UserConfig.GetUserConfigList();
                SplashMessage = "Connecting to your data.";
                if (AppSqlCon != null)
                {
                    AppSqlCon.Open();
                    while (AppSqlCon.State != System.Data.ConnectionState.Open) { }
                    AppSqlCon.StateChange += SqlCon_StateChange;
                }
                SplashMessage = "Making sure your errors are handled.";
                Current.Exit += App_Exit;
                AppDomain.CurrentDomain.UnhandledException += App_ExceptionCrash;
                Current.DispatcherUnhandledException += App_DispatherCrash;
                SystemEvents.PowerModeChanged += OnPowerChange;
                ViewFilter = new Dictionary<int, string>
                {
                    { 1, "" }
                    ,{ 2, "" }
                };
                SplashMessage = "Getting your schedule ready.  This may take a few moments.";
                var _load = Model.ModelBase.BuildMasterDataSet(UserConfig.GetIROD(), Site, AppSqlCon);
                if (_load.ContainsKey(true))
                {
                    var _msg = _load.TryGetValue(true, out string s) ? s : string.Empty;
                    MessageBox.Show(s, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                SplashMessage = string.Empty;
            }
            MainWindowViewModel.Initialization = true;
        }

        /// <summary>
        /// Application On Startup method for running cmd input overrides
        /// </summary>
        /// <param name="e">start up events sent from the application.exe</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] startUpArgs = null;
            try
            {
                startUpArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments?.ActivationData ?? null;
            }
            catch (NullReferenceException)
            {
                startUpArgs = e.Args;
            }
            if (startUpArgs != null)
            {
                foreach (string s in startUpArgs)
                {
                    var arg = s.Split('_');
                    //All start up command line arguments are to be added in the below switch statement as cases
                    switch (arg[0])
                    {
                        case "1":
                            CurrentUser.LogIn(arg[1]);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Show the dynamic splash screen
        /// </summary>
        private void ShowSplash()
        {
            var _splash = new DynamicSplashScreen();
            splashScreen = _splash;
            _splash.Show();
            Thread.Sleep(2000);
            ResetSplashCreated.Set();
            Dispatcher.Run();
        }

        /// <summary>
        /// SQLConnection state change watch
        /// Will try 10 times to reconnect and if unsuccessful will terminate the connection
        /// </summary>
        /// <param name="sender">empty object</param>
        /// <param name="e">Connection State Change Events</param>
        private static void SqlCon_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            var count = 0;
            while ((AppSqlCon.State == System.Data.ConnectionState.Broken || AppSqlCon.State == System.Data.ConnectionState.Closed) && count <= 5)
            {
                AppSqlCon.Open();
            }
        }

        /// <summary>
        /// SQLConnection Database change request
        /// </summary>
        /// <param name="dbName">Name of database to use</param>
        /// <returns>bool value for connection status; True = Pass, False = Failure</returns>
        public static bool DatabaseChange(string dbName)
        {
            try
            { 
                AppSqlCon.ChangeDatabase(dbName);
                Site = dbName;
                switch(dbName)
                {
                    case "CSI_MAIN":
                        SiteNumber = 2;
                        ErpCon.DatabaseChange(Database.CONTI, SiteNumber);
                        break;
                    case "WCCO_MAIN":
                        SiteNumber = 1;
                        ErpCon.DatabaseChange(Database.CONTI, SiteNumber);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// SQLConnection Database change request
        /// </summary>
        /// <param name="siteNbr">Site Number to switch</param>
        /// <returns>bool value for connection status; True = Pass, False = Failure</returns>
        public static bool DatabaseChange(int siteNbr)
        {
            var dbName = string.Empty;
            try
            {
                SiteNumber = siteNbr;
                ErpCon.DatabaseChange(Database.CONTI, siteNbr);
                AppSqlCon.ChangeDatabase(dbName);
                Site = dbName;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Application Exit Event
        /// </summary>
        /// <param name="sender">Current Application</param>
        /// <param name="e">Exit Event Arguments</param>
        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (AppSqlCon != null)
            {
                AppSqlCon.StateChange -= SqlCon_StateChange;
                AppSqlCon.Close();
                AppSqlCon.Dispose();
                AppSqlCon = null;
            }
        }

        /// <summary>
        /// Application UI Thread Exception Handler
        /// </summary>
        /// <param name="sender">Current Application</param>
        /// <param name="e">Dispatcher Thread Unhandled Exception</param>
        private static void App_DispatherCrash(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                if (AppSqlCon != null && AppSqlCon.State == System.Data.ConnectionState.Open)
                {
                    
                }
            }
            finally
            {
                if (AppSqlCon != null)
                {
                    AppSqlCon.StateChange -= SqlCon_StateChange;
                    AppSqlCon.Close();
                    AppSqlCon.Dispose();
                    AppSqlCon = null;
                }
                Current.Shutdown();
            }
        }

        /// <summary>
        /// Application Thread Domain Unhandled Exception Handler
        /// </summary>
        /// <param name="sender">Current Application Domain</param>
        /// <param name="e">Unhanded Exception</param>
        private static void App_ExceptionCrash(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                try
                {
                    if (AppSqlCon != null && AppSqlCon.State == System.Data.ConnectionState.Open)
                    {
                        var ex = (Exception)e.ExceptionObject;
                    }
                }
                finally
                {
                    AppSqlCon.StateChange -= SqlCon_StateChange;
                    AppSqlCon.Close();
                    AppSqlCon.Dispose();
                    AppSqlCon = null;
                    Current.Shutdown();
                }
            }
        }

        /// <summary>
        /// Called when the application PC switches power states
        /// </summary>
        /// <param name="sender">Current Application</param>
        /// <param name="e">Power Change</param>
        private void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    AppSqlCon.StateChange -= SqlCon_StateChange;
                    AppSqlCon.Close();
                    break;
                case PowerModes.Resume:
                    AppSqlCon.Open();
                    AppSqlCon.StateChange += SqlCon_StateChange;
                    break;
            }
        }

        /// <summary>
        /// Closes any window of type T in the application that is currently showing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CloseWindow<T>() where T : Window
        {
            foreach(Window w in Current.Windows)
            {
                if (w.GetType() == typeof(T))
                {
                    w.Close();
                }
            }
        }

        /// <summary>
        /// Focuses any window of type T in the application that is currently showing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataContext">Datacontext to pass into the focused window if it is already open</param>
        public static bool IsWindowOpen<T>(ViewModelBase dataContext = null) where T : Window
        {
            foreach (Window w in Current.Windows)
            {
                if (w.GetType() == typeof(T))
                {
                    w.Focus();
                    if (dataContext != null)
                    {
                        w.DataContext = dataContext;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get an open window in the application
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Window GetWindow<T>()
        {
            foreach (Window w in Current.Windows)
            {
                if (w.GetType() == typeof(T))
                {
                    return w;
                }
            }
            return null;
        }
    }
}
