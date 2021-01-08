using System.IO;
using System.Net;
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
            var userName = "michael.marsh";
            var pass = "Hard-to-remember-NOW!";
            var docId = 9;

            var _testReq = WebRequest.Create("https://wccobelt.qt9app1.com/services/wsDocuments.asmx");
            _testReq.Headers.Add(@"SOAPAction:QT9.QMS.WebD.WS/GetDocumentByID");
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
                                        <GetDocumentByID xmlns = ""QT9.QMS.WebD.WS"">
                                          <DocID>{docId}</DocID>
                                        </GetDocumentByID>
                                      </soap:Body>
                                    </soap:Envelope>");

            using (Stream stream = _testReq.GetRequestStream())
            {
                _testXDoc.Save(stream);
            }
            //Geting response from request    
            using (WebResponse Serviceres = _testReq.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    //reading stream    
                    var ServiceResult = rd.ReadToEnd();
                    //writting stream result on console
                    return ServiceResult;
                }
            }
        }
    }
}
