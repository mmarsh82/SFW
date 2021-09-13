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
        public string Shift
        {
            get
            { return _shift.ToString(); }
            set
            {
                _shift = int.TryParse(value, out int i) && i > 0 && i < 4 ? i : (int?)null;
                OnPropertyChanged(nameof(Shift));
            }
        }

        private RelayCommand _reportAction;
        private RelayCommand _addShift;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Press_ViewModel()
        { }

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
                    PressRound.Submit(Report, Report.ShiftReportList[0], App.AppSqlCon);
                    Report.ShiftReportList[0].RoundList = PressRound.GetRoundList(Convert.ToInt32(Report.ShiftReportList[0].ReportID), App.AppSqlCon);
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
        public List<TabItem> LoadShiftCollection(List<PressShiftReport> psReportList)
        {
            var _tempList = new List<TabItem>();
            foreach (var s in psReportList)
            {
                _tempList.Add(new TabItem
                {
                    Content = new PressShift_View
                    {
                        DataContext = new PressShift_ViewModel(s, Report.ShopOrder.Uom == "EA" || Report.ShopOrder.Uom == "PC")
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
                    PressReport.Update(Report, App.AppSqlCon);
                    Report.ShiftReportList[0].RoundList.Select(o => { o.HasChanges = false; return o; }).ToList();
                    break;
                case "Log Round":
                    PressRound.Submit(Report, Report.ShiftReportList[0], App.AppSqlCon);
                    Report.ShiftReportList[0].RoundList = PressRound.GetRoundList(Convert.ToInt32(Report.ShiftReportList[0].ReportID), App.AppSqlCon);
                    OnPropertyChanged(nameof(Report));
                    ((PressShift_ViewModel)((PressShift_View)ShiftCollection[0].Content).DataContext).UpdateView(Report.ShiftReportList[0]);
                    break;
                case "Delete Round":
                    PressRound.Delete(int.Parse(Report.ShiftReportList[0].ReportID.ToString()), Report.ShiftReportList[0].RoundList.Last().RoundNumber, App.AppSqlCon);
                    Report.ShiftReportList[0].RoundList = PressRound.GetRoundList(int.Parse(Report.ShiftReportList[0].ReportID.ToString()), App.AppSqlCon);
                    ((PressShift_ViewModel)((PressShift_View)ShiftCollection[0].Content).DataContext).UpdateView(Report.ShiftReportList[0]);
                    OnPropertyChanged(nameof(Report));
                    break;
                default:
                    break;
            }
        }
        private bool ReportActionCanExecute(object parameter)
        {
            if (parameter != null)
            {
                switch (parameter.ToString())
                {
                    case "Submit":
                        return Report.SlatTransfer > 0 && Report.RollLength > 0 && (Report.SlatBlankout >= 0 || !IsBlankVis) && !string.IsNullOrEmpty(Shift);
                    case "Update":
                        return Report.ShiftReportList[0].RoundList.Count(o => o.HasChanges) > 0;
                    case "Log Round":
                        return true;
                    case "Delete Round":
                        return Report.ShiftReportList[0].RoundList.Count > 0;
                    default:
                        return false;
                }
            }
            return false;
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
            if (Report.ShiftReportList.Count(o => o.Shift == int.Parse(Shift) && o.ReportDate == DateTime.Today) == 0)
            {
                if (int.TryParse(Shift.ToString(), out int i))
                {
                    Report.ShiftReportList.Insert(0, new PressShiftReport(Report.ShopOrder, DateTime.Now, i, App.AppSqlCon));
                    ShiftCollection = new ObservableCollection<TabItem>(LoadShiftCollection(PressShiftReport.GetPress_ShiftReportList(Report.ShopOrder.OrderNumber, Report.ShopOrder.Machine, App.AppSqlCon)));
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
        private bool AddShiftCanExecute(object parameter) => !CanCreate && !string.IsNullOrEmpty(Shift);

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
