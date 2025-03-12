using SFW.Controls;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        RelayCommand _xPic;
        RelayCommand _chgRev;
        RelayCommand _void;
        RelayCommand _clone;

        #endregion

        /// <summary>
        /// ViewModel Default Constructor
        /// </summary>
        public ViewModel(Ncr ncrObj, bool fromSched, bool isNew)
        {
            IsNewNcr = isNew;
            FromSchedule = fromSched;
            if (NcrObject == null)
            {
                NcrObject = new Ncr(new CrewMember(CurrentUser.FirstName, CurrentUser.LastName, false));
                NcrRevision = NcrObject.RevisionList.FirstOrDefault();
            }
            else
            {
                NcrObject = new Ncr(ncrObj);
                NcrRevision = NcrObject.RevisionList.FirstOrDefault();
            }
            ActionType = "Submit";
            if (IsNewNcr)
            {
                if (NcrReasonCollection == null)
                {
                    NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection();
                }
                if (NcrTypeCollection == null)
                {
                    NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection();
                }
                if (DispositionCollection == null)
                {
                    DispositionCollection = Ncr.Disposition.GetDispositionCollection();
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
                NcrObject = new Ncr(workOrder, new CrewMember(CurrentUser.FirstName, CurrentUser.LastName, false));
                NcrRevision = NcrObject.RevisionList[0];
                ActionType = "Submit";
                CrewCollection = CrewMember.GetCrewCollection(NcrObject.Site);
                LoadedWorkOrder = workOrder;
                NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection();
                NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection();
                DispositionCollection = Ncr.Disposition.GetDispositionCollection();
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
                CrewCollection = CrewMember.GetCrewCollection(NcrObject.Site);
                NcrObject.Reporter = CrewCollection.FirstOrDefault(o => o.IdNumber == NcrObject.Reporter.IdNumber);
                ViewPotentialLoss = NcrRevision.PotentialLoss.ToString();
                NcrReasonCollection = Ncr.DefectReason.GetDefectReasonCollection();
                SelectedReason = NcrReasonCollection.FirstOrDefault(o => o.Id == NcrRevision.DefectReason.Id);
                NcrTypeCollection = Ncr.DefectType.GetDefectTypeCollection();
                SelectedType = NcrTypeCollection.FirstOrDefault(o => o.Id == NcrRevision.DefectType.Id);
                DispositionCollection = Ncr.Disposition.GetDispositionCollection();
                SelectedDisposition = DispositionCollection.FirstOrDefault(o => o.Id == NcrRevision.Disposition.Id);
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    try
                    {
                        bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            var _actuals = Ncr.GetActuals(NcrObject.NcrId, App.AppSqlCon);
                            if (_actuals.Count > 0)
                            {
                                NcrRevision.ActualLoss = _actuals.FirstOrDefault().Key;
                                NcrRevision.ActualCost = _actuals.FirstOrDefault().Value;
                                OnPropertyChanged(nameof(NcrRevision));
                            }
                        });
                        bw.RunWorkerAsync();
                    }
                    catch (Exception)
                    {

                    }
                }
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
                && !NcrObject.IsEscape || (NcrObject.IsEscape && ((NcrObject.Part.IsLotTrace && NcrObject.LotList.Count(o => o.Validated) > 0) || !NcrObject.Part.IsLotTrace))
                && !string.IsNullOrEmpty(NcrRevision.DefectReason?.Description) && !string.IsNullOrEmpty(NcrRevision.DefectType?.Description)
                && !string.IsNullOrEmpty(NcrRevision.Description);
            var validLot = true;
            if (NcrObject.Part != null && NcrObject.LotList != null && NcrObject.Part.IsLotTrace && NcrObject.LotList.Count(o => o.Validated) > 0)
            {
                validLot = NcrObject.LotList.Where(o => o.Validated).Count() == NcrObject.LotList.Count();
            }
            return validObj && validRev && validLot;
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
                NcrRevision.Submitter = new CrewMember(CurrentUser.FirstName, CurrentUser.LastName, false);
                NcrRevision.Submit(NcrObject.NcrId, newRevId, App.AppSqlCon);
                NcrObject.SubmitLots(App.AppSqlCon);
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
                }
            }
        }

        #endregion

        #region Remove Photo ICommand

        public ICommand RemovePhotoICommand
        {
            get
            {
                if (_xPic == null)
                {
                    _xPic = new RelayCommand(RemovePhotoExecute);
                }
                return _xPic;
            }
        }

        private void RemovePhotoExecute(object parameter)
        {
            NcrObject.PhotoCollection.Remove(parameter.ToString());
            if (NcrObject.NcrId > 0)
            {
                Ncr.DeletePhotoPath(NcrObject.NcrId, parameter.ToString(), App.AppSqlCon);
            }
        }

        #endregion

        #region Change Revision ICommand

        public ICommand ChangeRevICommand
        {
            get
            {
                if (_chgRev == null)
                {
                    _chgRev = new RelayCommand(ChangeRevExecute);
                }
                return _chgRev;
            }
        }

        private void ChangeRevExecute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int i))
            {
                foreach (var _rev in NcrObject.RevisionList)
                {
                    _rev.Current = false;
                }
                NcrObject.RevisionList.FirstOrDefault(o => o.RevisionId == i).Current = true;
                NcrRevision = NcrObject.RevisionList.FirstOrDefault(o => o.RevisionId == i);
                OnPropertyChanged(nameof(NcrRevision));
            }
        }

        #endregion

        #region Void NCR ICommand

        public ICommand VoidICommand
        {
            get
            {
                if (_void == null)
                {
                    _void = new RelayCommand(VoidExecute);
                }
                return _void;
            }
        }

        private void VoidExecute(object parameter)
        {
            var _result = MessageBox.Show("Are you sure you want to void this NCR?\nOnce Voided only IT can bring it back.", "Void NCR", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (_result == MessageBoxResult.Yes)
            {
                NcrRevision.Disposition = new Ncr.Disposition(7, "Void", "Voided");
                NcrRevision.Submit(NcrObject.NcrId, NcrObject.RevisionList.Count() + 1, App.AppSqlCon);
            }
        }

        #endregion

        #region Clone NCR ICommand

        public ICommand CloneICommand
        {
            get
            {
                if (_clone == null)
                {
                    _clone = new RelayCommand(CloneExecute);
                }
                return _clone;
            }
        }

        private void CloneExecute(object parameter)
        {
            NcrObject.NcrId = 0;
            NcrObject.TempId = int.Parse(DateTime.Now.ToString("MMddmmss"));
            NcrObject.RevisionList.Clear();
            NcrRevision.RevisionId = 1;
            NcrObject.RevisionList.Add(NcrRevision);
            if(FromSchedule)
            {
                WorkSpaceDock.UpdateChildDock(1, 1, new ViewModel(NcrObject, true, true));
            }
            else
            {
                WorkSpaceDock.UpdateChildDock(9, 1, new ViewModel(NcrObject, false, true));
            }
            foreach (var _photo in NcrObject.PhotoCollection)
            {

            }
        }

        #endregion

    }
}
