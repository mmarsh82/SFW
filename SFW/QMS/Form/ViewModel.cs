using SFW.Controls;
using SFW.Helpers;
using SFW.Model.Management;
using SFW.Model.Production;
using SFW.Model.Quality;
using System;
using System.Collections.ObjectModel;
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

        private Revision _frmRev;
        public Revision FormRevision
        {
            get { return _frmRev; }
            set
            {
                _frmRev = value;
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        #region Defect Work flow

        public ObservableCollection<Defect> DefectCollection { get; set; }
        public Defect SelectedDefect
        {
            get { return FormRevision.DefectSubType; }
            set
            {
                if (FormRevision != null)
                {
                    if (value != null && FormRevision.DefectSubType?.Id != value.Id)
                    {
                        FormRevision.DefectSubType = value;
                        TypeCollection = Category.GetCategoryCollection(value.Id);
                        OnPropertyChanged(nameof(TypeCollection));
                        if (TypeCollection != null && TypeCollection.Count == 1)
                        {
                            SelectedType = TypeCollection[0];
                        }
                        else
                        {
                            ReasonCollection = null;
                            SelectedReason = null;
                            FormRevision.DefectReason = null;
                        }
                        OnPropertyChanged(nameof(ShowType));
                        OnPropertyChanged(nameof(ShowTypeCollection));
                        OnPropertyChanged(nameof(ShowReasonCollection));
                        OnPropertyChanged(nameof(ShowReason));

                    }
                }
                FormRevision.DefectSubType = value;
                OnPropertyChanged(nameof(SelectedDefect));
                OnPropertyChanged(nameof(FormRevision));
            }
        }

        #endregion

        #region Type Work flow

        public ObservableCollection<Category> TypeCollection { get; set; }
        public Category SelectedType
        {
            get { return FormRevision.DefectType; }
            set
            {
                if (FormRevision != null)
                {
                    if (value != null && FormRevision.DefectType?.Id != value.Id)
                    {
                        FormRevision.DefectType = value;
                        FormRevision.RevFormType = value == null ? Model.Quality.FormType.NCR : value.QmsFormType;
                        ReasonCollection = Reason.GetReasonCollection(value.Id, FormRevision.DefectSubType.Id);
                        OnPropertyChanged(nameof(ReasonCollection));
                        if (ReasonCollection != null && ReasonCollection.Count == 1)
                        {
                            SelectedReason = ReasonCollection[0];
                        }
                        OnPropertyChanged(nameof(ShowReason));
                        OnPropertyChanged(nameof(ShowReasonCollection));
                        SupplierCollection.Clear();
                        if (value.QmsFormType == Model.Quality.FormType.SCAR)
                        {
                            SupplierCollection = new ObservableCollection<Supplier>(Supplier.GetSupplierList(true));
                        }
                        OnPropertyChanged(nameof(SupplierCollection));
                        OnPropertyChanged(nameof(ShowSupplier));
                        OnPropertyChanged(nameof(ShowSupplierCollection));
                    }
                }
                FormRevision.DefectType = value;
                FormRevision.RevFormType = value == null ? Model.Quality.FormType.NCR : value.QmsFormType;
                OnPropertyChanged(nameof(SelectedType));
                OnPropertyChanged(nameof(FormRevision));
            }
        }
        public bool ShowType { get { return !string.IsNullOrEmpty(SelectedDefect?.Description); } }
        public bool ShowTypeCollection { get { return CurrentUser.IsQuality && TypeCollection?.Count > 1; } }

        #endregion

        #region Reason Work flow

        public ObservableCollection<Reason> ReasonCollection { get; set; }
        public Reason SelectedReason
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
        public bool ShowReason { get { return !string.IsNullOrEmpty(SelectedType?.Description); } }
        public bool ShowReasonCollection { get { return CurrentUser.IsQuality && ReasonCollection?.Count > 1; } }

        #endregion

        public ObservableCollection<Disposition> DispositionCollection { get; set; }
        public Disposition SelectedDisposition
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
                    if (value != null)
                    {
                        ReasonCollection = Reason.GetReasonCollection(FormRevision.FormSupplier.Classification);
                        SelectedReason = ReasonCollection[0];
                    }
                }
                OnPropertyChanged(nameof(SelectedSupplier));
                OnPropertyChanged(nameof(FormRevision));
                OnPropertyChanged(nameof(ShowSupplier));
                OnPropertyChanged(nameof(ShowSupplierCollection));
                OnPropertyChanged(nameof(SupplierCollection));
                OnPropertyChanged(nameof(ShowReasonCollection));
                OnPropertyChanged(nameof(ReasonCollection));
                OnPropertyChanged(nameof(ShowTypeCollection));
                OnPropertyChanged(nameof(TypeCollection));
            }
        }
        public bool ShowSupplier { get { return FormRevision?.RevFormType == Model.Quality.FormType.SCAR; } }
        public bool ShowSupplierCollection { get { return CurrentUser.IsQuality && FormRevision?.RevFormType == Model.Quality.FormType.SCAR; } }

        public ObservableCollection<Employee> CrewCollection { get; set; }

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

        #endregion

        /// <summary>
        /// ViewModel Default Constructor
        /// </summary>
        public ViewModel(QmsForm frmObj, bool fromSched, bool isNew, Model.Quality.FormType frmType)
        {
            IsNewForm = isNew;
            FromSchedule = fromSched;
            if (FormObject == null)
            {
                FormObject = new QmsForm(new Employee(CurrentUser.ErpId, false), frmType);
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
                if (ReasonCollection == null)
                {
                    ReasonCollection = Reason.GetReasonCollection();
                }
                if (DefectCollection == null)
                {
                    DefectCollection = Defect.GetCollection();
                }
                if (TypeCollection == null)
                {
                    TypeCollection = Category.GetCategoryCollection();
                }
                if (DispositionCollection == null)
                {
                    DispositionCollection = Disposition.GetDispositionCollection();
                }
                if (CrewCollection == null)
                {
                    CrewCollection = Employee.GetCollection(CurrentUser.Facility);
                }
                if (SupplierCollection == null)
                {
                    SupplierCollection = new ObservableCollection<Supplier>(Supplier.GetSupplierList(true));
                }
            }
        }

        /// <summary>
        /// ViewModel Constructor for creating new Ncr on a work order
        /// </summary>
        /// <param name="WorkOrder">WorkOrder Object</param>
        /// <param name="frmType">Form type to create</param>
        public ViewModel(WorkOrder workOrder, Model.Quality.FormType frmType)
        {
            try
            {
                FromSchedule = true;
                IsNewForm = true;
                FormObject = new QmsForm(workOrder, new Employee(CurrentUser.ErpId, false), frmType);
                FormRevision = FormObject.RevisionList[0];
                ActionType = "Submit";
                CrewCollection = Employee.GetCollection(FormObject.Site);
                LoadedWorkOrder = workOrder;
                DefectCollection = Defect.GetCollection();
                DispositionCollection = Disposition.GetDispositionCollection();
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
                CrewCollection = Employee.GetCollection(FormObject.Site);
                FormObject.Reporter = CrewCollection.FirstOrDefault(o => o.ErpId == FormObject.Reporter.ErpId);
                DefectCollection = Defect.GetCollection();
                SelectedDefect = DefectCollection.FirstOrDefault(o => o.Id == FormRevision.DefectSubType.Id);
                TypeCollection = Category.GetCategoryCollection(SelectedDefect.Id);
                SelectedType = TypeCollection.FirstOrDefault(o => o.Id == FormRevision.DefectType.Id);
                ReasonCollection = FormRevision.RevFormType == Model.Quality.FormType.NCR
                    ? Reason.GetReasonCollection(FormRevision.DefectType.Id, FormRevision.DefectSubType.Id)
                    : Reason.GetReasonCollection(FormRevision.FormSupplier.Classification);
                SelectedReason = ReasonCollection.FirstOrDefault(o => o.Id == FormRevision.DefectReason?.Id);
                DispositionCollection = Disposition.GetDispositionCollection();
                SelectedDisposition = DispositionCollection.FirstOrDefault(o => o.Id == FormRevision.Disposition.Id);
                if (FormRevision.RevFormType == Model.Quality.FormType.SCAR)
                {
                    SupplierCollection = new ObservableCollection<Supplier>(Supplier.GetSupplierList(true));
                    SelectedSupplier = SupplierCollection.FirstOrDefault(o => o.Id == FormRevision.FormSupplier.Id);
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
                FormRevision.Submitter = new Employee(CurrentUser.ErpId, false);
                FormRevision.Submit(FormObject.FormId, newRevId, App.AppSqlCon);
                FormObject.SubmitLots(App.AppSqlCon);
            }

            if (ApplicationTimer.Status == TimerState.Paused)
            {
                ApplicationTimer.Resume();
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
            FormObject.LotList.Add(new Model.Product.Lot());
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
            }
            else
            {
                WorkSpaceDock.QmsFormDock.Children.RemoveAt(1);
                var _ncr = new QmsForm(QmsForm.GetLastNcrId());
                WorkSpaceDock.QmsFormDock.Children.Insert(1, new View { DataContext = new ViewModel(_ncr, _ncr.RevisionList.Count()) });
            }
            if (ApplicationTimer.Status == TimerState.Paused)
            {
                ApplicationTimer.Resume();
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
                QmsForm.DeletePhotoPath(FormObject.FormId, parameter.ToString(), App.AppSqlCon);
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
                FormRevision.Disposition = new Disposition(7, "Void", "Voided");
                FormRevision.Submit(FormObject.FormId, FormObject.RevisionList.Count() + 1, App.AppSqlCon);
            }
        }

        #endregion

    }
}
