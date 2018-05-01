using SFW.Converters;
using SFW.Model;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static ICollectionView ScheduleView { get; set; }

        #endregion

        public ViewModel()
        {
            if (ScheduleView == null)
            {
                ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetWorkCenterList(App.AppSqlCon));
                ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(ScheduleView.Cast<Machine>().ToList())));
            }
        }
    }
}
