using M2kClient;
using SFW.Commands;
using SFW.Converters;
using SFW.Enumerations;
using SFW.Helpers;
using SFW.Model;
using SFW.Model.Production;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule
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
                Filter(_filter, 6);
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
                Filter(_filter, 5);
                _close = value;
                OnPropertyChanged(nameof(ClosedFilter));
            }
        }

        public IList<QueState> QueStateList { get; set; }

        private RelayCommand _stateChange;
        private RelayCommand _priChange;

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            ApplicationTimer.ActionList.Add(Refresh);
            CollectionView = new ListCollectionView(new DataView());
            Filter($"[Site] = {App.SiteNumber}", 3);
            if (CurrentUser.BasicUser)
            {
                Filter(UserConfig.BuildMachineFilter(), 1);
            }
            Filter(UserConfig.BuildPriorityFilter(), 4);
            ClosedFilter = false;
            InspectionFilter = false;
            QueStateList = Enum.GetValues(typeof(QueState)).Cast<QueState>().ToList();


            Initialize();
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
                if (App.LoadedModule == UsersControls.Schedule)
                {
                    var _dRow = (DataRowView)CollectionView.CurrentItem;
                    if (_dRow != null)
                    {
                        var _wo = new WorkOrder(_dRow.Row);
                        var _action = _wo.Product.Inspection
                            ? new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(1, 1, new ShopRoute.QTask.View { DataContext = new ShopRoute.QTask.ViewModel(_wo) }); })
                            : new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(1, 1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) }); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Reset the filter after a log in event
        /// </summary>
        public override void ResetFilter()
        {
            Filter($"[Site] = {App.SiteNumber}", 3);
            var _compUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Contains('\\')
                ? System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]
                : System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (CurrentUser.BasicUser || _compUser == CurrentUser.DomainUserName || string.IsNullOrEmpty(CurrentUser.DomainUserName))
            {
                Filter(UserConfig.BuildMachineFilter(), 1);
            }
            else
            {
                Filter("", 1);
            }
            Filter(UserConfig.BuildPriorityFilter(), 4);
            ClosedFilter = false;
            InspectionFilter = false;
            if (CollectionView != null)
            {
                ((DataView)CollectionView.SourceCollection).RowFilter = GetFilter();
            }
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
        }

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public override void Refresh()
        {
            try
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
                if (App.LoadedModule == Enumerations.UsersControls.Schedule)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                    if (CollectionView != null)
                    {
                        Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.MoveCurrentToPosition(_index); }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prod Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initialize the production schedule view
        /// </summary>
        public override void Initialize()
        {
            try
            {
                var _tempTable = ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name];
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prod Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
