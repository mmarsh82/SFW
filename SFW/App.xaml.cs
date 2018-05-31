using Microsoft.Win32;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace SFW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        public static SqlConnection AppSqlCon { get; private set; }

        #endregion

        public App()
        {
            AppSqlCon = new SqlConnection("Server=SQL-WCCO;User ID=omni;Password=Public2017@WORK!;DataBase=WCCO_MAIN;Connection Timeout=5;MultipleActiveResultSets=True");
            AppSqlCon.OpenAsync();
            Current.Exit += App_Exit;
            AppDomain.CurrentDomain.UnhandledException += App_ExceptionCrash;
            Current.DispatcherUnhandledException += App_DispatherCrash;
            SystemEvents.PowerModeChanged += OnPowerChange;
            AppSqlCon.StateChange += SqlCon_StateChangeAsync;
        }

        /// <summary>
        /// Application On Startup method for running cmd input overrides
        /// </summary>
        /// <param name="e">start up events sent from the application.exe</param>
        protected override void OnStartup(StartupEventArgs e)
        {
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
                    //All start up command line arguments are to be added in the below switch statement as cases
                    switch (s.Remove(0, 1))
                    {
                        
                    }
                }
            }
        }

        /// <summary>
        /// MySQLConnection state change watch
        /// Will try 10 times to reconnect and if unsuccessful will terminate the connection
        /// </summary>
        /// <param name="sender">empty object</param>
        /// <param name="e">Connection State Change Events</param>
        private async static void SqlCon_StateChangeAsync(object sender, System.Data.StateChangeEventArgs e)
        {
            var count = 0;
            while ((AppSqlCon.State == System.Data.ConnectionState.Broken) && count <= 10)
            {
                await AppSqlCon.OpenAsync();
                count++;
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
                AppSqlCon.StateChange -= SqlCon_StateChangeAsync;
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
                    AppSqlCon.StateChange -= SqlCon_StateChangeAsync;
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
                    AppSqlCon.StateChange -= SqlCon_StateChangeAsync;
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
                    AppSqlCon.StateChange -= SqlCon_StateChangeAsync;
                    AppSqlCon.Close();
                    break;
                case PowerModes.Resume:
                    AppSqlCon.OpenAsync();
                    AppSqlCon.StateChange += SqlCon_StateChangeAsync;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool LoadGlobalAppConfig()
        {
            if (File.Exists("C:\\Users\\michaelm\\Desktop\\AppConfig.xml"))
            {
                var test = XDocument.Load("C:\\Users\\michaelm\\Desktop\\AppConfig.xml");
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
                    var xDoc = new XDocument();
                    var xWrite = xDoc.CreateWriter();
                    //TODO create a default config file and test against the true if condition
                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
