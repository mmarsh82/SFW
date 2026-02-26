using M2kClient;
using SFW.Enumerations;
using SFW.Helpers;
using SFW.Model;
using SFW.Model.Management;
using SFW.Model.Product;
using SFW.Model.Production;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.ShopRoute
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private WorkOrder shopOrder;
        public WorkOrder ShopOrder
        {
            get { return shopOrder; }
            set
            {
                shopOrder = value;
                shopOrder.ToolList = shopOrder.ToolList ?? new List<Tool>();
                OnPropertyChanged(nameof(ShopOrder));
                OnPropertyChanged(nameof(FqSalesOrder));
                ShopOrderNotes = null;
                MachineGroup = string.Empty;
                OnPropertyChanged(nameof(CanCheckHistory));
                OnPropertyChanged(nameof(HasFirstPiece));
                OnPropertyChanged(nameof(SelectedOrder));
                OnPropertyChanged(nameof(IsQued));
                OnPropertyChanged(nameof(QueStateMessage));
            }
        }
        public string SelectedOrder { get { return ShopOrder.OrderNumber; } }

        public string FqSalesOrder
        {
            get { return $"{ShopOrder?.SalesOrder?.SalesNumber}*{ShopOrder?.SalesOrder?.LineNumber}"; }
        }

        public int CurrentSite { get { return App.SiteNumber; } }

        private string _shopNotes;
        public string ShopOrderNotes
        {
            get
            { return _shopNotes; }
            set
            {
                _shopNotes = string.IsNullOrEmpty(value) ? ShopOrder?.Notes : value;
                OnPropertyChanged(nameof(ShopOrderNotes));
            }
        }

        private string machGroup;
        public string MachineGroup
        {
            get
            { return machGroup; }
            set
            { machGroup = string.IsNullOrEmpty(value) ? Machine.GetGroup(ShopOrder.WorkCenter?.MachineName, 'M') : value; OnPropertyChanged(nameof(MachineGroup)); }
        }

        private string _sFile;
        public string SetupFile
        {
            get { return _sFile; }
            set { _sFile = value; OnPropertyChanged(nameof(SetupFile)); OnPropertyChanged(nameof(HasSetupFile)); }
        }
        public bool HasSetupFile { get { return !string.IsNullOrEmpty(SetupFile); } }

        private bool loading;
        public bool IsLoading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public bool CanCheckHistory { get { return ShopOrder?.StartQty != ShopOrder?.CurrentQty; } }
        public bool CanReport { get { return MachineGroup == "PRESS"; } }
        public bool CanSeeTrim { get { return MachineGroup == "PRESS"; } }

        public bool IsMultiLoading { get; set; }

        public IList<Lot> ILotResultsList { get; set; }

        private bool _noLot;
        public bool NoLotResults
        {
            get => _noLot;
            set { _noLot = value; OnPropertyChanged(nameof(NoLotResults)); }
        }

        public IList<Lot> IDedicateLotResultsList { get; set; }

        private bool _noDed;
        public bool NoDedicateResults
        {
            get => _noDed;
            set { _noDed = value; OnPropertyChanged(nameof(NoDedicateResults)); }
        }

        private string _lotText;
        public string LotListText
        {
            get => _lotText;
            set { _lotText = value; OnPropertyChanged(nameof(LotListText)); }
        }

        private Model.Production.Component _selItem;
        public Model.Production.Component SelectedILotItem
        {
            get
            { return _selItem; }
            set
            {
                if (value != null)
                {
                    if (value.IsLotTrace)
                    {
                        LotListLoading = true;
                        LotListText = "";
                        OnPropertyChanged(nameof(LotListText));
                        OnPropertyChanged(nameof(LotListLoading));
                        using (BackgroundWorker bw = new BackgroundWorker())
                        {
                            try
                            {
                                bw.DoWork += new DoWorkEventHandler(
                                    delegate (object sender, DoWorkEventArgs e)
                                    {
                                        ILotResultsList = Lot.GetOnHandList(value.ProductNumber, value.IsLotTrace, App.SiteNumber);
                                        NoLotResults = ILotResultsList.Count == 0;
                                        OnPropertyChanged(nameof(ILotResultsList));
                                        OnPropertyChanged(nameof(NoLotResults));
                                        LotListLoading = false;
                                        OnPropertyChanged(nameof(LotListLoading));
                                        IDedicateLotResultsList = Lot.GetDedicatedList(value.ProductNumber, ShopOrder.OrderNumber);
                                        NoDedicateResults = IDedicateLotResultsList.Count == 0;
                                        OnPropertyChanged(nameof(IDedicateLotResultsList));
                                        LotListText = NoDedicateResults && NoLotResults ? "No Onhand Material" : "";
                                        OnPropertyChanged(nameof(LotListText));
                                    });
                                bw.RunWorkerAsync();
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    else
                    {
                        ILotResultsList = Lot.GetOnHandList(value.ProductNumber, value.IsLotTrace, App.SiteNumber);
                        NoLotResults = ILotResultsList.Count == 0;
                        OnPropertyChanged(nameof(ILotResultsList));
                        IDedicateLotResultsList = Lot.GetDedicatedList(value.ProductNumber, ShopOrder.OrderNumber);
                        NoDedicateResults = IDedicateLotResultsList.Count == 0;
                        OnPropertyChanged(nameof(IDedicateLotResultsList));
                        LotListText = NoDedicateResults && NoLotResults ? "No Onhand Material" : "";
                    }
                }
                _selItem = value;
                ItemIsLotTrace = value.IsLotTrace;
                OnPropertyChanged(nameof(ItemIsLotTrace));
                OnPropertyChanged(nameof(SelectedILotItem));
                OnPropertyChanged(nameof(LotListText));
            }
        }
        public bool ItemIsLotTrace { get; set; }
        public bool LotListLoading { get; set; }

        public ObservableCollection<string> CompCollection { get; set; }
        private string _compSel;
        public string SelectedComp
        {
            get { return _compSel; }
            set
            {
                _compSel = value;
                ActivityTable = Sku.GetActivityTable(value, App.AppSqlCon).AsDataView();
                OnPropertyChanged(nameof(ActivityTable));
                OnPropertyChanged(nameof(SelectedComp));
            }
        }

        public bool HasFirstPiece
        {
            get
            { return ShopOrder?.WorkCenter?.MachineGroup == "PRESS"; }
        }

        private IList<string> _ncrList;
        public IList<string> NcrList
        {
            get { return _ncrList; }
            set
            {
                _ncrList = value;
                OnPropertyChanged(nameof(NcrList));
            }
        }

        private bool _isPlan;
        public bool IsPlan
        {
            get
            { return _isPlan; }
            set
            {
                _isPlan = value;
                OnPropertyChanged(nameof(IsPlan));
            }
        }

        private bool _bomOnly;
        public bool BomOnly
        {
            get
            { return _bomOnly; }
            set
            {
                _bomOnly = value;
                OnPropertyChanged(nameof(BomOnly));
            }
        }

        private bool _wipAct;
        public bool WipActive
        { 
            get 
            { return _wipAct; }
            set
            {
                _wipAct = value;
                OnPropertyChanged(nameof(WipActive));
            }
        }

        public IList<string> ComponentDefectList { get; set; }

        public DataView ActivityTable { get; set; }

        private string _com;
        public string CommentInput
        {
            get
            { return _com; }
            set
            {
                _com = value.ReplaceExplicitWords();
                OnPropertyChanged(nameof(CommentInput));
            }
        }
        public ObservableCollection<WorkOrderComment> CommentCollection { get; set; }

        private string _msg;
        public string Message
        {
            get
            { return _msg; }
            set
            {
                _msg = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string QueStateMessage
        {
            get
            {
                var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
                switch (_state)
                {
                    case QueState.Setup:
                        return "Setup";
                    case QueState.Inspection:
                        return "Inspection";
                    default:
                        return string.Empty;
                }
            }
        }

        public bool IsQued
        {
            get
            {
                if (CurrentUser.IsLoggedIn)
                {
                    var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
                    if (!CurrentUser.BasicUser)
                    {
                        return false;
                    }
                    return ValidateQueState();
                }
                Message = "            Not Signed In\nDetailed information hidden";
                return false;
            }
        }

        public bool ShowView
        {
            get
            {
                if (CurrentUser.IsLoggedIn)
                {
                    var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
                    if (!CurrentUser.BasicUser || ShopOrder.Status == "C")
                    {
                        return true;
                    }
                    if (_state == QueState.Setup || _state == QueState.Inspection || _state == QueState.Running)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ShowComplete
        {
            get
            {
                var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
                return (_state == QueState.Inspection || _state == QueState.Setup) && CurrentUser.BasicUser;
            }
        }

        public bool ShowSkip
        {
            get
            {
                var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
                return _state == QueState.Inspection && CurrentUser.BasicUser;
            }
        }

        public BindingList<Employee> CrewList { get; set; }

        private RelayCommand _noteChange;
        private RelayCommand _modComment;
        private RelayCommand _removeCrew;
        private RelayCommand _queStateChange;

        #endregion

        /// <summary>
        /// Shop Route Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ShopOrder == null)
            {
                ShopOrder = new WorkOrder();
            }
            LotListLoading = false;
            Message = string.Empty;
            CrewList = new BindingList<Employee>();
        }

        /// <summary>
        /// Shop Route Constructor for loading work orders
        /// </summary>
        /// <param name="workOrder">Work Order Object</param>
        public ViewModel(WorkOrder workOrder)
        {
            if (workOrder.OrderID == null)
            {
                workOrder = new WorkOrder(ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name].Rows[0]);
            }
            Message = string.Empty;
            LotListLoading = false;
            ShopOrder = workOrder;
            IsMultiLoading = true;
            NoLotResults = NoDedicateResults = true;
            LotListText = "Select a Part";
            NcrList = new List<string>();
            ComponentDefectList = new List<string>();
            IsPlan = BomOnly = ShopOrder.TaskType == "P";
            WipActive = false;
            CrewList = new BindingList<Employee> { new Employee() { ListId = 1 } };
            CrewList.ListChanged += CrewList_Changed;
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            //Getting the Work order work instructions
                            if (App.GlobalConfig != null)
                            {
                                try
                                {
                                    var _instList = SkuInstruction.GetList(ShopOrder.Product.SkuNumber, App.SiteNumber, App.GlobalConfig.First(o => o.Site == App.Facility).WI);
                                    if (_instList != null || _instList.Count > 0)
                                    {
                                        ShopOrder.Product.InstructionList = _instList;
                                    }
                                }
                                catch
                                {
                                    ShopOrder.Product.InstructionList = new List<string>();
                                }
                            }

                            //Getting the work order notes and the shop floor notes
                            ShopOrderNotes = ShopOrder.TaskType != "P" ? WorkOrderNote.GetNotes("WN", false, ShopOrder.OrderNumber) : string.Empty;
                            ShopOrder.ShopNotes = WorkOrderNote.GetNotes("SN", true, ShopOrder.OrderNumber, $"{ShopOrder.Product.SkuNumber}|0{ShopOrder.Facility}");

                            //Getting the sales order internal comments
                            ShopOrder.SalesOrder.InternalComments = Model.Sales.SalesOrderNote.GetNote(ShopOrder.SalesOrder.SalesNumber, 'C');

                            //Get the setup up print if it exists
                            SetupFile = GetSetupFile();

                            //Bill of Material and picklist loading, needs to be done in the background due to the recursive search
                            ShopOrder.ToolList = ShopOrder.TaskType == "R"
                                ? Tool.GetList(ShopOrder.Product.SkuNumber, Machine.GetNumber(ShopOrder.WorkCenter.MachineName), CurrentUser.Facility)
                                : Tool.GetList(ShopOrder.Product.SkuNumber, int.Parse(ShopOrder.Routing), CurrentUser.Facility);
                            ShopOrder.BillList = BillComponent.GetList(ShopOrder.Product.SkuNumber, ShopOrder.Seq);
                            ShopOrder.PickList = ShopOrder.TaskType != "P"
                                ? PickComponent.GetList(ShopOrder.OrderNumber, ShopOrder.Seq, ShopOrder.StartQty - ShopOrder.CurrentQty, ShopOrder.WorkCenter.MachineName)
                                : new List<PickComponent>();
                            IsMultiLoading = false;
                            if (App.SiteNumber == 1)
                            {
                                NcrList = Model.Quality.QmsForm.GetNcrList(ShopOrder.OrderNumber);
                            }
                            if (CurrentUser.CanSchedule)
                            {
                                CompCollection = BillComponent.GetFullNameCollection(ShopOrder.Product.SkuNumber, ShopOrder.Seq);
                                SelectedComp = CompCollection != null ? CompCollection.FirstOrDefault(o => o == ShopOrder.Product.SkuNumber) : null;
                                OnPropertyChanged(nameof(CompCollection));
                            }
                            CommentCollection = new ObservableCollection<WorkOrderComment>();
                            if (CurrentUser.IsLoggedIn)
                            {
                                CommentCollection = WorkOrderComment.GetCollection(ShopOrder.OrderID);
                                OnPropertyChanged(nameof(CommentCollection));
                            }
                            OnPropertyChanged(nameof(IsMultiLoading));
                            OnPropertyChanged(nameof(ShopOrder));
                            WipActive = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs == QueState.Running || CurrentUser.IsSupervisor || CurrentUser.CanSchedule || ShopOrder.Status == "C" : false;
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Get the filepath of a setup file if it exists
        /// </summary>
        /// <returns>Filepath of a setup print</returns>
        public string GetSetupFile()
        {
            var _filePath = string.Empty;
            try
            {
                switch (App.SiteNumber)
                {
                    case 2:
                        try
                        {
                            _filePath = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PressSetup}{ShopOrder.Product.SkuNumber}.pdf";
                            break;
                        }
                        catch (Exception)
                        {
                            _filePath = $"{ShopOrder.OrderNumber}|Old";
                            break;
                        }
                    case 1:
                        var _fileName = string.Empty;
                        switch (ShopOrder.WorkCenter.MachineGroup)
                        {
                            case "PRESS":
                            case "ENG":
                            case "ENDLSS":
                                _fileName = ExcelReader.GetSetupPrintNumber(ShopOrder.Product.SkuNumber, ShopOrder.WorkCenter.MachineName, App.GlobalConfig.First(o => o.Site == App.Facility).PressSetup, "Production");
                                if (!string.IsNullOrEmpty(_fileName) && !_fileName.Contains("ERR:"))
                                {
                                    var _fileheader = string.Empty;
                                    for (int i = 0; i < 8 - _fileName.Length; i++)
                                    {
                                        _fileheader += "0";
                                    }
                                    _fileName = _fileheader + _fileName;
                                    _filePath = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                }
                                else
                                {
                                    _filePath = _fileName;
                                }
                                break;
                            case "FABE":
                                 _fileName = ExcelReader.GetSetupPrintNumber(ShopOrder.Product.SkuNumber, ShopOrder.WorkCenter.MachineName, App.GlobalConfig.First(o => o.Site == App.Facility).SyscoSetup, "PRODUCTION");
                                _filePath = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                break;
                            case "EXT":
                                _fileName = ExcelReader.GetSetupPrintNumber(ShopOrder.Product.SkuNumber, ShopOrder.WorkCenter.MachineName, App.GlobalConfig.First(o => o.Site == App.Facility).ExtSetup, "PRODUCTION");
                                _filePath = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                break;
                        }
                        break;
                }
                return _filePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Validate the proper message for the current que state
        /// </summary>
        /// <returns></returns>
        public bool ValidateQueState()
        {
            var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
            var _queList = WorkOrder.GetQueDictionary(ShopOrder?.WorkCenter?.MachineNumber);
            var _invalidState = QueState.Unassigned;
            if ((_state == QueState.InQue || _state == QueState.Unassigned) && (ShopOrder.Status == "A" || ShopOrder.Status == "R"))
            {
                if (_queList != null)
                {
                    if (_queList.Count(o => o.Value == 5) > 0)
                    {
                        _invalidState = QueState.Down;
                    }
                    else if (_queList.Count(o => o.Value == 0) > 0 && _queList.Count(o => o.Key == ShopOrder.OrderID) == 0)
                    {
                        _invalidState = QueState.InQue;
                    }
                    else if (_state == QueState.Unassigned)
                    {
                        if (_queList.Count(o => o.Value == 3) > 0)
                        {
                            _invalidState = QueState.Running;
                        }
                        if (_queList.Count(o => o.Value == 2) > 0)
                        {
                            _invalidState = QueState.Inspection;
                        }
                    }
                    else if (_state == QueState.Setup)
                    {
                        if (_queList.Count(o => o.Value == 3) > 0)
                        {
                            _invalidState = QueState.Running;
                        }
                    }
                    if (_invalidState != QueState.Unassigned)
                    {
                        Message = _queList.Count(o => o.Value == (int)_invalidState) == 1 ? "Work Order " : "Work Orders ";
                        foreach (var _order in _queList.Where(o => o.Value == (int)_invalidState))
                        {
                            Message += $"{_order.Key.Split('*')[0]}, ";
                        }
                        Message = Message.TrimEnd(' ').TrimEnd(',');
                        Message += $"\nCurrently {_invalidState.ToString().ToUpper()}\nContact your supervisor for assitance.";
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<Component> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void CrewList_Changed(object sender, ListChangedEventArgs e)
        {
            var _add = false;
            ((BindingList<Employee>)sender).RaiseListChangedEvents = false;
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "ErpId")
            {
                if (Employee.ValidErpId(((BindingList<Employee>)sender)[e.NewIndex].ErpId) && ((BindingList<Employee>)sender).Count(o => o.ErpId == ((BindingList<Employee>)sender)[e.NewIndex].ErpId) == 1)
                {
                    var _tempCrew = new Employee(((BindingList<Employee>)sender)[e.NewIndex].ErpId, true, false);
                    ((BindingList<Employee>)sender)[e.NewIndex].Facility = _tempCrew.Facility;
                    ((BindingList<Employee>)sender)[e.NewIndex].IsDirect = _tempCrew.IsDirect;
                    ((BindingList<Employee>)sender)[e.NewIndex].Name = _tempCrew.Name;
                    ((BindingList<Employee>)sender)[e.NewIndex].Shift = _tempCrew.Shift;
                    ((BindingList<Employee>)sender)[e.NewIndex].ShiftEnd = _tempCrew.ShiftEnd;
                    ((BindingList<Employee>)sender)[e.NewIndex].ShiftStart = _tempCrew.ShiftStart;
                    ((BindingList<Employee>)sender)[e.NewIndex].LaborData = _tempCrew.LaborData;
                    ((BindingList<Employee>)sender)[e.NewIndex].SapId = _tempCrew.SapId;
                    if (((BindingList<Employee>)sender).Count() == ((BindingList<Employee>)sender).Count(o => !string.IsNullOrEmpty(o.Name)))
                    {
                        _add = true;
                    }
                }
                else if (!string.IsNullOrEmpty(((BindingList<Employee>)sender)[e.NewIndex].Name))
                {
                    //TODO: add in logic to remove the second entry when deleting the first and list only has 2 entrys
                    ((BindingList<Employee>)sender)[e.NewIndex].Name = null;
                    ((BindingList<Employee>)sender)[e.NewIndex].IsDirect = false;
                    ((BindingList<Employee>)sender)[e.NewIndex].Shift = 0;
                    ((BindingList<Employee>)sender)[e.NewIndex].Facility = null;
                    ((BindingList<Employee>)sender)[e.NewIndex].LaborData = null;
                }
            }
            ((BindingList<Employee>)sender).RaiseListChangedEvents = true;
            if (_add)
            {
                ((BindingList<Employee>)sender).Add(new Employee() { ListId = ((BindingList<Employee>)sender).Count });
            }
        }

        #region Work Order Note Change ICommand

        public ICommand WONoteChgICommand
        {
            get
            {
                if (_noteChange == null)
                {
                    _noteChange = new RelayCommand(NoteChgExecute);
                }
                return _noteChange;
            }
        }

        private void NoteChgExecute(object parameter)
        {
            var _note = ShopOrderNotes ?? string.Empty;
            var _noteArray = _note.Replace("\r", "").Replace("\n", "|").Split('|');
            var _changeRequest = M2kCommand.EditMVRecord("WP", ShopOrder.OrderNumber, 39, _noteArray, App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
                ShopOrderNotes = ShopOrder.Notes;
            }
        }

        #endregion

        #region Work Order Comment ICommand

        public ICommand ModifyCommentICommand
        {
            get
            {
                if (_modComment == null)
                {
                    _modComment = new RelayCommand(ModifyCommentExecute, ModifyCommentCanExecute);
                }
                return _modComment;
            }
        }

        private void ModifyCommentExecute(object parameter)
        {
            if (char.TryParse(parameter.ToString(), out char c))
            {
                var _woComment = new WorkOrderComment
                {
                    WpoId = ShopOrder.OrderID
                    ,CommentText = CommentInput
                    ,SubmitDate = DateTime.Now
                    ,SubmitterId = CurrentUser.ErpId
                    ,Status = 'A'
                };
                _woComment.CommentId = WorkOrderComment.SubmitComment(_woComment, App.AppSqlCon);
                if (_woComment.CommentId != 0)
                {
                    CommentCollection.Insert(0, _woComment);
                    OnPropertyChanged(nameof(CommentCollection));
                    CommentInput = string.Empty;
                }
            }
            else
            {
                if (parameter.GetType() == typeof(WorkOrderComment))
                {
                    var _com = (WorkOrderComment)parameter;
                    var _newStatus = WorkOrderComment.ModifyStatus(_com, App.AppSqlCon);
                    if (_newStatus != 'E')
                    {
                        CommentCollection.FirstOrDefault(o => o.CommentId == _com.CommentId).Status = _newStatus;
                        OnPropertyChanged(nameof(CommentCollection));
                    }
                }
            }
        }
        private bool ModifyCommentCanExecute(object parameter)
        {
            if (char.TryParse(parameter?.ToString(), out char c))
            {
                return !string.IsNullOrEmpty(CommentInput);
            }
            return true;
        }

        #endregion

        #region Remove Crew List Item ICommand

        public ICommand RemoveCrewICommand
        {
            get
            {
                if (_removeCrew == null)
                {
                    _removeCrew = new RelayCommand(RemoveCrewExecute);
                }
                return _removeCrew;
            }
        }

        private void RemoveCrewExecute(object parameter)
        {
            CrewList.Remove(CrewList.FirstOrDefault(c => c.ErpId.ToString() == parameter.ToString()));
            foreach (var _emp in CrewList)
            {
                _emp.ListId = CrewList.IndexOf(_emp);
            }
        }

        #endregion

        #region QueState Change ICommand

        public ICommand QueStateChangeICommand
        {
            get
            {
                if (_queStateChange == null)
                {
                    _queStateChange = new RelayCommand(QueStateChangeExecute, QueStateChangeCanExecute);
                }
                return _queStateChange;
            }
        }

        private void QueStateChangeExecute(object parameter)
        {
            var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
            var _newState = (int)_state + 1;
            switch (parameter.ToString())
            {
                case "B":
                    WorkOrder.UpdateQue(ShopOrder.OrderID, 1);
                    _newState = 1;
                    WorkOrder.SubmitTracking(ShopOrder.OrderID, 'S', CrewList.Where(o => !string.IsNullOrEmpty(o.Name)).Select(o => o.ErpId).ToList(), App.AppSqlCon);
                    break;
                case "S":
                    _newState = _state == QueState.Unassigned ? _newState + 2 : _newState;
                    WorkOrder.UpdateQue(ShopOrder.OrderID, _newState);
                    if (_state == QueState.Unassigned)
                    {
                        WorkOrder.SubmitTracking(ShopOrder.OrderID, 'I', CrewList.Where(o => !string.IsNullOrEmpty(o.Name)).Select(o => o.ErpId).ToList(), App.AppSqlCon);
                    }
                    else
                    {
                        WorkOrder.DeleteTracking(ShopOrder.OrderID, _state.GetDescription().ToCharArray()[0], App.AppSqlCon);
                    }
                    break;
                case "C":
                    if (_state == QueState.Setup)
                    {
                        var _crewList = WorkOrder.UpdateTracking(ShopOrder.OrderID, _state.GetDescription().ToCharArray()[0], App.AppSqlCon);
                        if (_crewList != null && _crewList.Count > 0)
                        {
                            WorkOrder.SubmitTracking(ShopOrder.OrderID, 'I', _crewList, App.AppSqlCon);
                        }
                    }
                    else
                    {
                        var _crewList = WorkOrder.UpdateTracking(ShopOrder.OrderID, _state.GetDescription().ToCharArray()[0], App.AppSqlCon);
                    }
                    WorkOrder.UpdateQue(ShopOrder.OrderID, _newState);
                    break;
            }
            ShopOrder.QueState = _newState;
            OnPropertyChanged(nameof(IsQued));
            OnPropertyChanged(nameof(ShowView));
            OnPropertyChanged(nameof(ShowComplete));
            OnPropertyChanged(nameof(ShowSkip));
            OnPropertyChanged(nameof(QueStateMessage));
            ApplicationTimer.Resume();
        }

        private bool QueStateChangeCanExecute(object parameter)
        {
            var _state = Enum.TryParse(ShopOrder.QueState.ToString(), out QueState qs) ? qs : QueState.Unassigned;
            if (_state == QueState.Unassigned)
            {
                return CurrentUser.CanWip && CrewList.Count(o => !string.IsNullOrEmpty(o.Name)) > 0;
            }
            else
            {
                return CurrentUser.CanWip;
            }
        }

        #endregion
    }
}
