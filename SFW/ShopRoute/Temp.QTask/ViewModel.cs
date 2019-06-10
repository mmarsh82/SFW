using SFW.Model;
using System.Collections.Generic;

namespace SFW.ShopRoute.Temp.QTask
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public List<QualityTask> QTaskList { get; set; }

        #endregion

        public ViewModel()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partNbr"></param>
        public ViewModel(string partNbr)
        {
            if (QTaskList == null)
            {
                QTaskList = QualityTask.GetQTaskList(partNbr, App.AppSqlCon);
            }
        }
    }
}
