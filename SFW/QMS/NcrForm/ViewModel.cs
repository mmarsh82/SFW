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

        private int? _pLoss;
        public string ViewPotentialLoss
        {
            get
            { return _pLoss.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _pLoss = i;
                    if (i > 0)
                    {
                        NcrRevision.PotentialLoss = i;
                        NcrRevision.PotentialValue = i * NcrObject.ProductValue;
                    }
                }
                else
                {
                    _pLoss = null;
                }
                OnPropertyChanged(nameof(ViewPotentialLoss));
            }
        }

        private bool _fromSched;
        public bool FromSchedule
        {
            get
            { return _fromSched; }
            set
            {
                _fromSched = value;
                OnPropertyChanged(nameof(FromSchedule));
            }
        }
        private WorkOrder LoadedWorkOrder;

        RelayCommand _action;
        RelayCommand _xLot;
        RelayCommand _aLot;
        RelayCommand _cancel;

        #endregion

        /// <summary>
        /// ViewModel Default Constructor
        /// </summary>
        public ViewModel(bool isNew)
        {
            IsNewNcr = isNew;
            FromSchedule = false;
            if (NcrObject == null)
            {
                NcrObject = new Ncr(new CrewMember(CurrentUser.FirstName, CurrentUser.LastName));
                NcrRevision = NcrObject.RevisionList.FirstOrDefault();
            }
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
                FromSchedule = true;
                IsNewNcr = true;
                NcrObject = new Ncr(workOrder, new CrewMember(CurrentUser.FirstName, CurrentUser.LastName));
                NcrRevision = NcrObject.RevisionList[0];
                ActionType = "Submit";
                NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection(App.AppSqlCon);
                NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection(App.AppSqlCon);
                DispositionCollection = Ncr.Disposition.GetDispositionCollection(App.AppSqlCon);
                MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(false, false, NcrObject.Site));
                CrewCollection = CrewMember.GetCrewCollection(NcrObject.Site);
                LoadedWorkOrder = workOrder;
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
        /// <param name="fromSched">Optional: Load from schedule</param>
        public ViewModel(Ncr ncr, int revId, bool fromSched = false)
        {
            try
            {
                IsNewNcr = false;
                FromSchedule = fromSched;
                if (FromSchedule)
                {
                    LoadedWorkOrder = new WorkOrder(ncr.OrderId);
                }
                NcrObject = ncr;
                NcrRevision = ncr.RevisionList.FirstOrDefault(o => o.RevisionId == revId);
                ActionType = "Update";
                NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection(App.AppSqlCon);
                SelectedReason = NcrReasonCollection.FirstOrDefault(o => o.Id == NcrRevision.DefectReason.Id);
                NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection(App.AppSqlCon);
                SelectedType = NcrTypeCollection.FirstOrDefault(o => o.Id == NcrRevision.DefectType.Id);
                DispositionCollection = Ncr.Disposition.GetDispositionCollection(App.AppSqlCon);
                SelectedDisposition = DispositionCollection.FirstOrDefault(o => o.Id == NcrRevision.Disposition.Id);
                MachineCollection = new ObservableCollection<Machine>(Machine.GetMachineList(false, false, NcrObject.Site));
                SelectedOriginMachine = MachineCollection.FirstOrDefault(o => o.MachineNumber == NcrRevision.OriginWorkCenter.MachineNumber);
                CrewCollection = CrewMember.GetCrewCollection(NcrObject.Site);
                NcrObject.Reporter = CrewCollection.FirstOrDefault(o => o.IdNumber == NcrObject.Reporter.IdNumber);
                ViewPotentialLoss = NcrRevision.PotentialLoss.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Validates a submission of a new NCR
        /// </summary>
        /// <returns>Validatity as bool</returns>
        public bool ValidateNewSubmission()
        {
            var validObj = NcrObject.IsValidOrder && !string.IsNullOrEmpty(NcrObject.Reporter?.Name);
            var validRev = !string.IsNullOrEmpty(NcrRevision.Disposition?.Description) 
                && ((NcrRevision.IsEscape && !string.IsNullOrEmpty(NcrRevision.OriginWorkCenter?.MachineName) || !NcrRevision.IsEscape))
                && !string.IsNullOrEmpty(NcrRevision.DefectReason?.Description) && !string.IsNullOrEmpty(NcrRevision.DefectType?.Description)
                && !string.IsNullOrEmpty(NcrRevision.Description);
            var validLot = (NcrObject.Part.IsLotTrace && NcrObject.LotList.Where(o => o.Validated).Count() == NcrObject.LotList.Count() && NcrObject.LotList.Count > 0) || !NcrObject.Part.IsLotTrace;
            return validObj && validRev;
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
            if (IsNewNcr)
            {
                NcrObject.NcrId = NcrObject.Submit(App.AppSqlCon);
                ActionType = "Update";
                IsNewNcr = false;
            }
            else
            {
                var newRevId = NcrObject.RevisionList.Count + 1;
                NcrRevision.SubmitDateTime = DateTime.Now;
                NcrRevision.Submitter = new CrewMember(CurrentUser.FirstName, CurrentUser.LastName);
                NcrRevision.Submit(NcrObject.NcrId, newRevId, App.AppSqlCon);
            }
            
            if (!RefreshTimer.Status)
            {
                RefreshTimer.Start();
                RefreshTimer.RefreshTimerTick();
            }
        }
        private bool ActionCanExecute(object parameter) => IsNewNcr ? ValidateNewSubmission() : true;

        #endregion

        #region Remove Lot List Item ICommand

        public ICommand RemoveLotICommand
        {
            get
            {
                if (_xLot == null)
                {
                    _xLot = new RelayCommand(RemoveLotExecute);
                }
                return _xLot;
            }
        }

        private void RemoveLotExecute(object parameter)
        {
            if (parameter == null)
            {
                NcrObject.LotList.Remove(NcrObject.LotList.LastOrDefault());
            }
            else
            {
                NcrObject.LotList.Remove(NcrObject.LotList.FirstOrDefault(o => o.LotNumber == parameter.ToString()));
            }
        }

        #endregion

        #region Remove Lot List Item ICommand

        public ICommand AddLotICommand
        {
            get
            {
                if (_aLot == null)
                {
                    _aLot = new RelayCommand(AddLotExecute);
                }
                return _aLot;
            }
        }

        private void AddLotExecute(object parameter)
        {
            NcrObject.LotList.Add(new Lot());
        }

        #endregion

        #region Cancel Submission ICommand

        public ICommand CancelICommand
        {
            get
            {
                if (_cancel == null)
                {
                    _cancel = new RelayCommand(CancelExecute);
                }
                return _cancel;
            }
        }

        private void CancelExecute(object parameter)
        {
            if(FromSchedule)
            {
                Controls.WorkSpaceDock.SchedDock.Children.RemoveAt(1);
                Controls.WorkSpaceDock.SchedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(LoadedWorkOrder) });
                if (!RefreshTimer.Status)
                {
                    RefreshTimer.Start();
                    RefreshTimer.RefreshTimerTick();
                }
            }
            else
            {
                Controls.WorkSpaceDock.NcrDock.Children.RemoveAt(1);
                var _ncr = new Ncr(Ncr.GetLastNcrId());
                Controls.WorkSpaceDock.NcrDock.Children.Insert(1, new View { DataContext = new ViewModel(_ncr, _ncr.RevisionList.Count()) });
                if (!RefreshTimer.Status)
                {
                    RefreshTimer.Start();
                    RefreshTimer.RefreshTimerTick();
                }
            }
        }

        #endregion
    }
}
