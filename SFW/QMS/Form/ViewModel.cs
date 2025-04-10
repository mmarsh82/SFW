using SFW.Controls;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.QMS.Form
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private QmsForm _form;
        public QmsForm FormObject
        {
            get { return _form; }
            set
            {
                _form = value;
                OnPropertyChanged(nameof(FormObject));
            }
        }

        private QmsForm.Revision _frmRev;
        public QmsForm.Revision FormRevision
        {
            get { return _frmRev; }
            set
            {
                _frmRev = value;
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        public ObservableCollection<QmsForm.DefectReason> DefectReasonCollection { get; set; }
        public QmsForm.DefectReason SelectedReason
        {
            get { return FormRevision.DefectReason; }
            set
            {
                if (FormRevision != null)
                {
                    FormRevision.DefectReason = value;
                }
                OnPropertyChanged(nameof(SelectedReason));
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        public ObservableCollection<QmsForm.DefectType> DefectTypeCollection { get; set; }
        public QmsForm.DefectType SelectedType
        {
            get { return FormRevision.DefectType; }
            set
            {
                if (FormRevision != null)
                {
                    FormRevision.DefectType = value;
                }
                OnPropertyChanged(nameof(SelectedType));
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        public ObservableCollection<QmsForm.Disposition> DispositionCollection { get; set; }
        public QmsForm.Disposition SelectedDisposition
        {
            get { return FormRevision.Disposition; }
            set
            {
                if (FormRevision != null)
                {
                    FormRevision.Disposition = value;
                }
                OnPropertyChanged(nameof(SelectedDisposition));
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        public ObservableCollection<Supplier> SupplierCollection { get; set; }
        public Supplier SelectedSupplier
        {
            get { return FormRevision.FormSupplier; }
            set
            {
                if (FormRevision != null)
                {
                    FormRevision.FormSupplier = value;
                }
                OnPropertyChanged(nameof(SelectedSupplier));
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        public ObservableCollection<CrewMember> CrewCollection { get; set; }

        private bool _isNew;
        public bool IsNewForm
        {
            get { return _isNew; }
            set
            {
                _isNew = value;
                OnPropertyChanged(nameof(IsNewForm));
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
                        FormRevision.PotentialLoss = i;
                        FormRevision.PotentialValue = i * FormObject.ProductValue;
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
        public ViewModel(QmsForm frmObj, bool fromSched, bool isNew, QmsForm.FormType frmType)
        {
            IsNewForm = isNew;
            FromSchedule = fromSched;
            if (FormObject == null)
            {
                FormObject = new QmsForm(new CrewMember(CurrentUser.ErpId, false), frmType);
                FormRevision = FormObject.RevisionList.FirstOrDefault();
            }
            else
            {
                FormObject = new QmsForm(frmObj);
                FormRevision = FormObject.RevisionList.FirstOrDefault();
            }
            ActionType = "Submit";
            if (IsNewForm)
            {
                if (DefectReasonCollection == null)
                {
                    DefectReasonCollection = QmsForm.DefectReason.GetDefectReasonCollection();
                }
                if (DefectTypeCollection == null)
                {
                    DefectTypeCollection = QmsForm.DefectType.GetDefectTypeCollection();
                }
                if (DispositionCollection == null)
                {
                    DispositionCollection = Model.QmsForm.Disposition.GetDispositionCollection();
                }
                if (CrewCollection == null)
                {
                    CrewCollection = CrewMember.GetCrewCollection(CurrentUser.Facility);
                }
                if (SupplierCollection == null)
                {
                    SupplierCollection = new ObservableCollection<Supplier>(Supplier.GetSupplierList());
                }
            }
        }

        /// <summary>
        /// ViewModel Constructor for creating new Ncr on a work order
        /// </summary>
        /// <param name="WorkOrder">WorkOrder Object</param>
        /// <param name="frmType">Form type to create</param>
        public ViewModel(WorkOrder workOrder, QmsForm.FormType frmType)
        {
            try
            {
                FromSchedule = true;
                IsNewForm = true;
                FormObject = new QmsForm(workOrder, new CrewMember(CurrentUser.ErpId, false), frmType);
                FormRevision = FormObject.RevisionList[0];
                ActionType = "Submit";
                CrewCollection = CrewMember.GetCrewCollection(FormObject.Site);
                LoadedWorkOrder = workOrder;
                DefectReasonCollection = QmsForm.DefectReason.GetDefectReasonCollection();
                DefectTypeCollection = QmsForm.DefectType.GetDefectTypeCollection();
                DispositionCollection = QmsForm.Disposition.GetDispositionCollection();
                SupplierCollection = new ObservableCollection<Supplier>(Supplier.GetSupplierList());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ViewModel Constructor for Ncr's from an existing Ncr
        /// </summary>
        /// <param name="frm">Form Object</param>
        /// <param name="revId">Form Revision ID to load</param>
        /// <param name="fromSched">Optional: Load from schedule</param>
        public ViewModel(QmsForm frm, int revId, bool fromSched = false)
        {
            try
            {
                IsNewForm = false;
                FromSchedule = fromSched;
                if (FromSchedule)
                {
                    LoadedWorkOrder = new WorkOrder(frm.OrderId);
                }
                FormObject = frm;
                FormRevision = frm.RevisionList.FirstOrDefault(o => o.RevisionId == revId);
                ActionType = "Update";
                CrewCollection = CrewMember.GetCrewCollection(FormObject.Site);
                FormObject.Reporter = CrewCollection.FirstOrDefault(o => o.ErpId == FormObject.Reporter.ErpId);
                ViewPotentialLoss = FormRevision.PotentialLoss.ToString();
                DefectReasonCollection = QmsForm.DefectReason.GetDefectReasonCollection();
                SelectedReason = DefectReasonCollection.FirstOrDefault(o => o.Id == FormRevision.DefectReason.Id);
                DefectTypeCollection = QmsForm.DefectType.GetDefectTypeCollection();
                SelectedType = DefectTypeCollection.FirstOrDefault(o => o.Id == FormRevision.DefectType.Id);
                DispositionCollection = QmsForm.Disposition.GetDispositionCollection();
                SelectedDisposition = DispositionCollection.FirstOrDefault(o => o.Id == FormRevision.Disposition.Id);
                SupplierCollection = new ObservableCollection<Supplier>(Supplier.GetSupplierList());
                if (FormRevision.FormSupplier != null)
                {
                    SelectedSupplier = SupplierCollection.FirstOrDefault(o => o.SupplierId == FormRevision.FormSupplier.SupplierId);
                }
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    try
                    {
                        bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            var _actuals = QmsForm.GetActuals(FormObject.FormId, App.AppSqlCon);
                            if (_actuals.Count > 0)
                            {
                                FormRevision.ActualLoss = _actuals.FirstOrDefault().Key;
                                FormRevision.ActualCost = _actuals.FirstOrDefault().Value;
                                OnPropertyChanged(nameof(FormRevision));
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
            var validObj = FormObject.IsValidOrder && !string.IsNullOrEmpty(FormObject.Reporter?.Name);
            var validRev = !string.IsNullOrEmpty(FormRevision.Disposition?.Description)
                && !FormObject.IsEscape || (FormObject.IsEscape && ((FormObject.Part.IsLotTrace && FormObject.LotList.Count(o => o.Validated) > 0) || !FormObject.Part.IsLotTrace))
                && !string.IsNullOrEmpty(FormRevision.DefectReason?.Description) && !string.IsNullOrEmpty(FormRevision.DefectType?.Description)
                && !string.IsNullOrEmpty(FormRevision.Description);
            var validLot = true;
            if (FormObject.Part != null && FormObject.LotList != null && FormObject.Part.IsLotTrace && FormObject.LotList.Count(o => o.Validated) > 0)
            {
                validLot = FormObject.LotList.Where(o => o.Validated).Count() == FormObject.LotList.Count();
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
            if (IsNewForm)
            {
                FormObject.FormId = FormObject.Submit(App.AppSqlCon);
                ActionType = "Update";
                IsNewForm = false;

            }
            else
            {
                var newRevId = FormObject.RevisionList.Count + 1;
                FormRevision.SubmitDateTime = DateTime.Now;
                FormRevision.Submitter = new CrewMember(CurrentUser.ErpId, false);
                FormRevision.Submit(FormObject.FormId, newRevId, App.AppSqlCon);
                FormObject.SubmitLots(App.AppSqlCon);
            }
            
            if (!RefreshTimer.Status)
            {
                RefreshTimer.Start();
                RefreshTimer.RefreshTimerTick();
            }
        }
        private bool ActionCanExecute(object parameter) => IsNewForm ? ValidateNewSubmission() : true;

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
                FormObject.LotList.Remove(FormObject.LotList.LastOrDefault());
            }
            else
            {
                FormObject.LotList.Remove(FormObject.LotList.FirstOrDefault(o => o.LotNumber == parameter.ToString()));
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
            FormObject.LotList.Add(new Lot());
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
                WorkSpaceDock.SchedDock.Children.RemoveAt(1);
                WorkSpaceDock.SchedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(LoadedWorkOrder) });
                if (!RefreshTimer.Status)
                {
                    RefreshTimer.Start();
                }
            }
            else
            {
                WorkSpaceDock.QmsFormDock.Children.RemoveAt(1);
                var _ncr = new Model.QmsForm(Model.QmsForm.GetLastNcrId());
                WorkSpaceDock.QmsFormDock.Children.Insert(1, new View { DataContext = new ViewModel(_ncr, _ncr.RevisionList.Count()) });
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
            FormObject.PhotoCollection.Remove(parameter.ToString());
            if (FormObject.FormId > 0)
            {
                Model.QmsForm.DeletePhotoPath(FormObject.FormId, parameter.ToString(), App.AppSqlCon);
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
                foreach (var _rev in FormObject.RevisionList)
                {
                    _rev.Current = false;
                }
                FormObject.RevisionList.FirstOrDefault(o => o.RevisionId == i).Current = true;
                FormRevision = FormObject.RevisionList.FirstOrDefault(o => o.RevisionId == i);
                OnPropertyChanged(nameof(FormRevision));
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
                FormRevision.Disposition = new Model.QmsForm.Disposition(7, "Void", "Voided");
                FormRevision.Submit(FormObject.FormId, FormObject.RevisionList.Count() + 1, App.AppSqlCon);
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
            FormObject.FormId = 0;
            FormObject.TempId = int.Parse(DateTime.Now.ToString("MMddmmss"));
            FormObject.RevisionList.Clear();
            FormRevision.RevisionId = 1;
            FormObject.RevisionList.Add(FormRevision);
            if(FromSchedule)
            {
                WorkSpaceDock.UpdateChildDock(1, 1, new ViewModel(FormObject, true, true, FormRevision.RevFormType));
            }
            else
            {
                WorkSpaceDock.UpdateChildDock(9, 1, new ViewModel(FormObject, false, true, FormRevision.RevFormType));
            }
            foreach (var _photo in FormObject.PhotoCollection)
            {

            }
        }

        #endregion

    }
}
