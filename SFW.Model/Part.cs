using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Model
{
    //TODO: refactor class to follow OOP best practices
    public class Part : Sku
    {
        #region Properties

        public string LotNumber { get; set; }
        public string DiamondNumber { get; set; }
        public string WorkOrderNumber { get; set; }
        public int WorkOrderSeq { get; set; }
        public string MachineNumber { get; set; }
        public string MachineName { get; set; }

        #endregion
    }
}
