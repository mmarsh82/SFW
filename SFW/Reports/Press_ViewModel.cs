using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using SFW.Commands;
using SFW.Enumerations;
using SFW.Model;

namespace SFW.Reports
{
    public class Press_ViewModel : ViewModelBase
    {
        #region Properties

        public PressReport Report { get; set; }
        public ObservableCollection<TabItem> ShiftCollection { get; set; }
        public TabItem SelectedShift { get; set; }

        private RelayCommand _reportAction;

        #endregion

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="wo">Work Order Object</param>
        /// <param name="isNew">Determines if this is a report creation or reader</param>
        public Press_ViewModel(WorkOrder wo, PressReportActions pressAction)
        {
            switch (pressAction)
            {
                case PressReportActions.New:
                    Report = new PressReport(wo, null);
                    Report.ShiftReportList.Add(new Press_ShiftReport(CurrentUser.FirstName, CurrentUser.LastName, Machine.GetMachineName(App.AppSqlCon, wo), DateTime.Today, App.AppSqlCon));
                    break;
                case PressReportActions.StartShift:
                    Report = new PressReport(wo, App.AppSqlCon);
                    Report.ShiftReportList.Insert(0, new Press_ShiftReport(CurrentUser.FirstName, CurrentUser.LastName, Machine.GetMachineName(App.AppSqlCon, wo), DateTime.Today, App.AppSqlCon));
                    break;
                case PressReportActions.ViewReport:

                    break;
                case PressReportActions.LogProgress:

                    break;
            }
            ShiftCollection = new ObservableCollection<TabItem>(LoadShiftCollection(Report.ShiftReportList));
            SelectedShift = ShiftCollection[0];
        }

        /// <summary>
        /// Load a TabItem list for observable collection initialization
        /// </summary>
        /// <param name="psReportList">List of Press Shift Report objects</param>
        /// <returns></returns>
        public List<TabItem> LoadShiftCollection(List<Press_ShiftReport> psReportList)
        {
            var _tempList = new List<TabItem>();
            foreach (var s in psReportList)
            {
                _tempList.Add(new TabItem
                {
                    Content = new PressShift_View
                    {
                        DataContext = new PressShift_ViewModel(s)
                    },
                    Header = $"{s.ReportDate.ToShortDateString()} Shift {s.Shift}"
                });
            }
            return _tempList;
        }

        #region Report Action ICommand

        public ICommand ReportActionICommand
        {
            get
            {
                if (_reportAction == null)
                {
                    _reportAction = new RelayCommand(ReportActionExecute, ReportActionCanExecute);
                }
                return _reportAction;
            }
        }

        private void ReportActionExecute(object parameter)
        {
            var _tempVM = (PressShift_ViewModel)parameter;
            switch (_tempVM.ReportAction)
            {
                case "Submit":
                    ((PressShift_ViewModel)parameter).UpdateView(Report.Submit(Report, _tempVM.PSReport, App.AppSqlCon), "Update");
                    break;
                case "Update":
                    Report.Update(Report, _tempVM.PSReport, App.AppSqlCon);
                    break;
            }
        }
        private bool ReportActionCanExecute(object parameter)
        {
            var _tempVM = (PressShift_ViewModel)parameter;
            switch (_tempVM.ReportAction)
            {
                case "Submit":
                    return _tempVM.PSReport?.CrewList?.Count > 0;
                case "Update":
                    return _tempVM.PSReport?.CrewList?.Count > 0 && _tempVM.PSReport?.ReportID > 0;
                default:
                    return false;
            }
        }

        #endregion
    }
}
