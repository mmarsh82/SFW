using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SFW.Enumerations;
using SFW.Model;

namespace SFW.Reports
{
    public class Press_ViewModel : ViewModelBase
    {
        #region Properties

        public WorkOrder ShopOrder { get; set; }
        public PressReport Report { get; set; }
        public ObservableCollection<TabItem> ShiftCollection { get; set; }
        public TabItem SelectedShift { get; set; }

        #endregion

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="wo">Work Order Object</param>
        /// <param name="isNew">Determines if this is a report creation or reader</param>
        public Press_ViewModel(WorkOrder wo, PressReportActions pressAction)
        {
            ShopOrder = wo;
            switch (pressAction)
            {
                case PressReportActions.New:
                    Report = new PressReport();
                    Report.ShiftReportList.Add(new Press_ShiftReport(CurrentUser.FirstName, CurrentUser.LastName, Machine.GetMachineName(App.AppSqlCon, wo), DateTime.Today, App.AppSqlCon));
                    var _tempList = new List<TabItem>
                    {
                        new TabItem { Content = new PressShift_View { DataContext = new PressShift_ViewModel(Report.ShiftReportList[0]) }, Header = $"{Report.ShiftReportList[0].ReportDate.ToShortDateString()} Shift {Report.ShiftReportList[0].Shift}" }
                    };
                    ShiftCollection = new ObservableCollection<TabItem>(_tempList);
                    SelectedShift = ShiftCollection[0];
                    break;
                case PressReportActions.StartShift:
                    break;
                case PressReportActions.ViewReport:
                    break;
                case PressReportActions.LogProgress:
                    break;
            }
        }
    }
}
