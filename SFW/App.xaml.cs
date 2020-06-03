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

        //Hardcoded application location will need to be changed based on actual file path
        public static string AppFilePath { get { return "\\\\fs-wcco\\WCCO-SFW\\ShopFloorWorkbench\\"; } }

        public static IList<AppGlobal> GlobalConfig { get; set; }

        public static SqlConnection AppSqlCon { get; set; }
        public static M2kConnection ErpCon { get; set; }

        public static List<UConfig> DefualtWorkCenter { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static ISplashScreen splashScreen;

        private readonly ManualResetEvent ResetSplashCreated;
        private readonly Thread SplashThread;

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
            //Loading application
            Site = "CSI_MAIN";
            SiteNumber = 0;
            GlobalConfig = LoadGlobalAppConfig();
            DefualtWorkCenter = LoadUserAppConfig();
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
            if (!CurrentUser.IsLoggedIn)
            {
                CurrentUser.LogIn();
            }
            var _folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists($"{_folder}\\SFW\\Labor\\"))
            {
                Directory.CreateDirectory($"{_folder}\\SFW\\Labor\\");
            }
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

                            writer.WriteComment("ERP connection parameters");
                            writer.WriteStartElement("M2kConnection");
                            writer.WriteAttributeString("Name", "ERP-WCCO");
                            writer.WriteAttributeString("IP", "172.16.0.10");
                            writer.WriteAttributeString("ServiceUser", "omniquery");
                            writer.WriteAttributeString("ServicePass", "omniquery");
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
                            writer.WriteAttributeString("Name", "SQL-WCCO");
                            writer.WriteAttributeString("IP", "172.16.0.114");
                            writer.WriteAttributeString("ServiceUser", "omni");
                            writer.WriteAttributeString("ServicePass", "Public2017@WORK!");
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
                            writer.WriteAttributeString("PartPrint", "\\\\fs-wcco\\WCCO-Prints\\");
                            writer.WriteAttributeString("PressSetup", "\\\\fs-wcco\\WCCO-Engineering\\Product\\Press Setups\\press setup and part number crossreference.xlsm");
                            writer.WriteAttributeString("SyscoSetup", "\\\\fs-wcco\\WCCO-Engineering\\Product\\Sysco Press Setups\\SYSCO PRESS - Setup cross reference.xlsx");
                            writer.WriteAttributeString("TrimSetup", "\\\\fs-wcco\\WCCO-Engineering\\Product\\Trimmin Info\\SFW_Trimming Info.xlsx");
                            writer.WriteAttributeString("WI", "\\\\fs-wcco\\WCCO-PublishedDocuments\\");
                            writer.WriteEndElement();

                            writer.WriteStartElement("CSI");
                            writer.WriteAttributeString("PartPrint", "\\\\fs-csi\\CSI-Prints\\part\\");
                            writer.WriteAttributeString("Setup", "\\\\fs-csi\\CSI-Prints\\setup\\");
                            writer.WriteAttributeString("WI", "\\\\fs-csi\\prints\\WI\\");
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
                                                                            MultipleActiveResultSets=True");
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
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Load the user XML config file into a list of objects
        /// </summary>
        private List<UConfig> LoadUserAppConfig()
        {
            var _uConf = new List<UConfig>();
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!File.Exists($"{folder}\\SFW\\SfwConfig.xml"))
                {
                    Directory.CreateDirectory($"{folder}\\SFW");
                    using (var wStream = new FileStream($"{folder}\\SFW\\SfwConfig.xml", FileMode.CreateNew))
                    {
                        var wSettings = new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = true };
                        using (var writer = XmlWriter.Create(wStream, wSettings))
                        {
                            writer.WriteComment("Default Work Centers");
                            writer.WriteComment("Work center name and schedule position seperated by Site number" );

                            writer.WriteStartElement("Default_WC");

                            writer.WriteStartElement("Site_0");

                            writer.WriteAttributeString("WC_Nbr", "");
                            writer.WriteAttributeString("Position", "1");

                            writer.WriteEndElement();

                            writer.WriteStartElement("Site_1");

                            writer.WriteAttributeString("WC_Nbr", "");
                            writer.WriteAttributeString("Position", "1");

                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }
                    }
                }
                using (var rStream = new FileStream($"{folder}\\SFW\\SfwConfig.xml", FileMode.Open))
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
                                    var _site = Convert.ToInt32(reader.Name.Substring(reader.Name.Length - 1));
                                    _uConf.Add(new UConfig { SiteNumber = _site, MachineNumber = reader.GetAttribute("WC_Nbr"), Position = Convert.ToInt32(reader.GetAttribute("Position")) });
                                }
                            }
                        }
                    }
                }
                return _uConf;
            }
            catch(Exception ex)
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
    }
}
