using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SFW.Tools
{
    public class PriorityList_ViewModel : ViewModelBase
    {
        #region Properties

        private IList<WorkOrder> _priList;
        public IList<WorkOrder> PriorityList
        {
            get
            { return _priList; }
            set
            {
                _priList = value;
                OnPropertyChanged(nameof(PriorityList));
            }
        }

        public IList<string> MachineList { get; set; }

        private string _selMach;
        public string SelectedMachine
        {
            get
            { return _selMach; }
            set
            {
                _selMach = value;
                PriorityList = WorkOrder.GetWorkOrderPriList(value);
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
            get
            { return _srch; }
            set
            {
                _srch = value;
                OnPropertyChanged(nameof(SearchInput));
            }
        }

        RelayCommand _main;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PriorityList_ViewModel()
        {
            PriorityList = new List<WorkOrder>();
            MachineList = Machine.GetMachineList(false);
            IsWorkOrderValid = true;
        }

        /// <summary>
        /// Checks for changes in the priority list
        /// </summary>
        /// <returns></returns>
        public bool CheckChanges()
        {
            if(!string.IsNullOrEmpty(SelectedMachine))
            {
                var _tempList = WorkOrder.GetWorkOrderPriList(SelectedMachine);
                var _counter = 0;
                foreach (var pri in _tempList)
                {
                    if (pri.Priority != PriorityList[_counter].Priority)
                    {
                        return true;
                    }
                    _counter++;
                }
                return false; ;
            }
            return false;
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

        }
        private bool MainCanExecute(object parameter)
        {
            if (parameter?.ToString().Length == 1)
            {
                switch (parameter.ToString())
                {
                    case "S":
                        return CheckChanges();
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
