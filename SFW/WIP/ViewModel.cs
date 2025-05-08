using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

//Created by Michael Marsh 10-23-18

namespace SFW.WIP
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WipReceipt WipRecord { get; set; }
        
        public string WipQuantity
        {
            get { return WipRecord.WipQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int _wipStr))
                {
                    if (_wipStr == 0 || value == null)
                    {
                        foreach (var c in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
                        {
                            c.WipInfo.Clear();
                            c.WipInfo.Add(new CompWipInfo(!string.IsNullOrEmpty(c.BackflushLoc), c.CompNumber, c.CompUom, App.SiteNumber, WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Seq));
                        }
                    }
                    else if (WipRecord.WipQty != _wipStr)
                    {
                        if (_wipStr == -987654)
                        {
                            _wipStr = Convert.ToInt32(WipRecord.WipQty);
                        }
                        foreach (var c in WipRecord.WipWorkOrder.Picklist)
                        {
                            var _qty = _wipStr;
                            _qty *= WipRecord.IsMulti && int.TryParse(RollQuantity, out int iRoll) ? iRoll : 1;
                            if (WipRecord.IsScrap == Model.Enumerations.Complete.Y)
                            {
                                if (WipRecord.ScrapList.Count(o => int.TryParse(o.Quantity, out int i) && i > 0) > 0)
                                {
                                    _qty += WipRecord.ScrapList.Where(o => int.TryParse(o.Quantity, out int i)).Sum(o => Convert.ToInt32(o.Quantity));
                                }
                            }
                            c.UpdateWipInfo(_qty);
                        }
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
                    IsLotValid = Lot.LotValidation(value, WipRecord.WipWorkOrder.SkuNumber);
                    /*IsLotValid = WipRecord.WipWorkOrder.Operation == "10"
                        ? Lot.LotValidation(value, WipRecord.WipWorkOrder.SkuNumber, WipRecord.WipWorkOrder.OrderNumber)
                        : Lot.LotValidation(value, WipRecord.WipWorkOrder.SkuNumber);*/
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
                    IsLocationValid = Sku.IsValidLocation(value, App.SiteNumber);
                    IsLocationEditable = true;
                }
                else if (!string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) && IsLotValid)
                {
                    var _loc = Lot.GetLotLocation(WipRecord.WipLot.LotNumber);
                    if (_loc == null)
                    {
                        IsLocationValid = Sku.IsValidLocation(value, App.SiteNumber);
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
                WipRecord.ScrapList.Add(new WipReceipt.Scrap { ID = WipRecord.ScrapList.Count().ToString() });
                WipRecord.ScrapList.ListChanged += ScrapList_ListChanged;
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
                WipRecord.ReclaimObject = new WipReceipt.Reclaim();
                if (WipRecord.WipWorkOrder.Picklist.Count(o => o.InventoryType == "RC") > 0)
                {
                    WipRecord.ReclaimObject.Parent = WipRecord.WipWorkOrder.Picklist.Where(o => o.InventoryType == "RC").FirstOrDefault().CompNumber;
                    WipRecord.ReclaimObject.ParentAssyQty = WipRecord.WipWorkOrder.Picklist.Where(o => o.InventoryType == "RC").FirstOrDefault().AssemblyQty;
                }
                else if (WipRecord.WipWorkOrder.Picklist.Count() == 1)
                {
                    var _tempComp = new Model.Component(WipRecord.WipWorkOrder.Picklist[0].CompNumber, "RC");
                    WipRecord.ReclaimObject.Parent = _tempComp.CompNumber;
                    WipRecord.ReclaimObject.ParentAssyQty = WipRecord.WipWorkOrder.Picklist[0].AssemblyQty * _tempComp.AssemblyQty;
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
            get { return WipRecord.IsLotTracable || WipRecord.WipWorkOrder.Picklist.Count(o => o.IsLotTrace) > 0; }
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
                return WipRecord.WipWorkOrder.Picklist.Count(o => o.InventoryType == "RC" || o.InventoryType == "CS") > 0 && App.SiteNumber == 2
                    ? !WipRecord.WipWorkOrder.Picklist.FirstOrDefault(o => o.InventoryType == "RC" || o.InventoryType == "CS").IsLotTrace
                    : false;
            }
        }

        RelayCommand _wip;
        RelayCommand _mPrint;
        RelayCommand _removeCrew;
        RelayCommand _removeComp;
        RelayCommand _removeScrap;
        RelayCommand _removeCompScrap;
        RelayCommand _removeReclaim;
        RelayCommand _addScrap;
        RelayCommand _addCompScrap;
        RelayCommand _addReclaim;
        RelayCommand _printBarLbl;
        RelayCommand _wPrint;

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            RefreshTimer.Stop();
            CompoundPart = new string[4];
            CompoundLot = new string[4];
            var erpCon = new string[5] { App.ErpCon.HostName, App.ErpCon.UserName, App.ErpCon.Password, App.ErpCon.UniAccount, App.ErpCon.UniService };
            foreach (var pl in woObject.Picklist.Where(o => o.IsLotTrace))
            {
                if (pl.WipInfo != null)
                {
                    pl.WipInfo.Clear();
                    pl.WipInfo.ListChanged += Model.Component.WipInfo_ListChanged;
                }
                else
                {
                    pl.WipInfo = new BindingList<CompWipInfo>();
                    pl.WipInfo.ListChanged += Model.Component.WipInfo_ListChanged;
                }
                pl.WipInfo.Add(new CompWipInfo(!string.IsNullOrEmpty(pl.BackflushLoc) ,pl.CompNumber, pl.CompUom, App.SiteNumber, woObject.OrderNumber, woObject.Seq));
                pl.WipInfo.Last().ScrapList.ListChanged += ScrapList_ListChanged;
            }
            WipRecord = new WipReceipt(new CrewMember(CurrentUser.ErpId, true), App.SiteNumber, woObject, erpCon);
            LotList = new List<string>();
            IsSubmitted = false;
            IsLotValid = IsLocationValid = IsLocationEditable = true;
        }

        /// <summary>
        /// Validates that the LotQty for the components equal the main part Wip quantity
        /// </summary>
        /// <returns>Validation response as bool</returns>
        private bool ValidateComponents()
        {
            try
            {
                var _validLoc = true;
                var _validQty = true;
                var _validScrap = false;
                foreach (var c in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
                {
                    _validLoc = string.IsNullOrEmpty(c.BackflushLoc)
                        ? c.WipInfo.Count(o => !string.IsNullOrEmpty(o.LotNbr)) == c.WipInfo.Count(o => !string.IsNullOrEmpty(o.RcptLoc))
                        : c.WipInfo.Where(o => !o.IsValidLot && !string.IsNullOrEmpty(o.LotNbr)).Count() == 0;
                    
                    if (c.WipInfo.Count(o => o.IsScrap == Model.Enumerations.Complete.Y) > 0)
                    {
                        foreach (var w in c.WipInfo.Where(o => o.IsScrap == Model.Enumerations.Complete.Y))
                        {
                            if (w.ScrapList.Count() != w.ScrapList.Count(o => int.TryParse(o.Quantity, out int i)))
                            {
                                return false;
                            }
                            foreach (var s in w.ScrapList.Where(o => int.TryParse(o.Quantity, out int i)))
                            {
                                if (Convert.ToInt32(s.Quantity) > 0)
                                {
                                    _validScrap = int.TryParse(s.Reference, out int lref);
                                    if (string.IsNullOrEmpty(w.LotNbr) && _validScrap)
                                    {
                                        _validScrap = Lot.IsValidQIR(s.Reference, WipRecord.WipWorkOrder.OrderNumber, "", w.PartNbr, App.AppSqlCon) || QmsForm.IsValid(lref);
                                    }
                                    else
                                    {
                                        _validScrap = Lot.IsValidQIR(s.Reference, WipRecord.WipWorkOrder.OrderNumber, w.LotNbr, w.PartNbr, App.AppSqlCon) || QmsForm.IsValid(lref);
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        _validScrap = true;
                    }
                    if (WipRecord.IsScrap == Model.Enumerations.Complete.Y && _validScrap)
                    {
                        _validQty = Math.Round(Convert.ToDecimal(WipRecord.WipQty + WipRecord.ScrapList.Sum(o => Convert.ToInt32(o.Quantity))) * c.AssemblyQty, 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty);
                    }
                    else
                    {
                        _validQty = Multi
                            ? Math.Round(Convert.ToDecimal(WipRecord.WipQty) * c.AssemblyQty * Convert.ToDecimal(WipRecord.RollQty), 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty)
                            : Math.Round(Convert.ToDecimal(WipRecord.WipQty) * c.AssemblyQty, 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty);
                    }
                    if (!_validLoc || !_validQty || !_validScrap)
                    {
                        return false;
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
                System.Windows.MessageBox.Show(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Happens when an item is added or changed in the Scrap Binding List property
        /// </summary>
        /// <param name="sender">BindingList<WipReceipt.Scrap> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void ScrapList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "Quantity")
            {
                WipQuantity = "-987654";
            }
            if(e.ListChangedType == ListChangedType.Reset)
            {
                if (WipRecord != null)
                {
                    foreach (var c in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
                    {
                        foreach (var s in c.WipInfo.Where(o => o.IsValidLot))
                        {
                            s.ScrapList.ListChanged += ScrapList_ListChanged;
                        }
                    }
                }
            }
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
            var _preOnHand = !string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) ? Lot.GetLotOnHandQuantity(WipRecord.WipLot.LotNumber, WipRecord.ReceiptLocation) : 0;
            var _machID = WipRecord.CrewList?.Count > 0 ? Machine.GetMachineNumber(WipRecord.WipWorkOrder.Machine) : "";
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
                    #region Core Wip Validation

                    var _baseValid = false;
                    var _locValid = !string.IsNullOrEmpty(WipRecord.ReceiptLocation) && Sku.IsValidLocation(WipRecord.ReceiptLocation, App.SiteNumber);
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
                        _baseValid = _baseValid ? Weight > 0 : false;
                    }

                    #endregion

                    #region Scrap Validation

                    var _scrapValid = true;
                    if (WipRecord.IsScrap == Model.Enumerations.Complete.Y)
                    {
                        if (WipRecord.ScrapList.Count(o => int.TryParse(o.Quantity, out int i) && i > 0) > 0)
                        {
                            if (App.SiteNumber == 1)
                            {
                                _scrapValid = false;
                                if (WipRecord.ScrapList.Count(o => Convert.ToInt32(o.Quantity) > 0) == WipRecord.ScrapList.Count())
                                {
                                    foreach(var s in WipRecord.ScrapList)
                                    {
                                        var _ncrId = int.TryParse(s.Reference, out int nRef) ? nRef : 0;
                                        _scrapValid = (WipRecord.IsLotTracable || string.IsNullOrEmpty(WipLot))
                                            ? Lot.IsValidQIR(s.Reference, WipRecord.WipWorkOrder.OrderNumber, "", WipRecord.WipWorkOrder.SkuNumber, App.AppSqlCon) || QmsForm.IsValid(nRef)
                                            : Lot.IsValidQIR(s.Reference, WipRecord.WipWorkOrder.OrderNumber, WipLot, WipRecord.WipWorkOrder.SkuNumber, App.AppSqlCon) || QmsForm.IsValid(nRef);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _scrapValid = false;
                        }
                    }

                    #endregion

                    #region Reclaim Validation

                    var _reclaimValid = true;
                    if (WipRecord.IsReclaim == Model.Enumerations.Complete.Y)
                    {
                        _reclaimValid = int.TryParse(WipRecord.ReclaimObject.Quantity, out int rRef) ? rRef > 0 : false;
                    }

                    #endregion

                    #region Labor Validation

                    var _laborValid = true;
                    _laborValid = WipRecord.CrewList != null;
                    if (WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.Name)) == 0)
                    {
                        _laborValid = false;
                    }
                    else
                    {
                        _laborValid = WipRecord.CrewList.Where(o => DateTime.TryParse(o.InTime, out var dt) && o.IsDirect).ToList().Count() == WipRecord.CrewList.Count(o => o.IsDirect);
                    }

                    #endregion

                    var _multiValid = !WipRecord.IsMulti || (WipRecord.IsMulti && WipRecord.RollQty > 0);
                    return _baseValid && _scrapValid && _reclaimValid && _multiValid && _laborValid;
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
                System.Windows.MessageBox.Show(e.Message);
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

            //Printing the travel card logic
            if (LotList == null || LotList.Count == 0)
            {
                if (App.SiteNumber == 1)
                {
                    foreach (var _rec in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace && o.InventoryType != "HM"))
                    {
                        if (_rec.WipInfo.Where(o => !string.IsNullOrEmpty(o.BaseLot)).Count() > 0)
                        {
                            _diamond = _rec.WipInfo.FirstOrDefault(o => !string.IsNullOrEmpty(o.BaseLot)).BaseLot;
                            break;
                        }
                    }
                    if (_diamond == string.Empty && WipRecord.IsLotTracable)
                    {
                        App.GetWindow<View>().Topmost = false;
                        _diamond = DiamondEntry.Show();
                        App.GetWindow<View>().Topmost = true;
                    }
                    var _ncr = WipRecord.IsLotTracable ? QmsForm.GetNcrId(WipRecord.WipLot.LotNumber, App.AppSqlCon) : QmsForm.GetNcrId(WipRecord.WipWorkOrder.OrderNumber);
                    TravelCard.Create("", "technology#1",
                        WipRecord.WipWorkOrder.SkuNumber,
                        WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "",
                        WipRecord.WipWorkOrder.SkuDescription,
                        _diamond,
                        _wQty,
                        WipRecord.WipWorkOrder.Uom,
                        _ncr,
                        deviation:WipRecord.WipWorkOrder.IsDeviated
                        );
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
                    if (WipRecord.WipWorkOrder.Picklist.Count(o => o.InventoryType == "RC") > 0)
                    {
                        if (WipRecord.WipWorkOrder.Picklist.FirstOrDefault(o => o.InventoryType == "RC").IsLotTrace)
                        {
                            foreach (var _part in WipRecord.WipWorkOrder.Picklist.Where(o => o.InventoryType == "RC" && o.IsLotTrace))
                            {
                                var _counter = 0;
                                foreach (var _wip in _part.WipInfo.Where(o => o.IsValidLot))
                                {
                                    CompoundPart[_counter] = _wip.PartNbr;
                                    CompoundLot[_counter] = _wip.LotNbr;
                                    _counter++;
                                }
                            }
                        }
                    }
                    TravelCard.Create("", "",
                        WipRecord.WipWorkOrder.SkuNumber,
                        WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "",
                        WipRecord.WipWorkOrder.SkuDescription,
                        "",
                        _wQty,
                        WipRecord.WipWorkOrder.Uom,
                        "",
                        int.TryParse(Weight.ToString(), out int i) ? i : 0,
                        WipRecord.Submitter,
                        CompoundPart,
                        CompoundLot
                        );
                    TravelCard.Display(FormType.CoC, App.GlobalConfig.FirstOrDefault(o => o.Site == "CSI").MaterialCard);
                }
            }
            else
            {
                if (App.SiteNumber == 1)
                {
                    foreach (var _rec in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace && o.InventoryType != "HM"))
                    {
                        if (_rec.WipInfo.Where(o => !string.IsNullOrEmpty(o.BaseLot)).Count() > 0)
                        {
                            _diamond = _rec.WipInfo.FirstOrDefault(o => !string.IsNullOrEmpty(o.BaseLot)).BaseLot;
                            break;
                        }
                    }
                    if (_diamond == string.Empty && WipRecord.IsLotTracable)
                    {
                        App.GetWindow<View>().Topmost = false;
                        _diamond = DiamondEntry.Show();
                        App.GetWindow<View>().Topmost = true;
                    }
                    foreach (var _lot in LotList)
                    {
                        var _ncr = QmsForm.GetNcrId(_lot, App.AppSqlCon);
                        TravelCard.Create("", "technology#1",
                            WipRecord.WipWorkOrder.SkuNumber,
                            _lot,
                            WipRecord.WipWorkOrder.SkuDescription,
                            _diamond,
                            _wQty,
                            WipRecord.WipWorkOrder.Uom,
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
                        WipRecord.WipWorkOrder.SkuNumber,
                        WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "",
                        WipRecord.WipWorkOrder.SkuDescription,
                        "",
                        _wQty,
                        WipRecord.WipWorkOrder.Uom,
                        "",
                        int.TryParse(Weight.ToString(), out int i) ? i : 0,
                        WipRecord.Submitter
                        );
                    TravelCard.Display(FormType.CoC, App.GlobalConfig.FirstOrDefault(o => o.Site == "CSI").MaterialCard);
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
            foreach (var _comp in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
            {
                var _counter = 0;
                foreach (var _compLot in _comp.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)))
                {
                    _fabricLot[_counter] = _compLot.LotNbr;
                    _counter++;
                }
            }
            if (WipRecord.WipWorkOrder.Picklist.Count(o => (o.InventoryType == "RC" || o.InventoryType == "CS") && o.IsLotTrace) > 0)
            {
                foreach (var _part in WipRecord.WipWorkOrder.Picklist.Where(o => (o.InventoryType == "RC" || o.InventoryType == "CS") && o.IsLotTrace))
                {
                    var _counter = 0;
                    foreach (var _wip in _part.WipInfo.Where(o => o.IsValidLot))
                    {
                        CompoundLot[_counter] = _wip.LotNbr;
                        _counter++;
                    }
                }
            }
            var _sticker = new WipSticker(
                WipRecord.WipWorkOrder.SalesOrder.CustomerName
                , WipRecord.WipWorkOrder.SalesOrder.CustomerNumber
                , WipRecord.WipWorkOrder.SkuNumber
                , WipRecord.WipWorkOrder.SkuDescription
                , WipRecord.WipWorkOrder.Uom
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
                    _removeCrew = new RelayCommand(RemoveCrewExecute, RemoveCrewCanExecute);
                }
                return _removeCrew;
            }
        }

        private void RemoveCrewExecute(object parameter)
        {
            WipRecord.CrewList.Remove(WipRecord.CrewList.FirstOrDefault(c => c.ErpId.ToString() == parameter.ToString()));
        }
        private bool RemoveCrewCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

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
            var _scr = (WipReceipt.Scrap)parameter;
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
            WipRecord.ScrapList.Add(new WipReceipt.Scrap { ID = WipRecord.ScrapList.Count().ToString() });
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
            var _scrArray = ((WipReceipt.Scrap)parameter).ID.Split('*');
            WipRecord.WipWorkOrder.Picklist.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault()
                .WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapList.Remove(
                WipRecord.WipWorkOrder.Picklist.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault()
                .WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapList.FirstOrDefault(o => o.ID == ((WipReceipt.Scrap)parameter).ID));
            OnPropertyChanged(nameof(WipRecord));
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
            var _scrArray = ((WipReceipt.Scrap)parameter).ID.Split('*');
            var _newID = WipRecord.WipWorkOrder.Picklist.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault().WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapList.Count;
            WipRecord.WipWorkOrder.Picklist.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault()
                .WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapList.Add(new WipReceipt.Scrap { ID = $"{_newID}*{_scrArray[1]}*{_scrArray[2]}" });
            OnPropertyChanged(nameof(WipRecord));
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
            foreach (var c in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
            {
                foreach (var w in c.WipInfo)
                {
                    if (w.LotNbr == parameter.ToString())
                    {
                        c.WipInfo.Remove(w);
                        WipQuantity = "-987654";
                        return;
                    }
                }
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
            var _diamond = "";
            //Populating the diamond number based on the Picklist WIP information that was submitted
            foreach (var w in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace && o.InventoryType != "HM" && o.InventoryType != "FR"))
            {
                foreach (var l in w.WipInfo.Where(o => o.IsValidLot))
                {
                    var _temp = Lot.GetDiamondNumber(l.LotNbr, App.SiteNumber);
                    _diamond += _diamond == _temp ? "" : $"/{_temp}";
                }
            }
            _diamond = _diamond.Trim('/');
            new PrintBarLabels().Execute(_diamond);
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
                RefreshTimer.Start();
                if (!RefreshTimer.IsRefreshing && IsSubmitted)
                {
                    RefreshTimer.RefreshTimerTick();
                    RefreshTimer.Reset();
                }
            }
        }
    }
}
