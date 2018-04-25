using SFW.Converters;
using SFW.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace SFW.Schedule
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public IList<Machine> WorkCenterList { get; set; }
        public ICollectionView ScheduleView { get; set; }

        #endregion

        public ViewModel()
        {
            if (WorkCenterList == null)
            {
                WorkCenterList = Machine.GetWorkCenterList(App.AppSqlCon);
            }
            if (ScheduleView == null)
            {
                ScheduleView = CollectionViewSource.GetDefaultView(WorkCenterList);
                ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(WorkCenterList.ToList())));
            }
        }
    }
}
