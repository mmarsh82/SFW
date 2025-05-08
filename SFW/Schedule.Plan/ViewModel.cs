using M2kClient;
using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule.Plan
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public string[] PlanViewFilter;
        public DataView PlanningView { get; set; }

        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                _selectedWO = value;
                if (value != null && App.LoadedModule == Enumerations.UsersControls.Plan)
                {
                    var _wo = new WorkOrder(value.Row);
                    var _action = _wo.Inspection
                        ? new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) }); })
                        : new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) }); });
                    Application.Current.Dispatcher.Invoke(_action);
                }
                OnPropertyChanged(nameof(SelectedWorkOrder));
            }
        }
        public int SelectedIndex;

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : PlanningView.Table.SearchRowFilter(value);
                PlanFilter(_filter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        private bool _insp;
        public bool InspectionFilter
        {
            get { return _insp; }
            set
            {
                var _filter = value ? "[Inspection] = 'Y'" : "";
                PlanFilter(_filter, 4);
                _insp = value;
                OnPropertyChanged(nameof(InspectionFilter));
            }
        }

        public ObservableCollection<string> TypeCollection { get; set; }
        private string _type;
        public string SelectedType
        {
            get
            { return _type; }
            set
            {
                _type = value;
                switch (value)
                {
                    case "All":
                        PlanFilter("", 5);
                        break;
                    case "Work Order":
                        PlanFilter("[WO_Type]<>'P'", 5);
                        break;
                    case "Plan":
                        PlanFilter("[WO_Type]='P'", 5);
                        break;
                }
                OnPropertyChanged(nameof(SelectedType));
            }
        }

        private DateTime _date;
        public DateTime SelectedDate
        {
            get
            { return _date; }
            set
            {
                _date = value;
                PlanFilter($"[WO_DueDate] <= '{value}'", 6);
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        public ObservableCollection<string> PlannerCollection { get; set; }
        private string _planner;
        public string SelectedPlanner
        {
            get
            { return _planner; }
            set
            {
                _planner = value;
                switch (value)
                {
                    case "All":
                        PlanFilter("", 7);
                        break;
                    default:
                        PlanFilter($"[PlannerName]='{value}'", 7);
                        break;
                }
                OnPropertyChanged(nameof(SelectedPlanner));
            }
        }

        public delegate void LoadDelegate(string s, int i);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public IAsyncResult LoadAsyncComplete { get; set; }

        private RelayCommand _stateChange;
        private RelayCommand _priChange;

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            if (App.SiteNumber == 1)
            {
                RefreshTimer.Add(RefreshSchedule);
                PlanningView = new DataView();
                PlanViewFilter = new string[8];
                TypeCollection = new ObservableCollection<string> { "All", "Work Order", "Plan" };
                SelectedType = TypeCollection[0];
                SelectedDate = DateTime.Today.AddMonths(1);
                PlannerCollection = Sku.GetPlannerCollection();
                SelectedPlanner = PlannerCollection[0];
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(PlanningView.RowFilter, 0, new AsyncCallback(ViewLoaded), null);
            }
        }

        /// <summary>
        /// Filter the schedule view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Work Center Filter
        /// 2 = Work Center Group Filter
        /// 3 = Work Order Priority Filter
        /// 4 = Inspection Filter
        /// 5 = Type Filter
        /// 6 = Date Filter
        /// 7 = Planner Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public void PlanFilter(string filter, int index)
        {
            PlanViewFilter[index] = filter;
            var _filterStr = string.Empty;
            foreach (var s in PlanViewFilter.Where(o => !string.IsNullOrEmpty(o)))
            {
                _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
            }
            PlanningView.RowFilter = _filterStr;
        }

        #region Loading Async Delegation Implementation

        public void ViewLoading(string filter, int index)
        {
            try
            {
                if (ModelBase.MasterDataSet.Tables.Contains("Plan"))
                {
                    SelectedIndex = index < 0 ? 0 : index;
                    PlanningView = ModelBase.MasterDataSet.Tables["Plan"].AsDataView();
                    PlanFilter(UserConfig.BuildMachineFilter(), 1);
                    PlanFilter(UserConfig.BuildPriorityFilter(), 3);
                    SearchFilter = !string.IsNullOrEmpty(SearchFilter) ? SearchFilter : string.Empty;
                    PlanningView.RowFilter = filter;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Planning Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            if (PlannerCollection != null && PlannerCollection.Count == 1)
            {
                PlannerCollection = Sku.GetPlannerCollection();
                OnPropertyChanged(nameof(PlannerCollection));
            }
            if (PlanningView != null && PlanningView.Count > 0)
            {
                SelectedWorkOrder = PlanningView.Count >= SelectedIndex ? PlanningView[SelectedIndex] : PlanningView[0];
            }
            OnPropertyChanged(nameof(PlanningView));
            if (App.LoadedModule == Enumerations.UsersControls.Plan)
            {
                MainWindowViewModel.DisplayAction = false;
            }
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            try
            {
                MainWindowViewModel.DisplayAction = App.LoadedModule == Enumerations.UsersControls.Plan;
                var _filter = PlanningView != null ? PlanningView.RowFilter : string.Empty;
                var _index = 0;
                if (SelectedWorkOrder != null)
                {
                    var _targetId = SelectedWorkOrder.Row.SafeGetField<string>("WorkOrderID");
                    _index = PlanningView.Cast<DataRowView>().Select((row, idx) => new { row, idx }).FirstOrDefault(o => o.row["WorkOrderID"].ToString() == _targetId)?.idx ?? 0;
                }
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(_filter, _index, new AsyncCallback(ViewLoaded), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Planning Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Order State Change ICommand

        public ICommand StateChangeICommand
        {
            get
            {
                if (_stateChange == null)
                {
                    _stateChange = new RelayCommand(StateChangeExecute, StateChangeCanExecute);
                }
                return _stateChange;
            }
        }

        private void StateChangeExecute(object parameter)
        {
            try
            {
                var _oldPri = SelectedWorkOrder?.Row?.SafeGetField<string>("WO_Priority").ToString();
                if (char.TryParse(SelectedWorkOrder?.Row?.SafeGetField<string>("WO_Priority").ToString(), out char _oldPriChar))
                {
                    var _oldPriInt = _oldPriChar % 32;
                    var _newPriInt = Convert.ToChar(parameter) % 32;
                    if (_oldPriInt < _newPriInt && (SelectedWorkOrder?.Row?.SafeGetField<int>("Sched_Shift").ToString() != "999" || SelectedWorkOrder?.Row?.SafeGetField<int>("Sched_Priority").ToString() != "999"))
                    {
                        new ClearPriority().Execute(SelectedWorkOrder);
                    }
                }
                if (!string.IsNullOrEmpty(parameter?.ToString()))
                {
                    var _woNumber = SelectedWorkOrder?.Row?.SafeGetField<string>("WorkOrder");
                    var _changeRequest = M2kCommand.EditRecord("WP", _woNumber, 40, parameter.ToString(), UdArrayCommand.Replace, App.ErpCon);
                    if (!string.IsNullOrEmpty(_changeRequest))
                    {
                        MessageBox.Show(_changeRequest, "ERP Record Error");
                        SelectedWorkOrder.BeginEdit();
                        SelectedWorkOrder["WO_Priority"] = _oldPri;
                        SelectedWorkOrder.EndEdit();
                    }
                    else
                    {
                        SelectedWorkOrder.BeginEdit();
                        SelectedWorkOrder["WO_Priority"] = parameter.ToString();
                        SelectedWorkOrder.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception");
            }
        }
        private bool StateChangeCanExecute(object parameter) => true;

        #endregion

        #region Priority Change ICommand

        public ICommand PriorityChangeICommand
        {
            get
            {
                if (_priChange == null)
                {
                    _priChange = new RelayCommand(PriorityChangeExecute, PriorityChangeCanExecute);
                }
                return _priChange;
            }
        }

        private void PriorityChangeExecute(object parameter)
        {
            var _shift = ((DataRowView)parameter).Row.SafeGetField<int>("Sched_Shift").ToString() == "999" ? 0 : Convert.ToInt32(((DataRowView)parameter).Row.SafeGetField<int>("Sched_Shift"));
            var _pri = ((DataRowView)parameter).Row.SafeGetField<int>("Sched_Priority").ToString() == "999" ? 0 : Convert.ToInt32(((DataRowView)parameter).Row.SafeGetField<int>("Sched_Priority"));
            var _woNumber = ((DataRowView)parameter).Row.SafeGetField<string>("WorkOrder");
            using (var _editPri = new Tools.PriorityEdit_ViewModel(_woNumber, _shift, _pri))
            {
                new Tools.PriorityEdit_View { DataContext = _editPri }.ShowDialog();
            }
        }
        private bool PriorityChangeCanExecute(object parameter) => true;

        #endregion
    }
}
