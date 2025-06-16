using SFW.Helpers;
using SFW.Model.Production;
using SFW.Model.Management;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Tools
{
    public class CrewList_ViewModel : ViewModelBase
    {
        #region Properties

        public ObservableCollection<Machine> MachineCollection { get; set; }
        public ObservableCollection<Employee> CrewCollection { get; set; }

        public bool NoData { get; set; }

        private char _actionType { get; set; }

        private int _shift;
        public int Shift
        {
            get
            { return _shift; }
            set
            {
                _shift = value;
                OnPropertyChanged(nameof(Shift));
            }
        }

        private string _manager;
        public string ManagerId
        {
            get
            { return _manager; }
            set
            {
                _manager = value;
                OnPropertyChanged(nameof(ManagerId));
            }
        }

        private DateTime _date;
        public DateTime SelectedDate
        {
            get
            { return _date; }
            set
            {
                if (!CanEdit)
                {
                    Shift = Shift == 0 ? 1 : Shift;
                    OnPropertyChanged(nameof(Shift));
                    CrewCollection = new ObservableCollection<Employee>(Employee.GetLaborList(Shift, value, App.AppSqlCon));
                    NoData = CrewCollection.Count == 0;
                    OnPropertyChanged(nameof(NoData));
                    OnPropertyChanged(nameof(CrewCollection));
                }
                else if (_date != value)
                {
                    if (Employee.IsPublished(ManagerId, value, App.AppSqlCon))
                    {
                        CrewCollection = new ObservableCollection<Employee>(Employee.GetLaborList(ManagerId, value, App.AppSqlCon));
                        _actionType = 'U';
                    }
                    else
                    {
                        CrewCollection = new ObservableCollection<Employee>();
                        foreach (var _report in CurrentUser.DirectReports)
                        {
                            var _tempCrew = new Employee(_report.Key, true);
                            if (!string.IsNullOrEmpty(_tempCrew.Name))
                            {
                                CrewCollection.Add(_tempCrew);
                            }
                        }
                        _actionType = 'S';
                    }
                    NoData = CrewCollection.Count == 0;
                    Published = !NoData && _actionType != 'S';
                }
                _date = value;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        private bool _edit;
        public bool CanEdit
        {
            get
            { return _edit; }
            set
            {
                _edit = value;
                OnPropertyChanged(nameof(CanEdit));
            }
        }

        private bool _publish;
        public bool Published
        {
            get
            { return _publish; }
            set
            {
                _publish = value;
                OnPropertyChanged(nameof(Published));
            }
        }

        private RelayCommand _submitICommand;
        private RelayCommand _viewICommand;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CrewList_ViewModel()
        {
            var _tempCrewMember = new Employee(CurrentUser.ErpId, true);
            Shift = _tempCrewMember.Shift;
            ManagerId = _tempCrewMember.ErpId;
            CanEdit = CurrentUser.IsSupervisor && CurrentUser.DirectReports.Count > 0;
            SelectedDate = DateTime.Today;
            if (MachineCollection == null)
            {
                MachineCollection = new ObservableCollection<Machine>(Machine.GetList(false, false, 1));
            }
        }

        #region Submit ICommand

        public ICommand SubmitICommand
        {
            get
            {
                if (_submitICommand == null)
                {
                    _submitICommand = new RelayCommand(ActionCommandExecute, ActionCommandCanExecute);
                }
                return _submitICommand;
            }
        }

        private void ActionCommandExecute(object parameter)
        {
            var _response = Employee.PublishLabor(CrewCollection.ToList(), _actionType, ManagerId, SelectedDate, App.AppSqlCon);
            MessageBox.Show(_response.FirstOrDefault().Value, "Publishing Message", MessageBoxButton.OK, MessageBoxImage.Information);
            if (_response.FirstOrDefault().Key)
            {
                _actionType = 'U';
                Published = true;
            }
        }
        private bool ActionCommandCanExecute(object parameter) => true;

        #endregion

        #region Shift View ICommand

        public ICommand ShiftViewICommand
        {
            get
            {
                if (_viewICommand == null)
                {
                    _viewICommand = new RelayCommand(ViewCommandExecute, ViewCommandCanExecute);
                }
                return _viewICommand;
            }
        }

        private void ViewCommandExecute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int i))
            {
                CrewCollection = new ObservableCollection<Employee>(Employee.GetLaborList(i, SelectedDate, App.AppSqlCon));
                NoData = CrewCollection.Count == 0;
                Shift = i;
                OnPropertyChanged(nameof(Shift));
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(CrewCollection));
            }
        }
        private bool ViewCommandCanExecute(object parameter) => true;

        #endregion

    }
}
