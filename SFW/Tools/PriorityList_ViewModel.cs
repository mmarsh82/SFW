using M2kClient;
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

namespace SFW.Tools
{
    public class PriorityList_ViewModel : ViewModelBase
    {
        #region Properties

        public ICollectionView PriorityView { get; set; }
        public bool ViewIsEmpty { get { return PriorityView != null && !PriorityView.IsEmpty; } }

        public IList<string> MachineList { get; set; }

        private string _selMach;
        public string SelectedMachine
        {
            get
            { return _selMach; }
            set
            {
                _selMach = value;

                OnPropertyChanged(nameof(SelectedMachine));
            }
        }

        private string _wo;
        public string WorkOrderInput
        {
            get
            { return _wo; }
            set
            {
                _wo = value;
                OnPropertyChanged(nameof(WorkOrderInput));
                IsWorkOrderValid = false;
                OnPropertyChanged(nameof(CanEditPriority));
            }
        }

        private bool _woValid;
        public bool IsWorkOrderValid
        {
            get
            { return _woValid; }
            set
            {
                _woValid = value ? value : WorkOrder.Exists(WorkOrderInput, SelectedMachine, 999);
                OnPropertyChanged(nameof(IsWorkOrderValid));
            }
        }

        private string _pri;
        public string PriorityInput
        {
            get
            { return _pri?.ToString(); }
            set
            {
                _pri = int.TryParse(value, out int i) ? i.ToString() : null;
                OnPropertyChanged(nameof(PriorityInput));
            }
        }
        public bool CanEditPriority { get { return IsWorkOrderValid && !string.IsNullOrEmpty(WorkOrderInput); } }

        private string _srch;
        public string SearchInput
        {
            get { return _srch; }
            set
            {
                _srch = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : ((DataView)PriorityView.SourceCollection).Table.SearchRowFilter(value);
                ViewFilter(_filter, 0);
                OnPropertyChanged(nameof(SearchInput));
            }
        }

        private string[] _priViewFltr;
        public string[] PriorityViewFilter
        {
            get { return _priViewFltr; }
            set
            {
                _priViewFltr = value;
                OnPropertyChanged(nameof(PriorityViewFilter));
            }
        }

        private bool _dAct;
        public bool DisplayAction
        {
            get
            { return _dAct; }
            set
            {
                _dAct = value;
                OnPropertyChanged(nameof(DisplayAction));
            }
        }

        RelayCommand _main;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PriorityList_ViewModel()
        {
            PriorityView = CollectionViewSource.GetDefaultView(ModelBase.MasterDataSet.Tables["Master"]);
            MachineList = Machine.GetMachineList(false, App.SiteNumber);
            IsWorkOrderValid = true;
            DisplayAction = false;
            PriorityViewFilter = new string[3];
        }

        /// <summary>
        /// Filter the schedule view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Work Center Filter
        /// 2 = Priority Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public void ViewFilter(string filter, int index)
        {
            if (PriorityViewFilter != null)
            {
                PriorityViewFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in PriorityViewFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                if (PriorityView != null)
                {
                    ((DataView)PriorityView.SourceCollection).RowFilter = _filterStr;
                    PriorityView.Refresh();
                }
            }
            else
            {
                PriorityViewFilter = new string[3];
            }
        }

        #region Main ICommand

        public ICommand MainICommand
        {
            get
            {
                if (_main == null)
                {
                    _main = new RelayCommand(MainExecute, MainCanExecute);
                }
                return _main;
            }
        }

        private void MainExecute(object parameter)
        {
            if (parameter?.ToString().Length == 1)
            {
                var _changeRequest = string.Empty;
                switch (parameter.ToString())
                {
                    //Save Command
                    case "S":
                        DisplayAction = true;
                        using (BackgroundWorker bw = new BackgroundWorker())
                        {
                            try
                            {
                                bw.DoWork += new DoWorkEventHandler(
                                    delegate (object sender, DoWorkEventArgs e)
                                    {
                                        foreach (var _wo in (List<WorkOrder>)PriorityView.SourceCollection)
                                        {
                                            if (!string.IsNullOrEmpty(_wo.Priority.ToString()))
                                            {
                                                _changeRequest = M2kCommand.EditRecord("WP", _wo.OrderNumber, 89, _wo.Priority.ToString(), UdArrayCommand.Replace, App.ErpCon);
                                            }
                                            if (!string.IsNullOrEmpty(_wo.Shift.ToString()) && string.IsNullOrEmpty(_changeRequest))
                                            {
                                                _changeRequest = M2kCommand.EditRecord("WP", _wo.OrderNumber, 90, _wo.Shift.ToString(), UdArrayCommand.Replace, App.ErpCon);
                                            }
                                            if (!string.IsNullOrEmpty(_changeRequest))
                                            {
                                                MessageBox.Show(_changeRequest, "ERP Record Error");
                                                return;
                                            }
                                            else
                                            {
                                                var _row = ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{_wo.OrderNumber}'");
                                                var _index = ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row.FirstOrDefault());
                                                ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Shift", _wo.Shift);
                                                ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Priority", _wo.Priority);
                                            }
                                        }
                                        DisplayAction = false;
                                        OnPropertyChanged(nameof(DisplayAction));
                                    });
                                bw.RunWorkerAsync();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        break;
                    //Insert Command
                    case "I":
                        if (!string.IsNullOrEmpty(PriorityInput.ToString()))
                        {
                            _changeRequest = M2kCommand.EditRecord("WP", WorkOrderInput, 89, PriorityInput, UdArrayCommand.Replace, App.ErpCon);
                        }
                        if (!string.IsNullOrEmpty(_changeRequest))
                        {
                            MessageBox.Show(_changeRequest, "ERP Record Error");
                            return;
                        }
                        else
                        {
                            var _row = ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{WorkOrderInput}'");
                            var _index = ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row.FirstOrDefault());
                            ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Priority", PriorityInput);
                        }
                        break;
                    //Organize Command
                    case "O":
                        DisplayAction = true;
                        using (BackgroundWorker bw = new BackgroundWorker())
                        {
                            try
                            {
                                bw.DoWork += new DoWorkEventHandler(
                                    delegate (object sender, DoWorkEventArgs e)
                                    {
                                        var _counter = 1;
                                        foreach (var _wo in (List<WorkOrder>)PriorityView.SourceCollection)
                                        {
                                            if (!string.IsNullOrEmpty(_counter.ToString()))
                                            {
                                                _changeRequest = M2kCommand.EditRecord("WP", _wo.OrderNumber, 89, _wo.Priority.ToString(), UdArrayCommand.Replace, App.ErpCon);
                                            }
                                            if (!string.IsNullOrEmpty(_wo.Shift.ToString()) && string.IsNullOrEmpty(_changeRequest))
                                            {
                                                _changeRequest = M2kCommand.EditRecord("WP", _wo.OrderNumber, 90, _wo.Shift.ToString(), UdArrayCommand.Replace, App.ErpCon);
                                            }
                                            if (!string.IsNullOrEmpty(_changeRequest))
                                            {
                                                MessageBox.Show(_changeRequest, "ERP Record Error");
                                                return;
                                            }
                                            else
                                            {
                                                var _row = ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{_wo.OrderNumber}'");
                                                var _index = ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row.FirstOrDefault());
                                                ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Shift", _wo.Shift);
                                                ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Sched_Priority", _counter.ToString());
                                            }
                                        }
                                        DisplayAction = false;
                                        OnPropertyChanged(nameof(DisplayAction));
                                    });
                                bw.RunWorkerAsync();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                new Commands.ClearPriority().Execute(parameter);
                ((List<WorkOrder>)PriorityView.SourceCollection).Remove(((List<WorkOrder>)PriorityView.SourceCollection).FirstOrDefault(o => o.OrderNumber == parameter.ToString()));
                List<WorkOrder> _tempList = ((List<WorkOrder>)PriorityView.SourceCollection).ToList();
                PriorityView = CollectionViewSource.GetDefaultView(_tempList);
            }
        }
        private bool MainCanExecute(object parameter)
        {
            if (parameter?.ToString().Length == 1)
            {
                switch (parameter.ToString())
                {
                    case "S":
                        return true;
                    case "I":
                        return IsWorkOrderValid && !string.IsNullOrEmpty(PriorityInput);
                    default:
                        return true;
                }
            }
            return true;
        }

        #endregion
    }
}
