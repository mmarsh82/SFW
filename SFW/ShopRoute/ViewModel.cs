using M2kClient;
using SFW.Helpers;
using SFW.Model;
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
            }
        }

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
                    ILotResultsList = Lot.GetOnHandList(value.ProductNumber, true, App.SiteNumber);
                    NoLotResults = ILotResultsList.Count == 0;
                    IDedicateLotResultsList = Lot.GetDedicatedList(value.ProductNumber, ShopOrder.OrderNumber);
                    NoDedicateResults = IDedicateLotResultsList.Count == 0;
                    if (App.SiteNumber == 0 && NoDedicateResults)
                    {

                    }
                    OnPropertyChanged(nameof(ILotResultsList));
                    OnPropertyChanged(nameof(IDedicateLotResultsList));
                    LotListText = NoDedicateResults && NoLotResults ? "No Onhand Material" : "";
                }
                _selItem = value;
                OnPropertyChanged(nameof(SelectedILotItem));
                OnPropertyChanged(nameof(LotListText));
            }
        }

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

        public DataView ActivityTable { get; set; }

        private RelayCommand _noteChange;

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
        }

        /// <summary>
        /// Shop Route Constructor for loading work orders
        /// </summary>
        /// <param name="workOrder">Work Order Object</param>
        public ViewModel(WorkOrder workOrder)
        {
            if (workOrder.OrderID == null)
            {
                workOrder = new WorkOrder(ModelBase.MasterDataSet.Tables["Master"].Rows[0]);
            }
            ShopOrder = workOrder;
            IsMultiLoading = true;
            NoLotResults = NoDedicateResults = true;
            LotListText = "Select a Part";
            NcrList = new List<string>();
            IsPlan = BomOnly = ShopOrder.TaskType == "P";
            WipActive = false;
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
                            OnPropertyChanged(nameof(IsMultiLoading));
                            OnPropertyChanged(nameof(ShopOrder));
                            WipActive = true;
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

        #region Work Order Note Change ICommand

        public ICommand WONoteChgICommand
        {
            get
            {
                if (_noteChange == null)
                {
                    _noteChange = new RelayCommand(NoteChgExecute, NoteChgCanExecute);
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
        private bool NoteChgCanExecute(object parameter) => true;

        #endregion
    }
}
