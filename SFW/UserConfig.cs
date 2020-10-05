using System.Collections.Generic;
using System.Linq;

namespace SFW
{
    public class UserConfig
    {
        #region Properties

        public int SiteNumber { get; set; }
        public int Position { get; set; }
        public string MachineNumber { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UserConfig()
        { }

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
                    foreach (var v in App.DefualtWorkCenter.Where(o => o.SiteNumber == App.SiteNumber && !string.IsNullOrEmpty(o.MachineNumber)))
                    {
                        _irod.Add(v.MachineNumber, v.Position);
                    }
                }
            }
            return _irod;
        }
    }
}
