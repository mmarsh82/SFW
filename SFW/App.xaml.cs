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
using System.Xml.Linq;

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

        public static SqlConnection AppSqlCon { get; set; }
        public static M2kConnection ErpCon { get; set; }

        public static List<UConfig> DefualtWorkCenter { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static ISplashScreen splashScreen;

        private ManualResetEvent ResetSplashCreated;
        private Thread SplashThread;

        #endregion

        public App()
        {
            //LoadGlobalAppConfig();
            DefualtWorkCenter = LoadUserAppConfig();
            Site = "CSI_MAIN";
            SiteNumber = 0;
            ErpCon = new M2kConnection("172.16.0.10", "omniquery", "omniquery", Database.CSI);
            AppSqlCon = new SqlConnection($"Server=172.16.0.114;User ID=omni;Password=Public2017@WORK!;DataBase={Site};Connection Timeout=60;MultipleActiveResultSets=True");
            AppSqlCon.Open();
            while (AppSqlCon.State != System.Data.ConnectionState.Open) { }
            Current.Exit += App_Exit;
            AppDomain.CurrentDomain.UnhandledException += App_ExceptionCrash;
            Current.DispatcherUnhandledException += App_DispatherCrash;
            SystemEvents.PowerModeChanged += OnPowerChange;
            AppSqlCon.StateChange += SqlCon_StateChange;
            RefreshTimer.Start(new TimeSpan(0, 5, 0));
            if (!CurrentUser.IsLoggedIn)
            {
                CurrentUser.LogIn();
            }
        }

        /// <summary>
        /// Application On Startup method for running cmd input overrides
        /// </summary>
        /// <param name="e">start up events sent from the application.exe</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            ResetSplashCreated = new ManualResetEvent(false);
            SplashThread = new Thread(ShowSplash);
            SplashThread.SetApartmentState(ApartmentState.STA);
            SplashThread.IsBackground = true;
            SplashThread.Name = "Splash Screen";
            SplashThread.Start();

            ResetSplashCreated.WaitOne();
            base.OnStartup(e);

            string[] startUpArgs = null;
            try
            {
                startUpArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData ?? null;
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
        private bool LoadGlobalAppConfig()
        {
            if (File.Exists("C:\\Users\\michaelm\\Desktop\\SfwConfig.xml"))
            {
                var test = XDocument.Load("C:\\Users\\michaelm\\Desktop\\SfwConfig.xml");
                var test2 = test.Descendants();
                foreach (var x in test2)
                {
                    var test3 = x.Value;
                }
                return true;
            }
            else
            {
                try
                {
                    XDocument xDoc =
                        new XDocument(
                            new XElement("SFWConfig",
                                new XElement("M2kConnection",
                                    new XElement("ManageHostName", 
                                        new XAttribute("Name", "manage"),
                                        new XAttribute("IP", "172.16.0.122")
                                    ),
                                    new XElement("ServiceAccount",
                                        new XAttribute("UserID", "omniquery"), 
                                        new XAttribute("Password", "omniquery")
                                    )
                                ),
                                new XElement("RefreshRate",
                                    new XElement("TimeSpan",
                                        new XAttribute("Hours", "0"),
                                        new XAttribute("Minutes", "5"),
                                        new XAttribute("Second", "0"),
                                        new XAttribute("MilliSeconds", "0")
                                    )
                                ),
                                new XElement("SQLConnection",
                                    new XElement("Server",
                                        new XAttribute("Name", "SQL-WCCO"),
                                        new XAttribute("IP", "172.16.0.114")
                                    ),
                                    new XElement("ServiceAccount",
                                        new XAttribute("UserID", "omni"),
                                        new XAttribute("Password", "Public2017@WORK!")
                                    ),
                                    new XElement("TimeOut",
                                        new XAttribute("Seconds", "60")
                                    )
                                ),
                                new XElement("PartDocuments",
                                    new XElement("FilePath",
                                        new XElement("Print",
                                            new XElement("Part",
                                                new XAttribute("CADPart", "\\\\manage2\\server\\Engineering\\Product\\Prints\\Controlled Production Prints\\"),
                                                new XAttribute("SlatPart", "\\\\manage2\\server\\Engineering\\Product\\Prints\\R SLAT MASTER PRINT.xlsx"),
                                                new XAttribute("ExtPart", "\\\\manage2\\server\\Engineering\\Product\\Prints\\R EXT MASTER PRINT.xlsx")
                                            ),
                                            new XElement("SetUp",
                                                new XAttribute("Press", "\\\\manage2\\server\\Engineering\\Product\\Press Setups\\press setup and part number crossreference.xlsm"),
                                                new XAttribute("Sysco", ""),
                                                new XAttribute("Trimming", "")
                                            )
                                        ),
                                        new XElement("Process",
                                            new XAttribute("WIorSOP", "\\\\manage2\\server\\Document Center\\Production\\")
                                        )
                                    )
                                ),
                                new XElement("LDAP",
                                    new XAttribute("Administrator", "SFW_Admin"),
                                    new XAttribute("Scheduler", "SFW_Sched"),
                                    new XAttribute("QTask", "SFW_QTask"),
                                    new XAttribute("Supervisor", "SFW_Super"),
                                    new XAttribute("Inventory Control", "SFW_IC"),
                                    new XAttribute("Quality", "SFW_QC"),
                                    new XAttribute("Deviations", "SFW_Deviate")
                                ),
                                new XElement("Shifts",
                                    new XElement("1",
                                        new XAttribute("Start", "7:00"),
                                        new XAttribute("End", "14:59")),
                                    new XElement("2",
                                        new XAttribute("Start","15:00"),
                                        new XAttribute("End", "22:59")),
                                    new XElement("3",
                                        new XAttribute("Start", "23:00"),
                                        new XAttribute("End", "6:59"))
                                )
                            )
                        );
                    xDoc.Save("C:\\Users\\michaelm\\Desktop\\SfwConfig.xml");
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
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

    }
}
