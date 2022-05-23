using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace SFW
{
    public class UserConfig : INotifyPropertyChanged
    {
        #region Properties

        private int _siteNbr;
        public int SiteNumber 
        {
            get { return _siteNbr; }
            set { _siteNbr = value; OnPropertyChanged(nameof(SiteNumber)); }
        }

        private int? _pos;
        public int? Position
        {
            get { return _pos; }
            set { _pos = value; OnPropertyChanged(nameof(Position)); }
        }

        private string _machNbr;
        public string MachineNumber
        {
            get { return _machNbr; }
            set { _machNbr = value; OnPropertyChanged(nameof(MachineNumber)); }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UserConfig()
        { }

        /// <summary>
        /// Get a list of the user XML config file
        /// </summary>
        public static List<UserConfig> GetUserConfigList()
        {
            var _uConf = new List<UserConfig>();
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!File.Exists($"{folder}\\SFW\\SfwConfig.xml"))
                {
                    CreateNewConfigFile();
                }
                using (var rStream = new FileStream($"{folder}\\SFW\\SfwConfig.xml", FileMode.Open))
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
                                    if (reader.Name.Contains("Site"))
                                    {
                                        var _site = Convert.ToInt32(reader.Name.Substring(reader.Name.Length - 1));
                                        _uConf.Add(new UserConfig { SiteNumber = _site, MachineNumber = reader.GetAttribute("WC_Nbr"), Position = Convert.ToInt32(reader.GetAttribute("Position")) });
                                    }
                                    else if (reader.Name == "Default_View")
                                    {
                                        App.IsFocused = bool.TryParse(reader.GetAttribute("Focus").ToString(), out bool b) && b;
                                    }
                                }
                            }
                        }
                    }
                }
                return _uConf;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Create a new user config file for the SFW application 
        /// </summary>
        public static void CreateNewConfigFile()
        {
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory($"{folder}\\SFW");
                using (var wStream = new FileStream($"{folder}\\SFW\\SfwConfig.xml", FileMode.CreateNew))
                {
                    var wSettings = new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = true };
                    using (var writer = XmlWriter.Create(wStream, wSettings))
                    {
                        writer.WriteStartElement("SFW_User_Config");

                        writer.WriteComment("Default View");
                        writer.WriteComment("Defines how the schedule is going to show work orders");
                        writer.WriteComment("true will only show approved, false will show all work orders");

                        writer.WriteStartElement("Default_View");
                        writer.WriteAttributeString("Focus", "false");
                        writer.WriteEndElement();

                        writer.WriteComment("Default Work Centers");
                        writer.WriteComment("Work center name and schedule position seperated by Site number");

                        writer.WriteStartElement("Default_WC");

                        writer.WriteStartElement("Site_0");
                        writer.WriteAttributeString("WC_Nbr", "");
                        writer.WriteAttributeString("Position", "1");
                        writer.WriteEndElement();

                        writer.WriteStartElement("Site_1");
                        writer.WriteAttributeString("WC_Nbr", "");
                        writer.WriteAttributeString("Position", "1");
                        writer.WriteEndElement();

                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Update the user config file for the SFW application 
        /// </summary>
        /// <param name="ucList">List of stored userconfig objects</param>
        /// <param name="focus">Set the focus view value</param>
        public static void UpdateConfigFile(List<UserConfig> ucList, bool focus)
        {
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory($"{folder}\\SFW");
                using (var wStream = new FileStream($"{folder}\\SFW\\SfwConfig.xml", FileMode.Create))
                {
                    var wSettings = new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = true };
                    using (var writer = XmlWriter.Create(wStream, wSettings))
                    {
                        writer.WriteStartElement("SFW_User_Config");

                        writer.WriteComment("Default View");
                        writer.WriteComment("Defines how the schedule is going to show work orders");
                        writer.WriteComment("true will only show approved, false will show all work orders");

                        writer.WriteStartElement("Default_View");
                        writer.WriteAttributeString("Focus", focus.ToString());
                        writer.WriteEndElement();

                        writer.WriteComment("Default Work Centers");
                        writer.WriteComment("Work center name and schedule position seperated by Site number");

                        writer.WriteStartElement("Default_WC");

                        if (ucList != null && ucList.Count(o => o.SiteNumber == 0) > 0)
                        {
                            foreach (UserConfig _uc in ucList.Where(o => o.SiteNumber == 0).OrderBy(o => o.Position))
                            {
                                writer.WriteStartElement("Site_0");
                                writer.WriteAttributeString("WC_Nbr", _uc.MachineNumber);
                                writer.WriteAttributeString("Position", _uc.Position.ToString());
                                writer.WriteEndElement();
                            }
                        }
                        else
                        {
                            writer.WriteStartElement("Site_0");
                            writer.WriteAttributeString("WC_Nbr", "");
                            writer.WriteAttributeString("Position", "1");
                            writer.WriteEndElement();
                        }

                        if (ucList != null && ucList.Count(o => o.SiteNumber == 1) > 0)
                        {
                            foreach (UserConfig _uc in ucList.Where(o => o.SiteNumber == 1).OrderBy(o => o.Position))
                            {
                                writer.WriteStartElement("Site_1");
                                writer.WriteAttributeString("WC_Nbr", _uc.MachineNumber);
                                writer.WriteAttributeString("Position", _uc.Position.ToString());
                                writer.WriteEndElement();
                            }
                        }
                        else
                        {
                            writer.WriteStartElement("Site_1");
                            writer.WriteAttributeString("WC_Nbr", "");
                            writer.WriteAttributeString("Position", "1");
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Get the values for the user config object
        /// </summary>
        /// <returns>Dictionary of user config values</returns>
        public static IReadOnlyDictionary<string, int> GetIROD()
        {
            var _irod = new Dictionary<string, int>();
            if (App.DefualtWorkCenter != null)
            {
                if (App.DefualtWorkCenter.Count(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)) > 0)
                {
                    foreach (var v in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber) && o.Position != null))
                    {
                        _irod.Add(v.MachineNumber, int.TryParse(v.Position.ToString(), out int r) ? r : 0);
                    }
                }
            }
            return _irod;
        }

        /// <summary>
        /// Get the values for the user config object
        /// </summary>
        /// <returns>Dictionary of user config values</returns>
        public static Dictionary<string, int> GetDict()
        {
            var _dict = new Dictionary<string, int>();
            if (App.DefualtWorkCenter != null)
            {
                if (App.DefualtWorkCenter.Count(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)) > 0)
                {
                    foreach (var v in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber) && o.Position != null))
                    {
                        _dict.Add(v.MachineNumber, int.TryParse(v.Position.ToString(), out int r) ? r : 0);
                    }
                }
            }
            return _dict;
        }

        /// <summary>
        /// Builds the original filter for the Schedule view based on the application user config file for machine selection
        /// </summary>
        /// <returns>DataTable filter string</returns>
        public static string BuildMachineFilter()
        {
            var _filter = string.Empty;
            if (App.DefualtWorkCenter?.Count(o => o.SiteNumber == App.SiteNumber) == 1 && !string.IsNullOrEmpty(App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber))
            {
                _filter = $@"MachineNumber = '{App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber}'";
            }
            else if (App.DefualtWorkCenter?.Count(o => o.SiteNumber == App.SiteNumber) > 1)
            {
                foreach (var m in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber))
                {
                    _filter += string.IsNullOrEmpty(_filter) ? $"(MachineNumber = '{m.MachineNumber}'" : $" OR MachineNumber = '{m.MachineNumber}'";
                }
                _filter += ")";
            }
            return _filter;
        }

        /// <summary>
        /// Builds the original filter for the Schedule view based on the application user config file for work order priority
        /// </summary>
        /// <returns>DataTable filter string</returns>
        public static string BuildPriorityFilter()
        {
            var _filter = string.Empty;
            if (App.IsFocused && string.IsNullOrEmpty(_filter))
            {
                _filter = "WO_Priority = 'A' OR WO_Priority = 'B'";
            }
            else if (App.IsFocused)
            {
                _filter += " AND (WO_Priority = 'A' OR WO_Priority = 'B')";
            }
            return _filter;
        }
    }
}
