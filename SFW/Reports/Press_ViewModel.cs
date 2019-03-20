using SFW.Commands;
using SFW.Enumerations;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

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
                    Report = new PressReport(wo, App.AppSqlCon);
                    break;
                case PressReportActions.LogProgress:
                    Report = new PressReport(wo, App.AppSqlCon);
                    Press_ShiftReport.SubmitRound(Report, Report.ShiftReportList[0], App.AppSqlCon);
                    Report.ShiftReportList[0].RoundTable = Press_ShiftReport.GetRoundTable(Convert.ToInt32(Report.ShiftReportList[0].ReportID), App.AppSqlCon);
                    break;
            }
            ShiftCollection = new ObservableCollection<TabItem>(LoadShiftCollection(Report.ShiftReportList));
            if (ShiftCollection.Count > 0)
            {
                SelectedShift = ShiftCollection[0];
            }
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

        /// <summary>
        /// Post labor to the ERP on either submission
        /// </summary>
        /// <param name="index">Index number of the Press Shift report object to use in the Press shift report list</param>
        public void PostLaborToERP(int index)
        {
            var _tempCon = new M2kClient.M2kConnection("manage", "omniquery", "omniquery", M2kClient.Database.WCCOTRAIN); //meant to only be used for testing
            var _machId = WorkOrder.GetAssignedMachine(Report.ShopOrder.OrderNumber, Report.ShopOrder.Seq, App.AppSqlCon);
            if (index > 0)
            {
                var _qtyComp = int.TryParse(Report.ShiftReportList[index - 1].RoundTable.Compute("SUM(RoundSlats)", string.Empty).ToString(), out int i) ? i : 0;
                var _count = Report.ShiftReportList[index - 1].CrewList.Count;
                foreach (var c in Report.ShiftReportList[index - 1].CrewList)
                {
                    M2kClient.M2kCommand.PostLabor("PRESS SFW", Convert.ToInt32(c.IdNumber), $"{Report.ShopOrder.OrderNumber}*{Report.ShopOrder.Seq}", _qtyComp, _machId, 'O', _tempCon, DateTime.Now.ToString("HH:mm"), _count);
                }
            }
            foreach (var c in Report.ShiftReportList[index].CrewList)
            {
                var _count = Report.ShiftReportList[index - 1].CrewList.Count;
                M2kClient.M2kCommand.PostLabor("PRESS SFW", Convert.ToInt32(c.IdNumber), $"{Report.ShopOrder.OrderNumber}*{Report.ShopOrder.Seq}", 0, _machId, 'I', _tempCon, DateTime.Now.ToString("HH:mm"), _count);
            }
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
                    PostLaborToERP(Report.ShiftReportList.IndexOf(_tempVM.PSReport));
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
