using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace SFW.Tools
{
    public class CrewList_ViewModel : ViewModelBase
    {
        #region Properties

        public ObservableCollection<Machine> MachineCollection { get; set; }
        public ObservableCollection<CrewMember> CrewCollection { get; set; }

        public bool NoData { get; set; }

        private bool _isLoading { get; set; }
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
                if (_date != value)
                {
                    if (CrewMember.IsPublished(ManagerId, value, App.AppSqlCon))
                    {
                        CrewCollection = new ObservableCollection<CrewMember>(CrewMember.GetCrewLaborList(ManagerId, value, App.AppSqlCon));
                        _actionType = 'U';
                    }
                    else
                    {
                        CrewCollection = new ObservableCollection<CrewMember>();
                        foreach (var _report in CurrentUser.DirectReports)
                        {
                            var _surName = _report.Value.Split(',')[0];
                            var _giveName = _report.Value.Split(',')[1];
                            CrewCollection.Add(new CrewMember(_giveName, _surName, true));
                        }
                        _actionType = 'S';
                    }
                    NoData = CrewCollection.Count == 0;
                    Published = !NoData && _actionType != 'S';
                    _date = value;
                }
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

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CrewList_ViewModel()
        {
            var _tempCrewMember = new CrewMember(CurrentUser.FirstName, CurrentUser.LastName, true);
            Shift = _tempCrewMember.Shift;
            ManagerId = _tempCrewMember.IdNumber;
            CanEdit = CurrentUser.IsSupervisor && CurrentUser.DirectReports.Count > 0;
            _isLoading = false;
            SelectedDate = DateTime.Today;
            if (MachineCollection == null)
            {
                MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(false, false, 1));
            }
        }

        #region Submit Command ICommand

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
            var _msgText = CrewMember.PublishLabor(CrewCollection.ToList(), _actionType, ManagerId, App.AppSqlCon);
            MessageBox.Show(_msgText, "Publishing Message", MessageBoxButton.OK, MessageBoxImage.Information);
            if (_msgText.Contains("Successfully"))
            {
                _actionType = 'U';
                Published = true;
            }
        }
        private bool ActionCommandCanExecute(object parameter) => true;

        #endregion

    }
}
