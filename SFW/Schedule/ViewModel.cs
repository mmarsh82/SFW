using M2kClient;
using SFW.Commands;
using SFW.Converters;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static string[] ScheduleViewFilter;
        public static ICollectionView ScheduleView { get; set; }

        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                try
                {
                    _selectedWO = value;
                    Controls.WorkSpaceDock.UpdateChildDock(0, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                    if (value != null)
                    {
                        var _wo = new WorkOrder(value.Row);
                        if (_wo.Inspection)
                        {
                            Controls.WorkSpaceDock.UpdateChildDock(1, 1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) });
                        }
                        else
                        {
                            Controls.WorkSpaceDock.UpdateChildDock(1, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) });
                        }
                    }
                    OnPropertyChanged(nameof(SelectedWorkOrder));
                }
                catch (Exception)
                { }
            }
        }
        private object _oldSelectedWO;

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
                var _filter = string.IsNullOrEmpty(value) ? "" : ((DataView)ScheduleView.SourceCollection).Table.SearchRowFilter(value);
                ScheduleFilter(_filter, 0);
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
                ScheduleFilter(_filter, 4);
                _insp = value;
                OnPropertyChanged(nameof(InspectionFilter));
            }
        }

        private bool _close;
        public bool ClosedFilter
        {
            get { return _close; }
            set
            {
                var _filter = value ? "[Status] = 'C'" : "[Status] <> 'C'";
                ScheduleFilter(_filter, 5);
                _close = value;
                OnPropertyChanged(nameof(ClosedFilter));
            }
        }

        private bool _site;
        public bool SiteFilter
        {
            get { return _site; }
            set
            {
                var _filter = $"[Site] = {App.SiteNumber}";
                ScheduleFilter(_filter, 6);
                _site = value;
                OnPropertyChanged(nameof(SiteFilter));
            }
        }

        private bool Refresh { get; set; }

        public delegate void LoadDelegate(string s);
        public LoadDelegate LoadAsyncDelegate { get; private set; }
        public LoadDelegate FilterAsyncDelegate { get; private set; }
        public static IAsyncResult LoadAsyncComplete { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private RelayCommand _stateChange;
        private RelayCommand _priChange;

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            Refresh = false;
            LoadAsyncDelegate = new LoadDelegate(ViewLoading);
            FilterAsyncDelegate = new LoadDelegate(FilterView);
            LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(App.ViewFilter[App.SiteNumber], new AsyncCallback(ViewLoaded), null);
            RefreshTimer.Add(RefreshSchedule);
            ScheduleViewFilter = new string[7];
            ScheduleFilter($"[Site] = {App.SiteNumber}", 6);
            ClosedFilter = false;
        }

        /// <summary>
        /// Schedule ViewModel constructor for loading in a specific workcenter
        /// </summary>
        /// <param name="machineNumber">Machine Number to load into the schedule</param>
        public ViewModel(string machineNumber)
        {
            try
            {
                LoadAsyncDelegate = new LoadDelegate(ViewLoading);
                FilterAsyncDelegate = new LoadDelegate(FilterView);
                LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(machineNumber, new AsyncCallback(ViewLoaded), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Schedule\n{ex.Message}", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// 5 = Closed Filter
        /// 6 = Site Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public static void ScheduleFilter(string filter, int index)
        {
            if (ScheduleViewFilter != null)
            {
                ScheduleViewFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in ScheduleViewFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                var _tempList = new List<DataView>();
                if (ScheduleView != null)
                {
                    ((DataView)ScheduleView.SourceCollection).RowFilter = _filterStr;
                    ScheduleView.Refresh();
                }
            }
            else
            {
                ScheduleViewFilter = new string[6];
            }
        }

        /// <summary>
        /// Clears the schedule filter string array
        /// </summary>
        public static void ClearFilter()
        {
            if (ScheduleViewFilter != null)
            {
                ScheduleViewFilter = new string[7];
                ((DataView)ScheduleView.SourceCollection).RowFilter = "";
                ScheduleFilter("[Status] <> 'C'", 5);
                ScheduleFilter($"[Site] = {App.SiteNumber}", 6);
                ScheduleView.Refresh();
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
                IsLoading = true;
                if (Refresh)
                {
                    ModelBase.BuildMasterDataSet(UserConfig.GetIROD(), App.Site, App.AppSqlCon);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ViewLoaded(IAsyncResult r)
        {
            try
            {
                RefreshTimer.IsRefreshing = IsLoading = Refresh = false;
                MainWindowViewModel.DisplayAction = false;
                var _oldfilter = string.Empty;
                if (ScheduleView != null && CurrentUser.IsLoggedIn)
                {
                    _oldfilter = ((DataView)ScheduleView.SourceCollection).RowFilter;
                }
                ScheduleView = CollectionViewSource.GetDefaultView(ModelBase.MasterDataSet.Tables["Master"]);
                ScheduleFilter(UserConfig.BuildMachineFilter(), 1);
                ScheduleFilter(UserConfig.BuildPriorityFilter(), 3);
                if (ScheduleView.GroupDescriptions.Count() == 0)
                {
                    ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter()));
                }
                if (_oldSelectedWO != null)
                {
                    if (((DataView)ScheduleView.SourceCollection).Table.AsEnumerable().Any(row => row.Field<string>("WorkOrderID") == ((DataRowView)_oldSelectedWO).Row.Field<string>("WorkOrderID")))
                    {
                        var _index = ScheduleView.IndexOf(_oldSelectedWO, "WorkOrderID");
                        ScheduleView.MoveCurrentToPosition(_index);
                        SelectedWorkOrder = (DataRowView)_oldSelectedWO;
                    }
                    else
                    {
                        ScheduleView.MoveCurrentToPosition(-1);
                        SelectedWorkOrder = null;
                    }
                }
                if (!string.IsNullOrEmpty(_oldfilter))
                {
                    ((DataView)ScheduleView.SourceCollection).RowFilter = _oldfilter;
                }
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    SearchFilter = SearchFilter;
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ScheduleView)));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (!IsLoading)
                {
                    RefreshTimer.IsRefreshing = IsLoading = Refresh = true;
                    MainWindowViewModel.DisplayAction = App.LoadedModule == Enumerations.UsersControls.Schedule;
                    _oldSelectedWO = ScheduleView.CurrentItem;
                    SelectedWorkOrder = null;
                    LoadAsyncComplete = LoadAsyncDelegate.BeginInvoke(((DataView)ScheduleView.SourceCollection).RowFilter, new AsyncCallback(ViewLoaded), null);
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
