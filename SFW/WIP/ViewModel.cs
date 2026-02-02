using DocumentFormat.OpenXml.Drawing.Diagrams;
using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using SFW.Model.Production;
using SFW.Model.Production.Wip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Input;

//Created by Michael Marsh 10-23-18

namespace SFW.WIP
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public Receipt WipRecord { get; set; }
        
        public string WipQuantity
        {
            get { return WipRecord.WipQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int _wipStr))
                {
                    if (_wipStr == 0 || value == null)
                    {
                        foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
                        {
                            _comp.LotList.Clear();
                            _comp.LotList.Add(new Lot());
                        }
                    }
                    else if (WipRecord.WipQty != _wipStr)
                    {
                        if (_wipStr == -987654)
                        {
                            _wipStr = Convert.ToInt32(WipRecord.WipQty);
                        }
                        var _qty = _wipStr;
                        _qty *= WipRecord.IsMulti && int.TryParse(RollQuantity, out int iRoll) ? iRoll : 1;
                        WipRecord.ComponentList.Update(_qty);
                    }
                    WipRecord.WipQty = _wipStr;
                }
                else
                {
                    WipRecord.WipQty = null;
                }
                OnPropertyChanged(nameof(WipQuantity));
                OnPropertyChanged(nameof(WipRecord));
            }
        }

        #region Wip Lot Property

        public string WipLot
        {
            get
            { return WipRecord.WipLot.LotNumber; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    IsLotValid = Model.Product.Lot.IsValid(value, WipRecord.WipWorkOrder.Product.SkuNumber);
                }
                else
                {
                    IsLotValid = true;
                }
                WipRecord.WipLot.LotNumber = value;
                WipLocation = null;
                OnPropertyChanged(nameof(WipLot));
                OnPropertyChanged(nameof(WipRecord));
            }
        }
        private bool _isLotValid;
        public bool IsLotValid
        {
            get
            { return _isLotValid; }
            set
            { _isLotValid = value; OnPropertyChanged(nameof(IsLotValid)); }
        }

        #endregion

        #region Wip Location Property

        public string WipLocation
        {
            get
            { return WipRecord.ReceiptLocation; }
            set
            {
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(WipRecord.WipLot.LotNumber))
                {
                    IsLocationValid = Location.Valid(value, App.SiteNumber);
                    IsLocationEditable = true;
                }
                else if (!string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) && IsLotValid)
                {
                    var _loc = Model.Product.Lot.GetLocation(WipRecord.WipLot.LotNumber);
                    if (_loc == null)
                    {
                        IsLocationValid = Location.Valid(value, App.SiteNumber);
                        IsLocationEditable = true;
                    }
                    else
                    {
                        IsLocationValid = true;
                        IsLocationEditable = false;
                        value = _loc;
                    }
                }
                else if (!string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) && !IsLotValid)
                {
                    IsLocationValid = true;
                    IsLocationEditable = false;
                }
                else
                {
                    IsLocationValid = true;
                    IsLocationEditable = true;
                }
                WipRecord.ReceiptLocation = value;
                OnPropertyChanged(nameof(WipLocation));
                OnPropertyChanged(nameof(WipRecord));
            }
        }
        private bool _isLocValid;
        public bool IsLocationValid
        {
            get { return _isLocValid; }
            set { _isLocValid = value; OnPropertyChanged(nameof(IsLocationValid)); }
        }
        private bool _isLocEdit;
        public bool IsLocationEditable
        {
            get { return _isLocEdit; }
            set { _isLocEdit = value; OnPropertyChanged(nameof(IsLocationEditable)); }
        }

        #endregion

        public Model.Enumerations.Complete Scrap
        {
            get { return WipRecord.IsScrap; }
            set
            {
                WipRecord.IsScrap = value;
                OnPropertyChanged(nameof(Scrap));
                WipRecord.ScrapList.Clear();
                WipRecord.ScrapList.Add(new Scrap("0", WipLot, WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Product.SkuNumber, 'P'));
                WipQuantity = "-987654";
                OnPropertyChanged(nameof(WipRecord));
            }
        }

        public Model.Enumerations.Complete Reclaim
        {
            get
            { return WipRecord.IsReclaim; }
            set
            {
                WipRecord.IsReclaim = value;
                OnPropertyChanged(nameof(Reclaim)); OnPropertyChanged(nameof(WipRecord));
                WipRecord.ReclaimObject = new Reclaim();
                if (WipRecord.WipWorkOrder.PickList.Count(o => o.InventoryType == "RC") > 0)
                {
                    WipRecord.ReclaimObject.Parent = WipRecord.WipWorkOrder.PickList.Where(o => o.InventoryType == "RC").FirstOrDefault().ProductNumber;
                    WipRecord.ReclaimObject.ParentAssyQty = WipRecord.WipWorkOrder.PickList.Where(o => o.InventoryType == "RC").FirstOrDefault().AssemblyQuantity;
                }
                else if (WipRecord.WipWorkOrder.PickList.Count() == 1)
                {
                    var _tempComp = new BillComponent(WipRecord.WipWorkOrder.PickList[0].ProductNumber, "RC");
                    WipRecord.ReclaimObject.Parent = _tempComp.ProductNumber;
                    WipRecord.ReclaimObject.ParentAssyQty = WipRecord.WipWorkOrder.PickList[0].AssemblyQuantity * _tempComp.AssemblyQuantity;
                }
                WipQuantity = "-987654";
                OnPropertyChanged(nameof(WipRecord));
            }
        }

        public string RollQuantity
        {
            get { return WipRecord.RollQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    WipRecord.RollQty = i;
                }
                else
                {
                    WipRecord.RollQty = null;
                }
                OnPropertyChanged(nameof(RollQuantity));
                WipQuantity = "-987654";
            }
        }

        public bool Multi
        {
            get { return WipRecord.IsMulti; }
            set
            {
                WipRecord.IsMulti = value;
                OnPropertyChanged(nameof(Multi));
                if (!value)
                {
                    RollQuantity = null;
                }
            }
        }

        public bool HasCrew { get { return WipRecord.CrewList != null; } }

        private int? tQty;
        public int? TQty
        {
            get { return tQty; }
            set { tQty = value; OnPropertyChanged(nameof(TQty)); }
        }

        public int? Weight
        {
            get { return WipRecord?.Weight; }
            set { WipRecord.Weight = value; OnPropertyChanged(nameof(Weight)); }
        }

        public bool IsLotTrace
        {
            get { return WipRecord.IsLotTracable || WipRecord.WipWorkOrder.PickList.Count(o => o.IsLotTrace) > 0; }
        }

        private List<string> _lList;
        public List<string> LotList
        {
            get
            { return _lList; }
            set
            {
                _lList = value;
                OnPropertyChanged(nameof(LotList));
            }
        }

        private bool isSubmit;
        public bool IsSubmitted
        {
            get
            { return isSubmit; }
            set
            { isSubmit = value; OnPropertyChanged(nameof(IsSubmitted)); }
        }

        private string[] _cPart;
        public string[] CompoundPart 
        {
            get { return _cPart; }
            set
            {
                _cPart = value;
                OnPropertyChanged(nameof(CompoundPart));
            }
        }

        private string[] _cLot;
        public string[] CompoundLot
        {
            get { return _cLot; }
            set
            {
                _cLot = value;
                OnPropertyChanged(nameof(CompoundLot));
            }
        }

        public bool Compound
        {
            get
            {
                return WipRecord.WipWorkOrder.PickList.Count(o => o.InventoryType == "RC" || o.InventoryType == "CS") > 0 && App.SiteNumber == 2 && !WipRecord.WipWorkOrder.PickList.FirstOrDefault(o => o.InventoryType == "RC" || o.InventoryType == "CS").IsLotTrace;
            }
        }

        private BindingList<Model.Quality.QmsForm> _dList;
        public BindingList<Model.Quality.QmsForm> DefectList
        {
            get
            { return _dList; }
            set
            {
                _dList = value;
                OnPropertyChanged(nameof(DefectList));
            }
        }

        private bool _notApply;
        public bool NotApply
        {
            get
            { return _notApply; }
            set
            {
                if (value)
                {
                    foreach (var _defect in DefectList)
                    {
                        _defect.IsSelected = false;
                    }
                }
                _notApply = value;
                OnPropertyChanged(nameof(NotApply));
            }
        }

        RelayCommand _wip;
        RelayCommand _mPrint;
        RelayCommand _removeCrew;
        RelayCommand _removeComp;
        RelayCommand _removeScrap;
        RelayCommand _removeCompScrap;
        RelayCommand _addScrap;
        RelayCommand _addCompScrap;
        RelayCommand _printBarLbl;
        RelayCommand _wPrint;

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            ApplicationTimer.Pause();
            CompoundPart = new string[4];
            CompoundLot = new string[4];
            var erpCon = new string[5] { App.ErpCon.HostName, App.ErpCon.UserName, App.ErpCon.Password, App.ErpCon.UniAccount, App.ErpCon.UniService };
            WipRecord = new Receipt(new Model.Management.Employee(CurrentUser.ErpId, true), App.SiteNumber, woObject, erpCon);
            LotList = new List<string>();
            IsSubmitted = false;
            IsLotValid = IsLocationValid = IsLocationEditable = true;
            var _tempList = Model.Quality.QmsForm.GetList(woObject.OrderNumber);
            DefectList = _tempList.Count() > 0
                ? _tempList
                : new BindingList<Model.Quality.QmsForm>();
            DefectList.ListChanged += DefectList_ListChanged;
        }

        /// <summary>
        /// Event handler for Defect List changes
        /// </summary>
        /// <param name="sender">DefectList object</param>
        /// <param name="e">Property that changed and what the changes were</param>
        private void DefectList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "IsSelected")
            {
                if (((BindingList<Model.Quality.QmsForm>)sender)[e.NewIndex].IsSelected && NotApply)
                {
                    NotApply = false;
                }
            }
        }

        /// <summary>
        /// Validates that the LotQty for the components equal the main part Wip quantity
        /// </summary>
        /// <returns>Validation response as bool</returns>
        private bool ValidateComponents()
        {
            try
            {
                foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
                {
                    if (_comp.LotList.Where(o => o.Valid).Sum(o => int.TryParse(o.Quantity, out int i) ? i : 0) != _comp.LotList.First().RequiredQuantity)
                    {
                        return false;
                    }
                    if (CurrentUser.Facility == 1)
                    {
                        foreach (var _lot in _comp.LotList)
                        {
                            if (_lot.HasScrap == Model.Enumerations.Complete.Y)
                            {
                                if (_lot.ScrapCollection.Count(o => o.Valid) != _lot.ScrapCollection.Count())
                                {
                                    return false;
                                }
                            }
                        }
                    }    
                }
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Core WIP receipt validation
        /// </summary>
        /// <returns></returns>
        private bool CoreValidation()
        {
            var _baseValid = false;
            var _locValid = !string.IsNullOrEmpty(WipRecord.ReceiptLocation) && Location.Valid(WipRecord.ReceiptLocation, App.SiteNumber);
            if (WipRecord.WipQty > 0)
            {
                _baseValid = _locValid && (string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) || IsLotValid) && ValidateComponents();
            }
            else if (WipRecord.WipQty == 0)
            {
                if (WipRecord.IsReclaim == Model.Enumerations.Complete.Y)
                {
                    _baseValid = _locValid;
                }
                else if (WipRecord.IsScrap == Model.Enumerations.Complete.Y)
                {
                    _baseValid = _locValid && IsLotValid && ValidateComponents();
                }
                else if (WipRecord.SeqComplete == Model.Enumerations.Complete.Y)
                {
                    _baseValid = _locValid = true;
                }
            }
            else if (WipRecord.WipQty < 0)
            {
                if (WipRecord.IsLotTracable)
                {
                    _baseValid = _locValid && !string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) && IsLotValid && ValidateComponents();
                }
                else
                {
                    _baseValid = _locValid;
                }
            }
            if (App.SiteNumber == 2)
            {
                _baseValid = _baseValid && Weight > 0;
            }
            return _baseValid;
        }

        /// <summary>
        /// Scrap WIP receipt validation
        /// </summary>
        /// <returns></returns>
        private bool ScrapValidation()
        {
            var _scrapValid = true;
            if (WipRecord.IsScrap == Model.Enumerations.Complete.Y)
            {
                var _qty = WipRecord.WipQty;
                _qty += WipRecord.ScrapList.Sum(o => o.Valid && int.TryParse(o.Quantity, out int i) ? i : 0);
                WipRecord.ComponentList.Update(decimal.Parse(_qty.ToString()));
                if (WipRecord.ScrapList.Count(o => int.TryParse(o.Quantity, out int i) && i > 0) > 0)
                {
                    if (App.SiteNumber == 1)
                    {
                        _scrapValid = false;
                        if (WipRecord.ScrapList.Count(o => o.Valid) == WipRecord.ScrapList.Count())
                        {
                            foreach (var s in WipRecord.ScrapList)
                            {
                                if (WipRecord.ScrapList.Count(o => o.Reference == s.Reference) > 1)
                                {
                                    _scrapValid = false;
                                    break;
                                }
                                var _ncrId = int.TryParse(s.Reference, out int nRef) ? nRef : 0;
                                _scrapValid = (WipRecord.IsLotTracable || string.IsNullOrEmpty(WipLot))
                                    ? Model.Product.Lot.IsValidQIR(s.Reference, WipRecord.WipWorkOrder.OrderNumber, "", WipRecord.WipWorkOrder.Product.SkuNumber, App.AppSqlCon) || Model.Quality.QmsForm.IsValid(nRef, WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Product.SkuNumber, 'P')
                                    : Model.Product.Lot.IsValidQIR(s.Reference, WipRecord.WipWorkOrder.OrderNumber, WipLot, WipRecord.WipWorkOrder.Product.SkuNumber, App.AppSqlCon) || Model.Quality.QmsForm.IsValid(nRef, WipRecord.WipWorkOrder.OrderNumber, WipLot, 'L');
                            }
                        }
                    }
                }
                else
                {
                    _scrapValid = false;
                }
            }
            return _scrapValid;
        }

        #region Process Wip ICommand

        public ICommand WipICommand
        {
            get
            {
                if (_wip == null)
                {
                    _wip = new RelayCommand(WipExecute, WipCanExecute);
                }
                return _wip;
            }
        }

        private void WipExecute(object parameter)
        {
            var _preOnHand = !string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) ? Model.Product.Lot.GetOnHandQuantity(WipRecord.WipLot.LotNumber, WipRecord.ReceiptLocation) : 0;
            var _machID = WipRecord.CrewList?.Count > 0 ? Machine.GetNumber(WipRecord.WipWorkOrder.WorkCenter.MachineName) : "";
            var _wipProc = M2kClient.M2kCommand.ProductionWip(WipRecord, WipRecord.CrewList?.Count > 0, App.ErpCon, WipRecord.IsLotTracable, _machID);
            if (_wipProc != null && _wipProc.First().Key > 0)
            {
                if (_wipProc.First().Value != null)
                {
                    WipLot = WipRecord.WipLot.LotNumber = _wipProc.First().Value.Contains("*") || !WipRecord.IsLotTracable ? "Mulitple" : _wipProc.First().Value;
                    LotList = _wipProc.First().Value.Contains("*") ? _wipProc.First().Value.Split('*').ToList() : null;
                }
                else
                {
                    WipRecord.WipLot.LotNumber = "NonLotWip";
                    LotList = null;
                }
                IsSubmitted = true;
                TQty = WipRecord.WipQty + _preOnHand;
                Model.Management.EmployeeLabor.UpdateTimeIn(WipRecord.CrewList.ToList(), App.AppSqlCon);
                if (App.SiteNumber == 2)
                {
                    try
                    {
                        M2kClient.M2kCommand.EditRecord("LOT.MASTER", $"{WipLot}|P|02", 19, Weight.ToString(), M2kClient.UdArrayCommand.Insert, App.ErpCon);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to write weight to the database.", "ERP Error");
                    }
                    WipStickerPrintExecute(null);
                }

                if (DefectList.Count() > 0 && !NotApply && WipRecord.IsLotTracable && !App.InTraining)
                {
                    foreach (var _form in DefectList.Where(o => o.IsSelected))
                    {
                        if (WipRecord.WipLot.LotNumber == "Multiple")
                        {
                            foreach (var _lot in LotList)
                            {
                                Model.Quality.QmsForm.SubmitDefectLot(_form.FormId, _lot, App.AppSqlCon);
                            }
                        }
                        else
                        {
                            Model.Quality.QmsForm.SubmitDefectLot(_form.FormId, WipRecord.WipLot.LotNumber, App.AppSqlCon);
                        }
                    }
                }

                OnPropertyChanged(nameof(WipRecord));
            }
            else
            {
                //TODO: Process errors here
            }
        }
        private bool WipCanExecute(object parameter)
        {
            try
            {
                if (WipRecord != null)
                {
                    var _baseValid = CoreValidation();
                    var _scrapValid = ScrapValidation();
                    var _laborValid = WipRecord.CrewList != null && WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.Name)) > 0;
                    var _reclaimValid = WipRecord.IsReclaim == Model.Enumerations.Complete.N 
                        || (WipRecord.IsReclaim == Model.Enumerations.Complete.Y && int.TryParse(WipRecord.ReclaimObject.Quantity, out int rRef) && rRef > 0);
                    var _multiValid = !WipRecord.IsMulti || (WipRecord.IsMulti && WipRecord.RollQty > 0);
                    var _defectValid = DefectList.Count() == 0 || (DefectList.Count() > 0 && (NotApply || DefectList.Count(o => o.IsSelected) > 0));

                    return _baseValid && _scrapValid && _reclaimValid && _multiValid && _laborValid && _defectValid;
                }
                else
                {
                    return false;
                }
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        #endregion

        #region Material Card Print ICommand

        public ICommand MPrintICommand
        {
            get
            {
                if (_mPrint == null)
                {
                    _mPrint = new RelayCommand(MPrintExecute, MPrintCanExecute);
                }
                return _mPrint;
            }
        }

        private void MPrintExecute(object parameter)
        {
            var _wQty = TQty == null || TQty == 0 ? Convert.ToInt32(WipRecord.WipQty) : Convert.ToInt32(TQty);
            var _diamond = string.Empty;
            var _isStandard = false;
            System.Windows.Forms.PrintDialog prtDialog = new System.Windows.Forms.PrintDialog();
            if (prtDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TravelCard.PrinterName = prtDialog.PrinterSettings.PrinterName;
                if (prtDialog.PrinterSettings.DefaultPageSettings.PaperSize.Kind == System.Drawing.Printing.PaperKind.Letter)
                {
                    _isStandard = true;
                }
            }

            //Printing the travel card logic
            if (LotList == null || LotList.Count == 0)
            {
                if (App.SiteNumber == 1)
                {
                    //Get the diamond number
                    if (WipRecord.IsLotTracable)
                    {
                        foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
                        {
                            foreach (var _lot in _comp.LotList.Where(o => !string.IsNullOrEmpty(o.ID)))
                            {
                                _diamond = Model.Product.Lot.GetDiamondNumber(_lot.ID, App.AppSqlCon);
                            }
                            if (!string.IsNullOrEmpty(_diamond) && _diamond != "error")
                            {
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(_diamond) || _diamond == "error")
                        {
                            App.GetWindow<View>().Topmost = false;
                            _diamond = DiamondEntry.Show();
                            App.GetWindow<View>().Topmost = true;
                        }
                    }

                    //Check to see if there is an associated NCR
                    var _ncr = string.Empty;
                    if (DefectList.Count() > 0)
                    {
                        foreach (var _defect in DefectList.Where(o => o.IsSelected))
                        {
                            _ncr += string.IsNullOrEmpty(_ncr) ? _defect.FormId.ToString() : $"-{_defect.FormId}";
                        }
                    }

                    //Print the travel card
                    TravelCard.Create("", "technology#1",
                        WipRecord.WipWorkOrder.Product.SkuNumber,
                        WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "",
                        WipRecord.WipWorkOrder.Product.SkuDescription,
                        _diamond,
                        _wQty,
                        WipRecord.WipWorkOrder.Product.Uom,
                        _ncr,
                        deviation:WipRecord.WipWorkOrder.IsDeviated
                        );
                    if (_isStandard)
                    {
                        switch (parameter.ToString())
                        {
                            case "T":
                                TravelCard.PrintPDF(FormType.Portrait, App.GlobalConfig.FirstOrDefault(o => o.Site == App.Facility).MaterialCard);
                                break;
                            case "R":
                                TravelCard.PrintPDF(FormType.Landscape, App.GlobalConfig.FirstOrDefault(o => o.Site == App.Facility).ReferenceCard);
                                break;
                        }
                    }
                    else
                    {
                        TravelCard.PrintZPL();
                    }
                }
                else
                {
                    if (WipRecord.WipWorkOrder.PickList.Count(o => o.InventoryType == "RC") > 0)
                    {
                        if (WipRecord.WipWorkOrder.PickList.FirstOrDefault(o => o.InventoryType == "RC").IsLotTrace)
                        {
                            foreach (var _part in WipRecord.WipWorkOrder.PickList.Where(o => o.InventoryType == "RC" && o.IsLotTrace))
                            {
                                var _counter = 0;
                                foreach (var _comp in WipRecord.ComponentList.Where(o => o.ProductNumber == _part.ProductNumber))
                                {
                                    foreach (var _lot in _comp.LotList.Where(o => o.Valid))
                                    {
                                        CompoundPart[_counter] = _comp.ProductNumber;
                                        CompoundLot[_counter] = _lot.ID;
                                        _counter++;
                                    }
                                }
                            }
                        }
                    }
                    TravelCard.Create("", "",
                        WipRecord.WipWorkOrder.Product.SkuNumber,
                        WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "",
                        WipRecord.WipWorkOrder.Product.SkuDescription,
                        "",
                        _wQty,
                        WipRecord.WipWorkOrder.Product.Uom,
                        "",
                        int.TryParse(Weight.ToString(), out int i) ? i : 0,
                        WipRecord.Submitter,
                        CompoundPart,
                        CompoundLot
                        );
                    TravelCard.Display(FormType.CoC, App.GlobalConfig.FirstOrDefault(o => o.Site == "Arlington").MaterialCard);
                }
            }
            else
            {
                if (App.SiteNumber == 1)
                {
                    if (_diamond == string.Empty && WipRecord.IsLotTracable)
                    {
                        App.GetWindow<View>().Topmost = false;
                        _diamond = DiamondEntry.Show();
                        App.GetWindow<View>().Topmost = true;
                    }
                    foreach (var _lot in LotList)
                    {
                        var _ncr = Model.Quality.QmsForm.GetNcrId(_lot, App.AppSqlCon);
                        TravelCard.Create("", "technology#1",
                            WipRecord.WipWorkOrder.Product.SkuNumber,
                            _lot,
                            WipRecord.WipWorkOrder.Product.SkuDescription,
                            _diamond,
                            _wQty,
                            WipRecord.WipWorkOrder.Product.Uom,
                            _ncr,
                            deviation:WipRecord.WipWorkOrder.IsDeviated);
                        switch (parameter.ToString())
                        {
                            case "T":
                                TravelCard.PrintPDF(FormType.Portrait, App.GlobalConfig.FirstOrDefault(o => o.Site == App.Facility).MaterialCard);
                                break;
                            case "R":
                                TravelCard.PrintPDF(FormType.Landscape, App.GlobalConfig.FirstOrDefault(o => o.Site == App.Facility).ReferenceCard);
                                break;
                        }
                    }
                }
                else
                {
                    TravelCard.Create("", "",
                        WipRecord.WipWorkOrder.Product.SkuNumber,
                        WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "",
                        WipRecord.WipWorkOrder.Product.SkuDescription,
                        "",
                        _wQty,
                        WipRecord.WipWorkOrder.Product.Uom,
                        "",
                        int.TryParse(Weight.ToString(), out int i) ? i : 0,
                        WipRecord.Submitter
                        );
                    TravelCard.Display(FormType.CoC, App.GlobalConfig.FirstOrDefault(o => o.Site == "Arlington").MaterialCard);
                }
            }
        }
        private bool MPrintCanExecute(object parameter)
        {
            if (App.SiteNumber == 2)
            {
                return Weight != null && Weight > 0;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Wip Stickers Print ICommand

        public ICommand WipStickerPrintICommand
        {
            get
            {
                if (_wPrint == null)
                {
                    _wPrint = new RelayCommand(WipStickerPrintExecute, WipStickerPrintCanExecute);
                }
                return _wPrint;
            }
        }

        private void WipStickerPrintExecute(object parameter)
        {
            var _fabricLot = new string[4];
            foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
            {
                var _counter = 0;
                foreach (var _lot in _comp.LotList.Where(o => !string.IsNullOrEmpty(o.ID)))
                {
                    _fabricLot[_counter] = _lot.ID;
                    _counter++;
                }
            }
            foreach (var _part in WipRecord.WipWorkOrder.PickList.Where(o => (o.InventoryType == "RC" || o.InventoryType == "CS") && o.IsLotTrace))
            {
                var _counter = 0;
                foreach (var _comp in WipRecord.ComponentList.Where(o => o.ProductNumber == _part.ProductNumber))
                {
                    foreach (var _lot in _comp.LotList.Where(o => o.Valid))
                    {
                        CompoundLot[_counter] = _lot.ID;
                        _counter++;
                    }
                }
            }
            var _sticker = new WipSticker(
                WipRecord.WipWorkOrder.SalesOrder.CustomerName
                , WipRecord.WipWorkOrder.SalesOrder.CustomerNumber
                , WipRecord.WipWorkOrder.Product.SkuNumber
                , WipRecord.WipWorkOrder.Product.SkuDescription
                , WipRecord.WipWorkOrder.Product.Uom
                , WipRecord.WipLot.LotNumber
                , _fabricLot
                , CompoundLot
                , int.Parse(WipRecord.WipQty.ToString())
                , int.Parse(Weight.ToString())
                , WipRecord.WipWorkOrder.SalesOrder.SalesNumber
                , "" //TODO: add in customer order
                , WipRecord.WipWorkOrder.OrderNumber);
            var _result = _sticker.Print(1);
            if (_result.FirstOrDefault().Key)
            {
                MessageBox.Show(_result.FirstOrDefault().Value, "Zebra Printer Error");
            }
        }
        private bool WipStickerPrintCanExecute(object parameter) => Weight != null && Weight > 0;

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
            WipRecord.CrewList.Remove(WipRecord.CrewList.FirstOrDefault(c => c.ErpId.ToString() == parameter.ToString()));
            foreach (var _emp in WipRecord.CrewList)
            {
                _emp.ListId = WipRecord.CrewList.IndexOf(_emp);
            }
        }

        #endregion

        #region Remove Scrap List Item ICommand

        public ICommand RemoveScrapICommand
        {
            get
            {
                if (_removeScrap == null)
                {
                    _removeScrap = new RelayCommand(RemoveScrapExecute, RemoveScrapCanExecute);
                }
                return _removeScrap;
            }
        }

        private void RemoveScrapExecute(object parameter)
        {
            var _scr = (Scrap)parameter;
            WipRecord.ScrapList.Remove(WipRecord.ScrapList.FirstOrDefault(c => c.ID == _scr.ID));
            WipQuantity = "-987654";
        }
        private bool RemoveScrapCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion

        #region Add Scrap List Item ICommand

        public ICommand AddScrapICommand
        {
            get
            {
                if (_addScrap == null)
                {
                    _addScrap = new RelayCommand(AddScrapExecute, AddScrapCanExecute);
                }
                return _addScrap;
            }
        }

        private void AddScrapExecute(object parameter)
        {
            var _newId = WipRecord.ScrapList.Count().ToString();
            WipRecord.ScrapList.Add(new Scrap(_newId, WipLot, WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Product.SkuNumber, 'P'));
            OnPropertyChanged(nameof(WipRecord));
        }
        private bool AddScrapCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion

        #region Remove Component Scrap List Item ICommand

        public ICommand RemoveCompScrapICommand
        {
            get
            {
                if (_removeCompScrap == null)
                {
                    _removeCompScrap = new RelayCommand(RemoveCompScrapExecute, RemoveCompScrapCanExecute);
                }
                return _removeCompScrap;
            }
        }

        private void RemoveCompScrapExecute(object parameter)
        {
            if (parameter.GetType() == typeof(Scrap))
            {
                var _scrapParam = (Scrap)parameter;
                foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
                {
                    if (_comp.LotList.Count(o => o.ID == _scrapParam.LotId) > 0)
                    {
                        var _scrapObj = _comp.LotList.FirstOrDefault(o => o.ID == _scrapParam.LotId).ScrapCollection.FirstOrDefault(o => o.ID == _scrapParam.ID);
                        _comp.LotList.FirstOrDefault(o => o.ID == _scrapParam.LotId).ScrapCollection.Remove(_scrapObj);
                        for (int i = 0; i < _comp.LotList.FirstOrDefault(o => o.ID == _scrapParam.LotId).ScrapCollection.Count(); i++)
                        {
                            _comp.LotList.FirstOrDefault(o => o.ID == _scrapParam.LotId).ScrapCollection[i].ID = i.ToString();
                        }
                        OnPropertyChanged(nameof(WipRecord));
                        break;
                    }
                }
            }
        }
        private bool RemoveCompScrapCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion

        #region Add Component Scrap List Item ICommand

        public ICommand AddCompScrapICommand
        {
            get
            {
                if (_addCompScrap == null)
                {
                    _addCompScrap = new RelayCommand(AddCompScrapExecute, AddCompScrapCanExecute);
                }
                return _addCompScrap;
            }
        }

        private void AddCompScrapExecute(object parameter)
        {
            if (parameter.GetType() == typeof(Lot))
            {
                var _lot = (Lot)parameter;
                foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
                {
                    if (_comp.LotList.Count(o => o.ID == _lot.ID) > 0)
                    {
                        var _id = _comp.LotList.FirstOrDefault(o => o.ID == _lot.ID).ScrapCollection.Count();
                        _comp.LotList.FirstOrDefault(o => o.ID == _lot.ID).ScrapCollection.Add(new Scrap(_id.ToString(), _lot.ID, WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Product.SkuNumber, 'C'));
                        OnPropertyChanged(nameof(WipRecord));
                        break;
                    }
                }
            }
        }
        private bool AddCompScrapCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion

        #region Remove Component List Item ICommand

        public ICommand RemoveCompICommand
        {
            get
            {
                if (_removeComp == null)
                {
                    _removeComp = new RelayCommand(RemoveCompExecute, RemoveCompCanExecute);
                }
                return _removeComp;
            }
        }

        private void RemoveCompExecute(object parameter)
        {
            foreach (var _comp in WipRecord.ComponentList.Where(o => o.LotTraceable))
            {
                var _record = WipRecord.ComponentList.FirstOrDefault(o => o.ProductNumber == _comp.ProductNumber).LotList.FirstOrDefault(o => o.ID == parameter.ToString());
                WipRecord.ComponentList.FirstOrDefault(o => o.ProductNumber == _comp.ProductNumber).LotList.Remove(_record);
            }
        }
        private bool RemoveCompCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion

        #region Print Barcode Labels ICommand

        public ICommand PrintBarLblICommand
        {
            get
            {
                if (_printBarLbl == null)
                {
                    _printBarLbl = new RelayCommand(PrintBarLblExecute, PrintBarLblCanExecute);
                }
                return _printBarLbl;
            }
        }

        private void PrintBarLblExecute(object parameter)
        {
            var _dmd = DiamondEntry.Show();
            new PrintBarLabels().Execute(_dmd);
        }
        private bool PrintBarLblCanExecute(object parameter) => true;

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                WipRecord = null;

                _wip = null;
                LotList = null;
                if (ApplicationTimer.Status == TimerState.Paused)
                {
                    ApplicationTimer.Resume();
                }
            }
        }
    }
}
