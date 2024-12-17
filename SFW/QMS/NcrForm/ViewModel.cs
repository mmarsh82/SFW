using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.QMS.NcrForm
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private Ncr _ncr;
        public Ncr NcrObject
        {
            get { return _ncr; }
            set
            {
                _ncr = value;
                OnPropertyChanged(nameof(NcrObject));
            }
        }
        private Ncr _origNcrObj;

        private Ncr.Revision _ncrRev;
        public Ncr.Revision NcrRevision
        {
            get { return _ncrRev; }
            set
            {
                _ncrRev = value;
                OnPropertyChanged(nameof(NcrRevision));
            }
        }
        private Ncr.Revision _origNcrRev;

        public ObservableCollection<Ncr.DefectReason> NcrReasonCollection { get; set; }
        public Ncr.DefectReason SelectedReason
        {
            get { return NcrRevision.DefectReason; }
            set
            {
                if (NcrRevision != null)
                {
                    NcrRevision.DefectReason = value;
                }
                OnPropertyChanged(nameof(SelectedReason));
                OnPropertyChanged(nameof(NcrRevision));
            }
        }

        public ObservableCollection<Ncr.DefectType> NcrTypeCollection { get; set; }
        public Ncr.DefectType SelectedType
        {
            get { return NcrRevision.DefectType; }
            set
            {
                if (NcrRevision != null)
                {
                    NcrRevision.DefectType = value;
                }
                OnPropertyChanged(nameof(SelectedType));
                OnPropertyChanged(nameof(NcrRevision));
            }
        }

        public ObservableCollection<Ncr.Disposition> DispositionCollection { get; set; }
        public Ncr.Disposition SelectedDisposition
        {
            get { return NcrRevision.Disposition; }
            set
            {
                if (NcrRevision != null)
                {
                    NcrRevision.Disposition = value;
                }
                OnPropertyChanged(nameof(SelectedDisposition));
                OnPropertyChanged(nameof(NcrRevision));
            }
        }

        public ObservableCollection<Machine> MachineCollection { get; set; }
        public Machine SelectedFoundMachine
        {
            get { return NcrObject.FoundWorkCenter; }
            set
            {
                if (NcrObject != null)
                {
                    NcrObject.FoundWorkCenter = value;
                }
                OnPropertyChanged(nameof(SelectedFoundMachine));
                OnPropertyChanged(nameof(NcrObject));
            }
        }
        public Machine SelectedOriginMachine
        {
            get { return NcrRevision.OriginWorkCenter; }
            set
            {
                if (NcrRevision != null)
                {
                    NcrRevision.OriginWorkCenter = value;
                }
                OnPropertyChanged(nameof(SelectedOriginMachine));
                OnPropertyChanged(nameof(NcrRevision));
            }
        }

        public ObservableCollection<CrewMember> CrewCollection { get; set; }
        public CrewMember SelectedReporter
        {
            get { return NcrObject.Reporter; }
            set
            {
                if (NcrObject != null)
                {
                    NcrObject.Reporter = value;
                }
                OnPropertyChanged(nameof(SelectedReporter));
                OnPropertyChanged(nameof(NcrObject));
            }
        }

        private bool _isNew;
        public bool IsNewNcr
        {
            get { return _isNew; }
            set
            {
                _isNew = value;
                OnPropertyChanged(nameof(IsNewNcr));
            }
        }

        private string _actType;
        public string ActionType
        {
            get { return _actType; }
            set 
            {
                _actType = value;
                OnPropertyChanged(nameof(ActionType));
            }
        }
        RelayCommand _action;

        #endregion

        /// <summary>
        /// ViewModel Default Constructor
        /// </summary>
        public ViewModel(bool isNew)
        {
            if (NcrObject == null)
            {
                NcrObject = new Ncr(new CrewMember(CurrentUser.UserIDNbr, false));
                _origNcrObj = NcrObject;
                NcrRevision = NcrObject.RevisionList.FirstOrDefault();
                _origNcrRev = NcrObject.RevisionList.FirstOrDefault();
            }
            IsNewNcr = isNew;
            ActionType = "Submit";
            if (IsNewNcr)
            {
                if (NcrReasonCollection == null)
                {
                    NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection(App.AppSqlCon);
                }
                if (NcrTypeCollection == null)
                {
                    NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection(App.AppSqlCon);
                }
                if (DispositionCollection == null)
                {
                    DispositionCollection = Ncr.Disposition.GetDispositionCollection(App.AppSqlCon);
                }
                if (MachineCollection == null)
                {
                    MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(false, false, CurrentUser.Facility));
                }
                if (CrewCollection == null)
                {
                    CrewCollection = CrewMember.GetCrewCollection(CurrentUser.Facility);
                }
            }
        }

        /// <summary>
        /// ViewModel Constructor for creating new Ncr on a work order
        /// </summary>
        /// <param name="WorkOrder">WorkOrder Object</param>
        public ViewModel(WorkOrder workOrder)
        {
            try
            {
                NcrObject = _origNcrObj = new Ncr(workOrder);
                NcrRevision = _origNcrRev = NcrObject.RevisionList[0];
                IsNewNcr = true;
                ActionType = "New";
                NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection(App.AppSqlCon);
                NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection(App.AppSqlCon);
                DispositionCollection = Ncr.Disposition.GetDispositionCollection(App.AppSqlCon);
                MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(false, false, NcrObject.Site));
                SelectedFoundMachine = MachineCollection.FirstOrDefault(o => o.MachineNumber == NcrObject.FoundWorkCenter.MachineNumber);
                CrewCollection = CrewMember.GetCrewCollection(NcrObject.Site);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ViewModel Constructor for Ncr's from an existing Ncr
        /// </summary>
        /// <param name="ncr">Ncr Object</param>
        /// <param name="revId">Ncr Revision ID to load</param>
        public ViewModel(Ncr ncr, int revId)
        {
            try
            {
                NcrObject = _origNcrObj = ncr;
                NcrRevision = _origNcrRev = ncr.RevisionList.FirstOrDefault(o => o.RevisionId == revId);
                IsNewNcr = false;
                ActionType = "Update";
                NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection(App.AppSqlCon);
                SelectedReason = NcrReasonCollection.FirstOrDefault(o => o.Id == NcrRevision.DefectReason.Id);
                NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection(App.AppSqlCon);
                SelectedType = NcrTypeCollection.FirstOrDefault(o => o.Id == NcrRevision.DefectType.Id);
                DispositionCollection = Ncr.Disposition.GetDispositionCollection(App.AppSqlCon);
                SelectedDisposition = DispositionCollection.FirstOrDefault(o => o.Id == NcrRevision.Disposition.Id);
                MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(false, false, NcrObject.Site));
                SelectedFoundMachine = MachineCollection.FirstOrDefault(o => o.MachineNumber == NcrObject.FoundWorkCenter.MachineNumber);
                SelectedOriginMachine = MachineCollection.FirstOrDefault(o => o.MachineNumber == NcrRevision.OriginWorkCenter.MachineNumber);
                CrewCollection = CrewMember.GetCrewCollection(NcrObject.Site);
                SelectedReporter = CrewCollection.FirstOrDefault(o => o.IdNumber == NcrObject.Reporter.IdNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Submission/Update ICommand

        public ICommand ActionICommand
        {
            get
            {
                if (_action == null)
                {
                    _action = new RelayCommand(ActionExecute, ActionCanExecute);
                }
                return _action;
            }
        }

        private void ActionExecute(object parameter)
        {
            NcrObject.Submit(App.AppSqlCon);
            ActionType = "Update";
            _origNcrObj = NcrObject;
            _origNcrRev = NcrRevision;
            if (!RefreshTimer.Status)
            {
                RefreshTimer.Start();
            }
        }
        private bool ActionCanExecute(object parameter)
        {
            return NcrObject != _origNcrObj && NcrRevision != _origNcrRev;
        }

        #endregion
    }
}
