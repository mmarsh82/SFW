using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SFW.Model
{
    public class Module
    {
        #region Properties

        public ModuleType Group { get; set; }
        public Type TableType { get; set; }
        public MethodInfo TableMethod { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public Module()
        { }

        /// <summary>
        /// Overridden constructor
        /// </summary>
        /// <param name="modType">Type of module to create</param>
        /// <param name="name">Name of the module table</param>
        /// <param name="act">Table loading method as an action</param>
        public Module(ModuleType modType, Type objType, MethodInfo tblMethod)
        {
            Group = modType;
            TableType = objType;
            TableMethod = tblMethod;
        }

        /// <summary>
        /// Get a list of module objects
        /// </summary>
        /// <returns></returns>
        public static List<Module> GetModuleList()
        {
            var _rtnList = new List<Module>();

            try
            {
                foreach (var _assembly in AppDomain.CurrentDomain.GetAssemblies().Where(o => o.ManifestModule.Name.Contains("Model")))
                {
                    foreach (var _type in _assembly.GetTypes().Where(o => o.IsClass && o.GetInterface("IModuleData") != null))
                    {
                        var _method = _type.GetMethod("GetTable");
                        var _moduleType = Enum.TryParse(_type.Namespace.Replace("SFW.Model.", ""), out ModuleType mt) ? mt : ModuleType.Production;
                        _rtnList.Add(new Module(_moduleType, _type, _method));
                    }
                }
                return _rtnList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a list of module objects
        /// </summary>
        /// <returns></returns>
        public static List<Module> GetModuleList(List<ModuleType> modTypeList)
        {
            var _rtnList = new List<Module>();

            try
            {
                foreach (var _assembly in AppDomain.CurrentDomain.GetAssemblies().Where(o => o.ManifestModule.Name.Contains("Model")))
                {
                    foreach (var _type in _assembly.GetTypes().Where(o => o.IsClass && o.GetInterface("IModuleData") != null))
                    {
                        var _method = _type.GetMethod("GetTable");
                        var _moduleType = Enum.TryParse(_type.Namespace.Replace("SFW.Model.", ""), out ModuleType mt) ? mt : ModuleType.Production;
                        if (modTypeList.Contains(_moduleType))
                        {
                            _rtnList.Add(new Module(_moduleType, _type, _method));
                        }
                    }
                }
                return _rtnList;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public enum ModuleType
    {
        Production = 0,
        Sales = 1,
        CycleCount = 2,
        Containers = 3,
        Quality = 4,
        Product = 5,
        Management = 6,
        SupplyChain = 7,
        InventoryControl = 8
    }
}
