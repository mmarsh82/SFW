using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Model
{
    /// <summary>
    /// Schedule's Work order object
    /// </summary>
    public class WorkOrder : ModelBase
    {
        #region Properties

        public string ID { get; set; }
        public string Test { get; set; }

        #endregion

        public WorkOrder()
        {
            
        }
    }
}
