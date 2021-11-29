namespace QT9Client
{
    public class QT9Connection
    {
        #region Properties

        /// <summary>
        /// Web URL for your site that will be used when makeing SOAP queries
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// QT9 certified username, best practice would be to use a service account
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password for the QT9 account you plan on using for SOAP authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Site or facility database that will beused with in QT9
        /// </summary>
        public string Site { get; set; }

        #endregion

        /// <summary>
        /// Create a fresh connection for QT9's cloud based client
        /// Will be able to authenticate, and make all standard HTTP requests
        /// </summary>
        /// <param name="webUrl">Web URL for your site that will be used when makeing SOAP queries</param>
        /// <param name="userName">QT9 certified username, best practice would be to use a service account</param>
        /// <param name="pass">Password for the QT9 account you plan on using for SOAP authentication</param>
        /// <param name="site">Site or facility database that will beused with in QT9</param>
        public QT9Connection(string webUrl, string userName, string pass, string site)
        {
            WebUrl = webUrl;
            UserName = userName;
            Password = pass;
            Site = site;
        }
    }
}
