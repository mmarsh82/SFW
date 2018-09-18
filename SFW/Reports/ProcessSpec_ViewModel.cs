using SFW.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Reports
{
    public class ProcessSpec_ViewModel
    {
        #region Properties

        public WorkOrder ShopOrder { get; set; }

        #endregion

        public ProcessSpec_ViewModel(WorkOrder wo)
        {
            ShopOrder = wo;
        }
    }
}
