using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace QT9Client
{
    public class Class1
    {
        public Class1()
        {
        }

        public static string Execute()
        {
            try
            {
                var userName = "qt9sa";
                var pass = "4WCKxqkFVn26bjaj";
                //var docId = 9;

                var _testReq = WebRequest.Create("https://wccobelt.qt9app1.com/services/wsDocuments.asmx");
                _testReq.Headers.Add(@"SOAPAction:QT9.QMS.WebD.WS/GetAllDocumentsAsDataSet");
                _testReq.ContentType = "text/xml;charset=\"utf-8\"";
                _testReq.Method = "POST";
                var _testXDoc = new XmlDocument();
                _testXDoc.LoadXml($@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                      <soap:Header>
                                        <wsAuthenticator xmlns = ""QT9.QMS.WebD.WS"">
                                          <UserName>{userName}</UserName>
                                          <Password>{pass}</Password>
                                        </wsAuthenticator>
                                      </soap:Header>
                                      <soap:Body>
                                        <GetAllDocumentsAsDataSet xmlns=""QT9.QMS.WebD.WS"">
                                          <IncludeInactive>0</IncludeInactive>
                                        </GetAllDocumentsAsDataSet>
                                      </soap:Body>
                                    </soap:Envelope>");
                using (Stream _stream = _testReq.GetRequestStream())
                {
                    _testXDoc.Save(_stream);
                }
                using (WebResponse Serviceres = _testReq.GetResponse())
                {
                    using (Stream str = Serviceres.GetResponseStream())
                    {
                        try
                        {
                            var testDs = new System.Data.DataSet();
                            testDs.ReadXml(str, System.Data.XmlReadMode.ReadSchema);
                        }
                        catch (System.Exception ex)
                        {
                            var testing = ex.Message;
                        }
                    }
                    return string.Empty;
                    /*using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                    {
                        var ServiceResult = rd.ReadToEnd();
                        return ServiceResult;
                    }*/
                }
            }
            catch (WebException wex)
            {
                string pageContent = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd().ToString();
                return pageContent;
            }
            catch (Exception)
            {
                return null;
            }
            
        }
    }
}
