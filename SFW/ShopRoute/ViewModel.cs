using M2kClient;
using SFW.Helpers;
using SFW.Model;
using SFW.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            { machGroup = string.IsNullOrEmpty(value) ? Machine.GetMachineGroup(ShopOrder.Machine, 'M') : value; OnPropertyChanged(nameof(MachineGroup)); }
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

        private Model.Component _selItem;
        public Model.Component SelectedILotItem
        {
            get
            { return _selItem; }
            set
            {
                if (value != null)
                {
                    ILotResultsList = Lot.GetOnHandLotList(value.CompNumber, true, App.SiteNumber);
                    NoLotResults = ILotResultsList.Count == 0;
                    IDedicateLotResultsList = Lot.GetDedicatedLotList(value.CompNumber, ShopOrder.OrderNumber);
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

        public bool HasFirstPiece
        {
            get
            { return ShopOrder.MachineGroup == "PRESS"; }
        }

        private RelayCommand _noteChange;
        private RelayCommand _loadReport;

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
            ShopOrder = workOrder;
            IsMultiLoading = true;
            NoLotResults = NoDedicateResults = true;
            LotListText = "Select a Part";
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
                                    ShopOrder.InstructionList = Sku.GetInstructions(ShopOrder.SkuNumber, App.SiteNumber, App.GlobalConfig.First(o => o.Site == App.Facility).WI);
                                }
                                catch
                                {
                                    ShopOrder.InstructionList = new List<string>();
                                }
                            }

                            //Getting the work order notes and the shop floor notes
                            ShopOrderNotes = WorkOrder.GetNotes("WN", false, ShopOrder.OrderNumber);
                            ShopOrder.ShopNotes = WorkOrder.GetNotes("SN", true, ShopOrder.OrderNumber, $"{ShopOrder.SkuNumber}|0{ShopOrder.Facility}");

                            //Getting the sales order internal comments
                            ShopOrder.SalesOrder.InternalComments = Model.SalesOrder.GetNotes(ShopOrder.SalesOrder.SalesNumber, 'C');

                            //Get the setup up print if it exists
                            SetupFile = GetSetupFile();

                            //Bill of Material and picklist loading, needs to be done in the background due to the recursive search
                            ShopOrder.ToolList = Tool.GetToolList(ShopOrder.SkuNumber, ShopOrder.Routing, CurrentUser.Facility);
                            ShopOrder.Bom = Model.Component.GetComponentBomList(ShopOrder.SkuNumber, ShopOrder.Seq);
                            ShopOrder.Picklist = Model.Component.GetComponentPickList(ShopOrder.OrderNumber, ShopOrder.Seq, ShopOrder.StartQty - ShopOrder.CurrentQty, ShopOrder.Machine);
                            IsMultiLoading = false;
                            OnPropertyChanged(nameof(IsMultiLoading));
                            OnPropertyChanged(nameof(ShopOrder));
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
                            _filePath = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PressSetup}{ShopOrder.SkuNumber}.pdf";
                            break;
                        }
                        catch (Exception)
                        {
                            _filePath = $"{ShopOrder.OrderNumber}|Old";
                            break;
                        }
                    case 1:
                        var _fileName = string.Empty;
                        switch (ShopOrder.MachineGroup)
                        {
                            case "PRESS":
                            case "ENG":
                            case "ENDLSS":
                                _fileName = ExcelReader.GetSetupPrintNumber(ShopOrder.SkuNumber, ShopOrder.Machine, App.GlobalConfig.First(o => o.Site == App.Facility).PressSetup, "Production");
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
                                 _fileName = ExcelReader.GetSetupPrintNumber(ShopOrder.SkuNumber, ShopOrder.Machine, App.GlobalConfig.First(o => o.Site == App.Facility).SyscoSetup, "PRODUCTION");
                                _filePath = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                break;
                            case "EXT":
                                _fileName = ExcelReader.GetSetupPrintNumber(ShopOrder.SkuNumber, ShopOrder.Machine, App.GlobalConfig.First(o => o.Site == App.Facility).ExtSetup, "PRODUCTION");
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

        #region Press Report ICommand

        public ICommand ReportICommand
        {
            get
            {
                if (_loadReport == null)
                {
                    _loadReport = new RelayCommand(ReportExecute, ReportCanExecute);
                }
                return _loadReport;
            }
        }

        private void ReportExecute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int i))
            {
                var _repType = (Enumerations.PressReportActions)i;
                var _rep = new PressReport(shopOrder, App.AppSqlCon);
                if (_repType == Enumerations.PressReportActions.LogProgress && (_rep.IsNew || _rep.ShiftReportList.Count == 0))
                {
                    MessageBox.Show("There is currently no report created for this work order.\nPlease click on the report sheet button and submit a new report.", "No Report Sheet", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    if (ShopOrder != null)
                    {
                        if (!App.IsWindowOpen<Press_View>(new Press_ViewModel()))
                        {
                            new Press_View { DataContext = new Press_ViewModel(ShopOrder, _repType) }.Show();
                        }
                        else
                        {
                            var _win = App.GetWindow<Press_View>();
                            if (_win != null)
                            {
                                _win.DataContext = new Press_ViewModel(ShopOrder, _repType);
                                _win.Focus();
                            }
                        }
                    }
                }
            }
        }
        private bool ReportCanExecute(object parameter) => true;

        #endregion
    }
}
