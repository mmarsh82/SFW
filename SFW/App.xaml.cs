using M2kClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

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
        public static int _siteNbr;
        public static int SiteNumber
        {
            get { return _siteNbr; }
            set { _siteNbr = value; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(SiteNumber))); }
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

        //Hardcoded application location will need to be changed based on actual file path
        public static string AppFilePath { get { return "\\\\fs-wcco\\WCCO-SFW\\ShopFloorWorkbench\\"; } }

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
            //Setting up the splash screen
            ResetSplashCreated = new ManualResetEvent(false);
            SplashThread = new Thread(ShowSplash);
            SplashThread.SetApartmentState(ApartmentState.STA);
            SplashThread.IsBackground = true;
            SplashThread.Name = "Splash Screen";
            SplashThread.Start();
            ResetSplashCreated.WaitOne();

            //Initialization of default application properties
            SiteNumber = CurrentUser.GetSite();
            Site = $"{CurrentUser.Site}_MAIN";
            GlobalConfig = LoadGlobalAppConfig();
            if (!CurrentUser.IsLoggedIn)
            {
                CurrentUser.LogIn();
            }
            DefualtWorkCenter = UserConfig.GetUserConfigList();
            ErpCon.DatabaseChange(Enum.TryParse(Site.Replace("_MAIN", ""), out Database _db) ? _db : Database.CSI);
            if (AppSqlCon != null)
            {
                AppSqlCon.Open();
                while (AppSqlCon.State != System.Data.ConnectionState.Open) { }
                AppSqlCon.StateChange += SqlCon_StateChange;
            }
            Current.Exit += App_Exit;
            AppDomain.CurrentDomain.UnhandledException += App_ExceptionCrash;
            Current.DispatcherUnhandledException += App_DispatherCrash;
            SystemEvents.PowerModeChanged += OnPowerChange;
            ViewFilter = new Dictionary<int, string>
            {
                { 0, "" }
                ,{ 1, "" }
            };
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
                        SiteNumber = 0;
                        ErpCon.DatabaseChange(Database.CSI);
                        break;
                    case "WCCO_MAIN":
                        SiteNumber = 1;
                        ErpCon.DatabaseChange(Database.WCCO);
                        break;
                    case "CSI_TRAIN":
                        SiteNumber = 0;
                        ErpCon.DatabaseChange(Database.CSITRAIN);
                        break;
                    case "WCCO_TRAIN":
                        SiteNumber = 1;
                        ErpCon.DatabaseChange(Database.WCCOTRAIN);
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
                switch (siteNbr)
                {
                    case 0:
                        SiteNumber = siteNbr;
                        ErpCon.DatabaseChange(Database.CSI);
                        dbName = "CSI_MAIN";
                        break;
                    case 1:
                        SiteNumber = siteNbr;
                        ErpCon.DatabaseChange(Database.WCCO);
                        dbName = "WCCO_MAIN";
                        break;
                    case 2:
                        SiteNumber = 0;
                        ErpCon.DatabaseChange(Database.CSITRAIN);
                        dbName = "CSI_TRAIN";
                        break;
                    case 3:
                        SiteNumber = 1;
                        ErpCon.DatabaseChange(Database.WCCOTRAIN);
                        dbName = "WCCO_TRAIN";
                        break;
                }
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
        /// Loads the global config file, if none exists but the location is valid will create one
        /// </summary>
        /// <returns>Config file existance or creation</returns>
        private IList<AppGlobal> LoadGlobalAppConfig()
        {
            try
            {
                //TODO: need to add in the version check here and rewrite when the version is updated
                if (!File.Exists($"{AppFilePath}GlobalConfig.xml"))
                {
                    using (var wStream = new FileStream($"{AppFilePath}GlobalConfig.xml", FileMode.CreateNew))
                    {
                        var wSettings = new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = true };
                        using (var writer = XmlWriter.Create(wStream, wSettings))
                        {
                            writer.WriteComment("SFW Global Config File");
                            writer.WriteStartElement("GlobalConfig");
                            writer.WriteAttributeString("Version", "1.0");

                            writer.WriteComment("Application parameters");
                            writer.WriteStartElement("SFWApp");
                            writer.WriteAttributeString("IsLocked", "");
                            writer.WriteEndElement();

                            writer.WriteComment("ERP connection parameters");
                            writer.WriteStartElement("M2kConnection");
                            writer.WriteAttributeString("Name", "");
                            writer.WriteAttributeString("IP", "");
                            writer.WriteAttributeString("ServiceUser", "");
                            writer.WriteAttributeString("ServicePass", "");
                            writer.WriteEndElement();

                            writer.WriteComment("SFW Automatic Refresh rate");
                            writer.WriteStartElement("RefreshRate");
                            writer.WriteAttributeString("Hours", "0");
                            writer.WriteAttributeString("Minutes", "5");
                            writer.WriteAttributeString("Seconds", "0");
                            writer.WriteAttributeString("MilliSeconds", "0");
                            writer.WriteEndElement();

                            writer.WriteComment("SQL connection parameters");
                            writer.WriteComment("Connection Timeout is in seconds");
                            writer.WriteStartElement("SqlConnection");
                            writer.WriteAttributeString("Name", "");
                            writer.WriteAttributeString("IP", "");
                            writer.WriteAttributeString("ServiceUser", "");
                            writer.WriteAttributeString("ServicePass", "");
                            writer.WriteAttributeString("TimeOut", "60");
                            writer.WriteEndElement();

                            writer.WriteComment("Production Work Enviroment");
                            writer.WriteStartElement("Shifts");
                            writer.WriteComment("All shifts must be in a military time format");
                            writer.WriteComment("Shift total must not exceed a 24 hour day");
                            writer.WriteStartElement("First");
                            writer.WriteAttributeString("Start", "07:00");
                            writer.WriteAttributeString("End", "14:59");
                            writer.WriteEndElement();
                            writer.WriteStartElement("Second");
                            writer.WriteAttributeString("Start", "15:00");
                            writer.WriteAttributeString("End", "22:59");
                            writer.WriteEndElement();
                            writer.WriteStartElement("Third");
                            writer.WriteAttributeString("Start", "23:00");
                            writer.WriteAttributeString("End", "06:59");
                            writer.WriteEndElement();
                            writer.WriteEndElement();

                            writer.WriteComment("Supporting document file paths");
                            writer.WriteStartElement("HelpFile");
                            writer.WriteAttributeString("Path", "");
                            writer.WriteEndElement();

                            writer.WriteComment("All Site documentation file paths must be contained below");
                            writer.WriteComment("If you are incorparating a new file path you will need to contact the developer to add in the corresponding properties");
                            writer.WriteStartElement("SiteDocumentation");

                            writer.WriteStartElement("WCCO");
                            writer.WriteAttributeString("PartPrint", "");
                            writer.WriteAttributeString("PressSetup", "");
                            writer.WriteAttributeString("SyscoSetup", "");
                            writer.WriteAttributeString("TrimSetup", "");
                            writer.WriteAttributeString("WI", "");
                            writer.WriteEndElement();

                            writer.WriteStartElement("CSI");
                            writer.WriteAttributeString("PartPrint", "");
                            writer.WriteAttributeString("Setup", "");
                            writer.WriteAttributeString("WI", "");
                            writer.WriteEndElement();

                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }
                    }
                }
                var _tempList = new List<AppGlobal>();
                using (var rStream = new FileStream($"{AppFilePath}GlobalConfig.xml", FileMode.Open))
                {
                    var rSettings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };
                    using (var reader = XmlReader.Create(rStream, rSettings))
                    {
                        while (reader.Read())
                        {
                            if (reader.HasAttributes)
                            {
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    switch (reader.Name)
                                    {
                                        case "SFWApp":
                                            AppLock = bool.TryParse(reader.GetAttribute("IsLocked"), out bool b) ? b : true;
                                            break;
                                        case "M2kConnection":
                                            ErpCon = new M2kConnection(reader.GetAttribute("Name"), reader.GetAttribute("ServiceUser"), reader.GetAttribute("ServicePass"), Database.CSI);
                                            break;
                                        case "RefreshRate":
                                            var _hour = int.TryParse(reader.GetAttribute("Hours"), out int h) ? h : 0;
                                            var _min = int.TryParse(reader.GetAttribute("Minutes"), out int m) ? m : 5;
                                            var _sec = int.TryParse(reader.GetAttribute("Seconds"), out int s) ? s : 0;
                                            var _mSec = int.TryParse(reader.GetAttribute("MilliSeconds"), out int mls) ? mls : 0;
                                            RefreshTimer.Start(new TimeSpan(0, _hour, _min, _sec, _mSec));
                                            break;
                                        case "SqlConnection":
                                            AppSqlCon = new SqlConnection($@"Server={reader.GetAttribute("IP")};
                                                                            User ID={reader.GetAttribute("ServiceUser")};
                                                                            Password={reader.GetAttribute("ServicePass")};
                                                                            DataBase={Site};
                                                                            Connection Timeout={reader.GetAttribute("TimeOut")};
                                                                            MultipleActiveResultSets=True;
                                                                            Max Pool Size=3;
                                                                            Pooling=False;");
                                            break;
                                        //SiteDocumentation Element is written below
                                        //Make sure any site added in the SiteDocumentation element exists in the main application site list
                                        case "WCCO":
                                            _tempList.Add(new AppGlobal
                                            {
                                                Site = "WCCO"
                                                ,PartPrint = reader.GetAttribute("PartPrint")
                                                ,PressSetup = reader.GetAttribute("PressSetup")
                                                ,SyscoSetup = reader.GetAttribute("SyscoSetup")
                                                ,TrimSetup = reader.GetAttribute("TrimSetup")
                                                ,WI = reader.GetAttribute("WI")
                                            });
                                            break;
                                        case "CSI":
                                            _tempList.Add(new AppGlobal
                                            {
                                                Site = "CSI"
                                                ,PartPrint = reader.GetAttribute("PartPrint")
                                                ,PressSetup = reader.GetAttribute("Setup")
                                                ,WI = reader.GetAttribute("WI")
                                            });
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                return _tempList;
            }
            catch(Exception)
            {
                return null;
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
