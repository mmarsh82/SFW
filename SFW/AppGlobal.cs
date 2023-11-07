using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;

namespace SFW
{
    public class AppGlobal
    {
        #region Properties

        public string Site { get; set; }
        public string PartPrint { get; set; }
        public string PressSetup { get; set; }
        public string SyscoSetup { get; set; }
        public string TrimSetup { get; set; }
        public string ExtSetup { get; set; }
        public string WI { get; set; }
        public string WorkOrderWeb { get; set; }
        public string SalesOrderWeb { get; set; }
        public static string ConfigFilePath { get; set; }

        public static string _zLock;
        public static string ZoneLock
        {
            get { return _zLock; }
            set
            {
                _zLock = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ZoneLock)));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppGlobal()
        { }

        /// <summary>
        /// Check to see if a global config exists
        /// </summary>
        /// <param name="filePath">File path to the XML config file</param>
        /// <returns>Pass/fail as true/false</returns>
        public static bool Exists(string filePath)
        {
            try
            {
                return File.Exists($"{filePath}");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create a new global config
        /// </summary>
        /// <param name="filePath">File path to create the new XML config file</param>
        /// <returns>Dictionary with pass/fail as key and any messages as value</returns>
        public static IDictionary<bool,string> Create(string filePath)
        {
            var _rDict = new Dictionary<bool, string>();
            ConfigFilePath = filePath;
            try
            {
                using (var wStream = new FileStream($"{filePath}", FileMode.CreateNew))
                {
                    var wSettings = new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = true };
                    using (var writer = XmlWriter.Create(wStream, wSettings))
                    {
                        writer.WriteComment("SFW Global Config File");
                        writer.WriteStartElement("GlobalConfig");
                        writer.WriteAttributeString("Version", "2.0");

                        writer.WriteComment("ERP connection parameters");
                        writer.WriteStartElement("M2kConnection");
                        writer.WriteAttributeString("Name", "");
                        writer.WriteAttributeString("IP", "");
                        writer.WriteAttributeString("ServiceUser", "");
                        writer.WriteAttributeString("ServicePass", "");
                        writer.WriteEndElement();

                        writer.WriteComment("SFW Automatic Refresh rate");
                        writer.WriteStartElement("RefreshRate");
                        writer.WriteAttributeString("Hours", "0");
                        writer.WriteAttributeString("Minutes", "5");
                        writer.WriteAttributeString("Seconds", "0");
                        writer.WriteAttributeString("MilliSeconds", "0");
                        writer.WriteEndElement();

                        writer.WriteComment("SQL connection parameters");
                        writer.WriteComment("Connection Timeout is in seconds");
                        writer.WriteStartElement("SqlConnection");
                        writer.WriteAttributeString("Name", "");
                        writer.WriteAttributeString("IP", "");
                        writer.WriteAttributeString("ServiceUser", "");
                        writer.WriteAttributeString("ServicePass", "");
                        writer.WriteAttributeString("TimeOut", "60");
                        writer.WriteEndElement();

                        writer.WriteComment("Production Work Enviroment");
                        writer.WriteStartElement("Shifts");
                        writer.WriteComment("All shifts must be in a military time format");
                        writer.WriteComment("Shift total must not exceed a 24 hour day");
                        writer.WriteStartElement("First");
                        writer.WriteAttributeString("Start", "07:00");
                        writer.WriteAttributeString("End", "14:59");
                        writer.WriteEndElement();
                        writer.WriteStartElement("Second");
                        writer.WriteAttributeString("Start", "15:00");
                        writer.WriteAttributeString("End", "22:59");
                        writer.WriteEndElement();
                        writer.WriteStartElement("Third");
                        writer.WriteAttributeString("Start", "23:00");
                        writer.WriteAttributeString("End", "06:59");
                        writer.WriteEndElement();
                        writer.WriteEndElement();

                        writer.WriteComment("Supporting document file paths");
                        writer.WriteStartElement("HelpFile");
                        writer.WriteAttributeString("Path", "");
                        writer.WriteEndElement();

                        writer.WriteComment("All Site documentation file paths must be contained below");
                        writer.WriteComment("If you are incorparating a new file path you will need to contact the developer to add in the corresponding properties");
                        writer.WriteStartElement("SiteDocumentation");

                        writer.WriteStartElement("WCCO");
                        writer.WriteAttributeString("PartPrint", "");
                        writer.WriteAttributeString("PressSetup", "");
                        writer.WriteAttributeString("SyscoSetup", "");
                        writer.WriteAttributeString("TrimSetup", "");
                        writer.WriteAttributeString("ExtruderSetup", "");
                        writer.WriteAttributeString("WI", "");
                        writer.WriteEndElement();

                        writer.WriteStartElement("CSI");
                        writer.WriteAttributeString("PartPrint", "");
                        writer.WriteAttributeString("Setup", "");
                        writer.WriteAttributeString("WI", "");
                        writer.WriteEndElement();

                        writer.WriteEndElement();

                        writer.WriteComment("Application locks for business inventory control processes");
                        writer.WriteStartElement("ApplicationLocks");
                        writer.WriteAttributeString("Zone", "");
                        writer.WriteAttributeString("System", "0");
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                }
                _rDict.Add(true, string.Empty);
                return _rDict;
            }
            catch (Exception ex)
            {
                _rDict.Add(false, ex.Message);
                return _rDict;
            }
        }

        /// <summary>
        /// Loads a list of the global config file objects
        /// </summary>
        /// <returns>Config file existance or creation</returns>
        public static IList<AppGlobal> Load(string filePath)
        {
            try
            {
                ConfigFilePath = filePath;
                if (!Exists(filePath))
                {
                    Create(filePath);
                }
                var _tempList = new List<AppGlobal>();
                using (var rStream = new FileStream(filePath, FileMode.Open))
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
                                    switch (reader.Name)
                                    {
                                        case "SFWApp":
                                            App.AppLock = bool.TryParse(reader.GetAttribute("IsLocked"), out bool b) ? b : true;
                                            break;
                                        case "M2kConnection":
                                            App.ErpCon = new M2kClient.M2kConnection(reader.GetAttribute("Name"), reader.GetAttribute("ServiceUser"), reader.GetAttribute("ServicePass"), M2kClient.Database.CONTI, App.SiteNumber);
                                            break;
                                        case "RefreshRate":
                                            var _hour = int.TryParse(reader.GetAttribute("Hours"), out int h) ? h : 0;
                                            var _min = int.TryParse(reader.GetAttribute("Minutes"), out int m) ? m : 5;
                                            var _sec = int.TryParse(reader.GetAttribute("Seconds"), out int s) ? s : 0;
                                            var _mSec = int.TryParse(reader.GetAttribute("MilliSeconds"), out int mls) ? mls : 0;
                                            RefreshTimer.Start(new TimeSpan(0, _hour, _min, _sec, _mSec));
                                            break;
                                        case "SqlConnection":
                                            var pass = new SecureString();
                                            foreach (var c in reader.GetAttribute("ServicePass"))
                                            {
                                                pass.AppendChar(c);
                                            }
                                            pass.MakeReadOnly();
                                            var sqlCred = new SqlCredential(reader.GetAttribute("ServiceUser"), pass);
                                            App.AppSqlCon = new SqlConnection($"Server={reader.GetAttribute("IP")};DataBase={App.Site};Connection Timeout={reader.GetAttribute("TimeOut")};MultipleActiveResultSets=True;Connection Lifetime=3;Max Pool Size=3;Pooling=true;", sqlCred);
                                            App.AppSqlCon.StatisticsEnabled = true;
                                            break;
                                        //SiteDocumentation Element is written below
                                        //Make sure any site added in the SiteDocumentation element exists in the main application site list
                                        case "WCCO":
                                            _tempList.Add(new AppGlobal
                                            {
                                                Site = "WCCO"
                                                ,PartPrint = reader.GetAttribute("PartPrint")
                                                ,PressSetup = reader.GetAttribute("PressSetup")
                                                ,SyscoSetup = reader.GetAttribute("SyscoSetup")
                                                ,TrimSetup = reader.GetAttribute("TrimSetup")
                                                ,ExtSetup = reader.GetAttribute("ExtruderSetup")
                                                ,WI = reader.GetAttribute("WI")
                                            });
                                            break;
                                        case "CSI":
                                            _tempList.Add(new AppGlobal
                                            {
                                                Site = "CSI"
                                                ,PartPrint = reader.GetAttribute("PartPrint")
                                                ,PressSetup = reader.GetAttribute("Setup")
                                                ,WI = reader.GetAttribute("WI")
                                            });
                                            break;
                                        case "Locks":
                                            ZoneLock = reader.GetAttribute("Location");
                                            App.AppLock = int.TryParse(reader.GetAttribute("System"), out int _sys) ? _sys == 1 : false;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                return _tempList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Refreshes all properties of the global config
        /// </summary>
        public static void RefreshAll()
        {
            try
            {
                var _tempList = new List<AppGlobal>();
                using (var rStream = new FileStream(ConfigFilePath, FileMode.Open))
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
                                    switch (reader.Name)
                                    {
                                        case "RefreshRate":
                                            var _hour = int.TryParse(reader.GetAttribute("Hours"), out int h) ? h : 0;
                                            var _min = int.TryParse(reader.GetAttribute("Minutes"), out int m) ? m : 5;
                                            var _sec = int.TryParse(reader.GetAttribute("Seconds"), out int s) ? s : 0;
                                            var _mSec = int.TryParse(reader.GetAttribute("MilliSeconds"), out int mls) ? mls : 0;
                                            RefreshTimer.Start(new TimeSpan(0, _hour, _min, _sec, _mSec));
                                            break;
                                        case "SqlConnection":
                                            var pass = new SecureString();
                                            foreach (var c in reader.GetAttribute("ServicePass"))
                                            {
                                                pass.AppendChar(c);
                                            }
                                            pass.MakeReadOnly();
                                            var sqlCred = new SqlCredential(reader.GetAttribute("ServiceUser"), pass);
                                            App.AppSqlCon = new SqlConnection($"Server={reader.GetAttribute("IP")};DataBase={App.Site};Connection Timeout={reader.GetAttribute("TimeOut")};MultipleActiveResultSets=True;Connection Lifetime=3;Max Pool Size=3;Pooling=true;", sqlCred);
                                            App.AppSqlCon.StatisticsEnabled = true;
                                            break;
                                        //SiteDocumentation Element is written below
                                        //Make sure any site added in the SiteDocumentation element exists in the main application site list
                                        case "WCCO":
                                            _tempList.Add(new AppGlobal
                                            {
                                                Site = "WCCO"
                                                ,PartPrint = reader.GetAttribute("PartPrint")
                                                ,PressSetup = reader.GetAttribute("PressSetup")
                                                ,SyscoSetup = reader.GetAttribute("SyscoSetup")
                                                ,TrimSetup = reader.GetAttribute("TrimSetup")
                                                ,ExtSetup = reader.GetAttribute("ExtruderSetup")
                                                ,WI = reader.GetAttribute("WI")
                                            });
                                            break;
                                        case "CSI":
                                            _tempList.Add(new AppGlobal
                                            {
                                                Site = "CSI"
                                                ,PartPrint = reader.GetAttribute("PartPrint")
                                                ,PressSetup = reader.GetAttribute("Setup")
                                                ,WI = reader.GetAttribute("WI")
                                            });
                                            break;
                                        case "ApplicationLocks":
                                            ZoneLock =reader.GetAttribute("Location");
                                            App.AppLock = int.TryParse(reader.GetAttribute("System"), out int _sys) ? _sys == 1 : false;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Refreshes the locks portion of the global config
        /// </summary>
        public static void RefreshLocks()
        {
            try
            {
                var _tempList = new List<AppGlobal>();
                using (var rStream = new FileStream(ConfigFilePath, FileMode.Open))
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
                                    switch (reader.Name)
                                    {
                                        case "ApplicationLocks":
                                            ZoneLock = reader.GetAttribute("Location");
                                            App.AppLock = int.TryParse(reader.GetAttribute("System"), out int _sys) ? _sys == 1 : false;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Update an attribute value in the global config
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <param name="attributeName">Name of the attribute you are changing</param>
        /// <param name="newValue">New value for the attribute</param>
        /// <returns>Pass - fail check on return of true or false</returns>
        public static bool UpdateAttributeValue(string elementName, string attributeName, string newValue)
        {
            try
            {
                XDocument _configDoc = XDocument.Load(ConfigFilePath);
                var _attribute = _configDoc.Elements("GlobalConfig").Elements(elementName).Attributes().Where(o => o.Name == attributeName).FirstOrDefault();
                if (_attribute != null)
                {
                    _attribute.Value = newValue;
                    _configDoc.Save(ConfigFilePath);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
