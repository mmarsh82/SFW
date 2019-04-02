using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using SFW.Model;
using System.Xml.Linq;

namespace SFW.UserConfig
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public XmlDocument xmlDoc { get; set; }
        private List<Machine> mList;
        public List<Machine> MachineList
        {
            get { return mList; }
            set { mList = value; OnPropertyChanged(nameof(MachineList)); }
        }
        private string sMach;
        public string SelectedMachine
        {
            get { return sMach; }
            set
            {
                if (sMach != value && sMach != null)
                {
                    XmlNode node = xmlDoc.SelectSingleNode("/Default_WC[@name='WC_Nbr']");
                    if (node != null)
                    {

                    }
                }
                sMach = value;
                OnPropertyChanged(nameof(SelectedMachine));
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewModel()
        {
            MachineList = MainWindowViewModel.MachineList;
            MachineList.Insert(0, new Machine { MachineName = "None" });
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (xmlDoc == null && File.Exists($"{folder}\\SFW\\SfwConfig.xml"))
            {
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
                                    if (MachineList.Exists(o => o.MachineNumber == reader.GetAttribute("WC_Nbr")))
                                    {
                                        SelectedMachine = MachineList.FirstOrDefault(o => o.MachineNumber == reader.GetAttribute("WC_Nbr")).MachineName;
                                    }
                                }
                            }
                        }
                    }
                }
                //TODO: move this to the set property so that you can update the values in the XML document
                var test = XDocument.Load($"{folder}\\SFW\\SfwConfig.xml");
                var test2 = test.Descendants("Default_WC").Single();
                test2.Attribute("WC_Nbr").Value = "41001";
                test.Save($"{folder}\\SFW\\SfwConfig.xml");
            }
        }

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                xmlDoc = null;
            }
        }
    }
}
