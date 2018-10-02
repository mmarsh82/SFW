namespace M2k
{
    public class M2kConnection
    {
        #region Properties

        /// <summary>
        /// Manage 2000 Host Name for a connection, generally an IP to the server or the server name in the DNS
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Manage 2000 certified username, best practice would be to use a service account
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Password for the Manage 2000 account you plan on using for the UniSession
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Manage 2000 database location, this can typically be found on the host server
        /// </summary>
        public string Database { get; private set; }
        public string Service { get { return "udcs"; } }

        #endregion

        /// <summary>
        /// Set up a connection to Manage 2000 using a Uni tunnel
        /// </summary>
        /// <param name="hostName">Manage 2000 Host Name for a connection, generally an IP to the server or the server name in the DNS</param>
        /// <param name="userName">Manage 2000 certified username, best practice would be to use a service account</param>
        /// <param name="password">Password for the Manage 2000 account you plan on using for the UniSession</param>
        /// <param name="database">Manage 2000 database location, this can typically be found on the host server</param>
        public M2kConnection(string hostName, string userName, string password, string database)
        {
            HostName = hostName;
            UserName = userName;
            Password = password;
            Database = database;

        }
    }
}
