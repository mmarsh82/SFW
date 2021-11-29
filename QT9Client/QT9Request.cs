using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QT9Client
{
    public class QT9Request
    {
        #region Properties

        /// <summary>
        /// QT9 WebRequest object
        /// </summary>
        public static WebRequest QT9WebRequest { get; set; }

        /// <summary>
        /// QT9 XmlDocument object
        /// </summary>
        public static XmlDocument QT9XmlDocument { get; set; }

        /// <summary>
        /// QT9 web module
        /// </summary>
        public static Enum Module { get; set; }

        #endregion

        public QT9Request()
        { }

        /// <summary>
        /// Create a QT9 web request
        /// </summary>
        /// <param name="service">WSDL header function that will be used to POST/GET data</param>
        /// <param name="reqHeader">Module with in the WSDL header that will be used to POST/GET data</param>
        public static void Create(QT9Services service, Enum reqHeader, QT9Connection qCon)
        {
            Module = reqHeader;
            QT9WebRequest = WebRequest.Create($"{qCon.WebUrl}/services/{service}.asmx");
            QT9WebRequest.ContentType = "text/xml;charset=\"utf-8\"";
            QT9WebRequest.Method = "POST";
            QT9WebRequest.Headers.Add($"SOAPAction:QT9.QMS.WebD.WS/{Module}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qCon"></param>
        /// <returns></returns>
        public static object GetResponse(QT9Connection qCon, params object[] parameters)
        {
            try
            {
                QT9XmlDocument = BuildDocument(parameters, qCon);
                using (Stream _xmlStream = QT9WebRequest.GetRequestStream())
                {
                    QT9XmlDocument.Save(_xmlStream);
                }
                using (WebResponse _response = QT9WebRequest.GetResponse())
                {
                    using (Stream _responseStream = _response.GetResponseStream())
                    {
                        if (Module.ToString().Contains("Add"))
                        {
                            //TODO: handle the add key word and the response
                            return null;
                        }
                        else if (Module.ToString().Contains("DataSet"))
                        {
                            using (DataSet _dataSet = new DataSet())
                            {
                                _dataSet.ReadXml(_responseStream, XmlReadMode.ReadSchema);
                                return _dataSet;
                            }
                        }
                        else
                        {
                            //TODO: handle all other types of responses
                            return null;
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                string pageContent = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd().ToString();
                return $"ERROR:{pageContent}";
            }
            catch (Exception ex)
            {
                return $"ERROR:{ex.Message}";
            }
        }

        /// <summary>
        /// Build the QT9 XmlDocument object
        /// </summary>
        /// <param name="qCon">QT9 Connection to use</param>
        /// <returns>Populated QT9XmlDocument object</returns>
        public static XmlDocument BuildDocument(object[] parameters, QT9Connection qCon)
        {
            var _rtnDoc = new XmlDocument();
            _rtnDoc.LoadXml($@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        {QT9Header(qCon)}
                                        {QT9Body(parameters)}
                                    </soap:Envelope>");
            return _rtnDoc;
        }

        /// <summary>
        /// Build the header element of the QT9 XmlDocument
        /// </summary>
        /// <param name="qCon">QT9 Connection to use</param>
        /// <returns>Xml Header for a web request as a string</returns>
        public static string QT9Header(QT9Connection qCon)
        {
            try
            {
                return $@"<soap:Header>
                            <wsAuthenticator xmlns = ""QT9.QMS.WebD.WS"">
                                <UserName>{qCon.UserName}</UserName>
                                <Password>{qCon.Password}</Password>
                            </wsAuthenticator>
                          </soap:Header>";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Build the body element of the QT9 XmlDocument
        /// </summary>
        /// <param name="parameters">Any parameter that will need to be added to populate values in the body</param>
        /// <returns>Xml body for a web request as a string</returns>
        public static string QT9Body(object[] parameters)
        {
            try
            {
                if (Module != null)
                {
                    var _body = Module.GetDescription();
                    var _counter = 1;
                    while (_body.Contains($"@p{_counter}"))
                    {
                        _counter++;
                    }
                    _counter--;
                    if (parameters.Length != _counter && parameters.Length != 0)
                    {
                        return string.Empty;
                    }
                    var _fillCount = 1;
                    while (_fillCount <= _counter && parameters.Length != 0)
                    {
                        var _listString = string.Empty;
                        if (parameters.GetType() == typeof(List<string>))
                        {
                            var _list = (List<string>)parameters[_fillCount];
                            foreach (var _string in _list)
                            {
                                _listString += $"<{_string}/>";
                            }
                        }
                        _body = _body.Replace($"@p{_fillCount}", string.IsNullOrEmpty(_listString) ? parameters[_fillCount - 1].ToString() : _listString);
                        _fillCount++;
                    }
                    return _body;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
