using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW
{
    public class UConfig
    {
        #region Properties

        public int SiteNumber { get; set; }
        public int Position { get; set; }
        public string MachineNumber { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UConfig()
        { }
    }
}
