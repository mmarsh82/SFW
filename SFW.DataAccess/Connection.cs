using System.Data;
using Microsoft.Data.SqlClient;
using System.Security;

namespace SFW.DataAccess
{
    public class Connection : Base
    {
        /// <summary>
        /// Library main SQL Connection
        /// </summary>
        public static SqlConnection Sql { get; set; }

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
            Sql = new SqlConnection($"Server={server};DataBase={database};Connection Timeout={timeout};MultipleActiveResultSets=True;Connection Lifetime=3;Max Pool Size=3;Pooling=true;", sqlCred)
            {
                StatisticsEnabled = true
            };
            Sql.Open();
            while (Sql.State != ConnectionState.Open) { }
            Sql.StateChange += SqlCon_StateChange;
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
            while ((Sql.State == ConnectionState.Broken || Sql.State == ConnectionState.Closed) && count <= 5)
            {
                if (!string.IsNullOrEmpty(Sql.ConnectionString))
                {
                    Sql.Open();
                }
                else
                {
                    break;
                }
            }
        }
    }
}
