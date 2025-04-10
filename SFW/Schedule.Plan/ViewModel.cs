using M2kClient;
using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public static string[] PlanViewFilter;
        public static DataView PlanningView { get; set; }

        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                try
                {
                    _selectedWO = value;
                    Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                    if (value != null)
                    {
                        var _wo = new WorkOrder(value.Row);
                        if (_wo.Inspection)
                        {
                            Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) });
                        }
                        else
                        {
                            Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) });
                        }
                    }
                    OnPropertyChanged(nameof(SelectedWorkOrder));
                }
                catch (Exception)
                { }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

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
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(PlanningView)));
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
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(PlanningView)));
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
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(PlanningView)));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
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
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                FilterAsyncDelegate = new LoadDelegate(FilterView);
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(App.ViewFilter[App.SiteNumber], new AsyncCallback(ViewLoaded), null);
                RefreshTimer.Add(RefreshSchedule);
                PlanViewFilter = new string[8];
                TypeCollection = new ObservableCollection<string> { "All", "Work Order", "Plan" };
                SelectedType = TypeCollection.FirstOrDefault(o => o == "All");
                SelectedDate = DateTime.Today.AddMonths(1);
                PlannerCollection = Sku.GetPlannerCollection();
                SelectedPlanner = PlannerCollection.FirstOrDefault(o => o == "All");
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
        public static void PlanFilter(string filter, int index)
        {
            if (PlanViewFilter != null)
            {
                PlanViewFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in PlanViewFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                if (PlanningView != null && PlanningView.Table.Rows.Count > 0)
                {
                    PlanningView.RowFilter = _filterStr;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(PlanningView)));
                }
            }
            else
            {
                PlanViewFilter = new string[6];
            }
        }

        /// <summary>
        /// Clears the schedule filter string array
        /// </summary>
        public void ClearFilter()
        {
            if (PlanViewFilter != null)
            {
                PlanViewFilter = new string[7];
                if (PlanningView != null && PlanningView!= null && PlanningView.RowFilter != null)
                {
                    PlanningView.RowFilter = "";
                }
                PlanFilter("[Status] <> 'C'", 5);
                PlanFilter($"[Site] = {App.SiteNumber}", 6);
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(PlanningView)));
            }
        }

        #region Loading Async Delegation Implementation

        /// <summary>
        /// Async filter the schedule view
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        public void FilterSchedule(string filter)
        {
            LoadAsyncComplete = FilterAsyncDelegate.BeginInvoke(filter, new AsyncCallback(ViewLoaded), null);
        }

        public void FilterView(string filter)
        {
            IsLoading = true;
            ViewLoading(filter);
        }

        public void ViewLoading(string filter)
        {
            try
            {
                var _oldfilter = string.Empty;
                if (PlanningView != null && CurrentUser.IsLoggedIn)
                {
                    _oldfilter = PlanningView.RowFilter;
                }
                PlanningView = ModelBase.MasterDataSet.Tables["Plan"].AsDataView();
                PlanFilter(UserConfig.BuildMachineFilter(), 1);
                PlanFilter(UserConfig.BuildPriorityFilter(), 3);
                if(SelectedWorkOrder != null)
                {
                    var _targetId = SelectedWorkOrder.Row.SafeGetField<int>("WorkOrderID").ToString();
                    var _index = PlanningView.Cast<DataRowView>().Select((row, idx) => new { row, idx }).FirstOrDefault(o => o.row["WorkOrderID"].ToString() == _targetId)?.idx ?? -1;
                    if (_index == -1)
                    {
                        SelectedWorkOrder = PlanningView[0];
                    }
                    else
                    {
                        SelectedWorkOrder = null;
                        SelectedWorkOrder = PlanningView[_index];
                    }
                }
                if (!string.IsNullOrEmpty(_oldfilter))
                {
                    PlanningView.RowFilter = _oldfilter;
                }
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    SearchFilter = SearchFilter;
                }
                OnPropertyChanged(nameof(PlanningView));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Planning Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            IsLoading = false;
            if (PlannerCollection != null && PlannerCollection.Count == 1)
            {
                PlannerCollection = Sku.GetPlannerCollection();
                OnPropertyChanged(nameof(PlannerCollection));
            }
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(PlanningView)));
        }

        #endregion

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public void RefreshSchedule()
        {
            try
            {
                if (!IsLoading)
                {
                    MainWindowViewModel.DisplayAction = App.LoadedModule == Enumerations.UsersControls.Plan;
                    LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(PlanningView.RowFilter, new AsyncCallback(ViewLoaded), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
