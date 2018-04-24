using SFW.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.ViewModel
{
    public class MainWindow : ViewModelBase
    {
        #region Properties

        public IList<WorkCenter> WorkCenterList { get; set; }

        #endregion

        public MainWindow()
        {
            if (WorkCenterList == null)
            {
                WorkCenterList = WorkCenter.GetWorkCenterList();
            }
        }
    }
}
