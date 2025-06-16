using System.Data;
using System.Data.SqlClient;
using System.Security;

namespace SFW.DataAccess
{
    public class Connection : Base
    {
        /// <summary>
        /// Library main SQL Connection
        /// </summary>
        public static SqlConnection DataConnection { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Connection()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="user">Service account ID for the connection</param>
        /// <param name="password">Service account password for the connection</param>
        /// <param name="server">SQL server FQDN or IP address</param>
        /// <param name="database">DataBase to use</param>
        /// <param name="timeout">Timeout interval</param>
        public Connection(string user, SecureString password, string server, string database, string timeout)
        {
            var sqlCred = new SqlCredential(user, password);
            DataConnection = new SqlConnection($"Server={server};DataBase={database};Connection Timeout={timeout};MultipleActiveResultSets=True;Connection Lifetime=3;Max Pool Size=3;Pooling=true;", sqlCred);
            DataConnection.StatisticsEnabled = true;
            DataConnection.Open();
            while (DataConnection.State != ConnectionState.Open) { }
            DataConnection.StateChange += SqlCon_StateChange;
        }

        /// <summary>
        /// SQLConnection state change watch
        /// Will try 10 times to reconnect and if unsuccessful will terminate the connection
        /// </summary>
        /// <param name="sender">empty object</param>
        /// <param name="e">Connection State Change Events</param>
        private static void SqlCon_StateChange(object sender, StateChangeEventArgs e)
        {
            var count = 0;
            while ((DataConnection.State == ConnectionState.Broken || DataConnection.State == ConnectionState.Closed) && count <= 5)
            {
                if (!string.IsNullOrEmpty(DataConnection.ConnectionString))
                {
                    DataConnection.Open();
                }
                else
                {
                    break;
                }
            }
        }
    }
}
