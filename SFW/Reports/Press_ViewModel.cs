using SFW.Enumerations;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace SFW.Reports
{
    public class Press_ViewModel : ViewModelBase
    {
        #region Properties

        public PressReport Report { get; set; }
        public ObservableCollection<TabItem> ShiftCollection { get; set; }
        public TabItem SelectedShift { get; set; }
        public bool IsBlankVis { get { return Report.ShopOrder.Uom == "EA" || Report.ShopOrder.Uom == "PC"; } }
        public bool CanCreate { get { return Report.IsNew; } }
        public string ReportAction { get { return CanCreate ? "Submit" : "Update"; } }

        private int? _shift;
        public int? Shift
        {
            get
            { return _shift; }
            set
            { _shift = value; OnPropertyChanged(nameof(Shift)); }
        }

        private RelayCommand _reportAction;
        private RelayCommand _addShift;

        #endregion

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="wo">Work Order Object</param>
        /// <param name="pressAction">Type of action to take on loading the report</param>
        public Press_ViewModel(WorkOrder wo, PressReportActions pressAction)
        {
            switch (pressAction)
            {
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
            switch (parameter.ToString())
            {
                case "Submit":
                    PressReport.Submit(Report, App.AppSqlCon);
                    AddShiftExecute(null);
                    Report.IsNew = false;
                    Shift = null;
                    OnPropertyChanged(nameof(CanCreate));
                    OnPropertyChanged(nameof(ReportAction));
                    break;
                case "Update":
                    //Report.Update(Report, App.AppSqlCon);
                    break;
            }
        }
        private bool ReportActionCanExecute(object parameter)
        {
            return Report.SlatTransfer > 0 && Report.RollLength > 0 && (Report.SlatBlankout > 0 || !IsBlankVis);
        }

        #endregion

        #region Add Shift ICommand

        public ICommand AddShiftICommand
        {
            get
            {
                if (_addShift == null)
                {
                    _addShift = new RelayCommand(AddShiftExecute, AddShiftCanExecute);
                }
                return _addShift;
            }
        }

        private void AddShiftExecute(object parameter)
        {
            if (Report.ShiftReportList.Count(o => o.Shift == Shift && o.ReportDate == DateTime.Today) == 0)
            {
                if (int.TryParse(Shift.ToString(), out int i))
                {
                    new Press_ShiftReport(Report.ShopOrder, DateTime.Now, i, App.AppSqlCon);
                    ShiftCollection = new ObservableCollection<TabItem>(LoadShiftCollection(Press_ShiftReport.GetPress_ShiftReportList(Report.ShopOrder.OrderNumber, Report.ShopOrder.Machine, App.AppSqlCon)));
                    OnPropertyChanged(nameof(ShiftCollection));
                    Shift = null;
                    SelectedShift = ShiftCollection[0];
                    OnPropertyChanged(nameof(SelectedShift));
                }
            }
            else
            {
                //TODO: add in a focus command so that instead of a warning with instructions it will focus the TabItem
                System.Windows.MessageBox.Show($"Report sheet for this shift already exists.\nPlease select {DateTime.Today.ToShortDateString()} Shift {Shift} in the workspace.", "Duplicate Entry", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        private bool AddShiftCanExecute(object parameter) => !CanCreate && Shift > 0 && Shift < 4;

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                Report = null;
                ShiftCollection = null;
                SelectedShift = null;
                _reportAction = null;
                _addShift = null;
            }
        }
    }
}
