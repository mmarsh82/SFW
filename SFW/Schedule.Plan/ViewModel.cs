using M2kClient;
using SFW.Commands;
using SFW.Converters;
using SFW.Helpers;
using SFW.Model;
using SFW.Model.Production;
using SFW.Model.SupplyChain;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule.Plan
{
    public class ViewModel : ScheduleBase
    {
        #region Properties

        private bool _insp;
        public bool InspectionFilter
        {
            get { return _insp; }
            set
            {
                var _filter = value ? "[Inspection] = 'Y'" : "";
                Filter(_filter, 4);
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
                        Filter("", 5);
                        break;
                    case "Work Order":
                        Filter("[WO_Type]<>'P'", 5);
                        break;
                    case "Plan":
                        Filter("[WO_Type]='P'", 5);
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
                Filter($"[WO_DueDate] <= '{value}'", 6);
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
                        Filter("", 7);
                        break;
                    default:
                        Filter($"[PlannerName]='{value}'", 7);
                        break;
                }
                OnPropertyChanged(nameof(SelectedPlanner));
            }
        }

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
                CollectionView = new ListCollectionView(new DataView());
                TypeCollection = new ObservableCollection<string> { "All", "Work Order", "Plan" };
                SelectedType = TypeCollection[0];
                SelectedDate = DateTime.Today.AddMonths(1);
                PlannerCollection = WorkPlan.GetPlannerCollection();
                SelectedPlanner = PlannerCollection[0];
                if (ModelBase.MasterDataSet.Tables.Contains(typeof(WorkPlan).Name))
                {
                    Initialize();
                    ApplicationTimer.ActionList.Add(Refresh);
                }
            }
        }

        /// <summary>
        /// Tracks the item selections from the CollectionView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionView_ItemChanged(object sender, EventArgs e)
        {
            try
            {
                if (App.LoadedModule == Enumerations.UsersControls.Plan)
                {
                    var _dRow = (DataRowView)CollectionView.CurrentItem;
                    if (_dRow != null)
                    {
                        var _wo = new WorkOrder(_dRow.Row);
                        var _action = _wo.Product.Inspection
                            ? new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) }); })
                            : new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(11, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) }); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public override void Refresh()
        {
            try
            {
                if (ModelBase.MasterDataSet.Tables.Contains(typeof(WorkPlan).Name) && (CollectionView == null || ((DataView)CollectionView.SourceCollection).Table == null))
                {
                    CollectionView = new ListCollectionView(new DataView());
                    Initialize();
                }
                else
                {
                    var _index = CollectionView.CurrentPosition;
                    foreach (var _keyValPair in UserConfig.GetIROD())
                    {
                        DataRow[] _rows = ((DataView)CollectionView.SourceCollection).Table.Select($"MachineNumber={_keyValPair.Key}");
                        foreach (DataRow _row in _rows)
                        {
                            var _rIndex = ((DataView)CollectionView.SourceCollection).Table.Rows.IndexOf(_row);
                            ((DataView)CollectionView.SourceCollection).Table.Rows[_rIndex].SetField("MachineOrder", _keyValPair.Value);
                        }
                    }
                    ((DataView)CollectionView.SourceCollection).Sort = "MachineOrder, MachineGroup, MachineNumber, WO_Priority, Sched_Shift, Sched_Priority";
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                    if (CollectionView != null)
                    {
                        Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.MoveCurrentToPosition(_index); }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Plan Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public override void Initialize()
        {
            try
            {
                if (ModelBase.MasterDataSet.Tables.Contains(typeof(WorkPlan).Name))
                {
                    var _tempTable = ModelBase.MasterDataSet.Tables[typeof(WorkPlan).Name];
                    foreach (var _keyValPair in UserConfig.GetIROD())
                    {
                        DataRow[] _rows = _tempTable.Select($"MachineNumber={_keyValPair.Key}");
                        foreach (DataRow _row in _rows)
                        {
                            var _index = _tempTable.Rows.IndexOf(_row);
                            _tempTable.Rows[_index].SetField("MachineOrder", _keyValPair.Value);
                        }
                    }
                    var _tempDataView = _tempTable.AsDataView();
                    _tempDataView.Sort = "MachineOrder, MachineGroup, MachineNumber, WO_Priority, Sched_Shift, Sched_Priority";
                    CollectionView = new ListCollectionView(_tempDataView);
                    if (CollectionView.GroupDescriptions.Count() != 0)
                    {
                        Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
                    }
                    Application.Current?.Dispatcher.Invoke(new Action(delegate
                    {
                        CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("MachineGroup"));
                        CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter()));
                    }));

                    OnPropertyChanged(nameof(CollectionView));
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                    CollectionView.CurrentChanged += CollectionView_ItemChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Plan Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var _dRow = (DataRowView)CollectionView.CurrentItem;
                var _oldPri = _dRow?.Row?.SafeGetField<string>("WO_Priority").ToString();
                if (char.TryParse(_dRow?.Row?.SafeGetField<string>("WO_Priority").ToString(), out char _oldPriChar))
                {
                    var _oldPriInt = _oldPriChar % 32;
                    var _newPriInt = Convert.ToChar(parameter) % 32;
                    if (_oldPriInt < _newPriInt && (_dRow?.Row?.SafeGetField<int>("Sched_Shift").ToString() != "999" || _dRow?.Row?.SafeGetField<int>("Sched_Priority").ToString() != "999"))
                    {
                        new ClearPriority().Execute(_dRow);
                    }
                }
                if (!string.IsNullOrEmpty(parameter?.ToString()))
                {
                    var _woNumber = _dRow?.Row?.SafeGetField<string>("WorkOrder");
                    var _changeRequest = M2kCommand.EditRecord("WP", _woNumber, 40, parameter.ToString(), UdArrayCommand.Replace, App.ErpCon);
                    if (!string.IsNullOrEmpty(_changeRequest))
                    {
                        MessageBox.Show(_changeRequest, "ERP Record Error");
                        _dRow.BeginEdit();
                        _dRow["WO_Priority"] = _oldPri;
                        _dRow.EndEdit();
                    }
                    else
                    {
                        _dRow.BeginEdit();
                        _dRow["WO_Priority"] = parameter.ToString();
                        _dRow.EndEdit();
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
