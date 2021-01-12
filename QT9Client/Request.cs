using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QT9Client
{
    public class Request
    {
        #region Properties

        public static XmlDocument RequestDocument { get; set; }

        #endregion

        public Request(string userName, string password, string site, Enum service)
        {

        }
    }
}
