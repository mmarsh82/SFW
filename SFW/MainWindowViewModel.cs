using SFW.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

//Created by Michael Marsh 4-19-18

namespace SFW
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        public IList<Machine> WorkCenterList { get; set; }
        public ICollectionView ScheduleView { get; set; }
        

        #endregion

        /// <summary>
        /// Main Window View Default Constructor
        /// </summary>
        public MainWindowViewModel()
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
