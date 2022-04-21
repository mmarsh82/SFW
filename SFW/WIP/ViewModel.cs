using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
                        foreach (var c in WipRecord.WipWorkOrder.Picklist)
                        {
                            c.WipInfo.Clear();
                            c.WipInfo.Add(new CompWipInfo(!string.IsNullOrEmpty(c.BackflushLoc), c.CompNumber, c.CompUom));
                        }
                    }
                    else if (WipRecord.WipQty != _wipStr)
                    {
                        if (_wipStr == -1)
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
                    IsLotValid = WipRecord.WipWorkOrder.Operation == "10"
                        ? Lot.LotValidation(value, WipRecord.WipWorkOrder.SkuNumber, WipRecord.WipWorkOrder.OrderNumber)
                        : Lot.LotValidation(value, WipRecord.WipWorkOrder.SkuNumber);
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
                    IsLocationValid = Sku.IsValidLocation(value);
                    IsLocationEditable = true;
                }
                else if (!string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) && IsLotValid)
                {
                    value = Lot.GetLotLocation(WipRecord.WipLot.LotNumber);
                    IsLocationValid = true;
                    IsLocationEditable = false;
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
                WipQuantity = "-1";
                OnPropertyChanged(nameof(WipRecord));
            }
        }

        public Model.Enumerations.Complete Reclaim
        {
            get
            { return WipRecord.IsReclaim; }
            set
            { WipRecord.IsReclaim = value; OnPropertyChanged(nameof(Reclaim)); OnPropertyChanged(nameof(WipRecord)); }
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
                WipQuantity = "-1";
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

        public bool IsLotTrace
        {
            get { return WipRecord.IsLotTracable || WipRecord.WipWorkOrder.Picklist.Count(o => o.IsLotTrace) > 0; }
        }

        public ObservableCollection<string> ScrapReasonCollection { get; set; }

        private List<string> _lotList;

        private bool isSubmit;
        public bool IsSubmitted
        {
            get
            { return isSubmit; }
            set
            { isSubmit = value; OnPropertyChanged(nameof(IsSubmitted)); }
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

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            RefreshTimer.Stop();
            var erpCon = new string[5] { App.ErpCon.HostName, App.ErpCon.UserName, App.ErpCon.Password, App.ErpCon.UniAccount, App.ErpCon.UniService };
            var _delList = new List<CompWipInfo>();
            foreach (var pl in woObject.Picklist)
            {
                foreach(var wi in pl.WipInfo.Where(o => o.LotNbr != null))
                {
                    _delList.Add(wi);
                }
                if (_delList.Count > 0)
                {
                    foreach (var delWi in _delList)
                    {
                        pl.WipInfo.Remove(delWi);
                    }
                    _delList.Clear();
                }
            }
            WipRecord = new WipReceipt(CurrentUser.UserIDNbr, CurrentUser.FirstName, CurrentUser.LastName, woObject, erpCon);
            if (ScrapReasonCollection == null)
            {
                var _tempList = Enum.GetValues(typeof(M2kClient.AdjustCode)).Cast<M2kClient.AdjustCode>().Where(o => o != M2kClient.AdjustCode.CC && o != M2kClient.AdjustCode.REC);
                var _descList = new List<string>();
                foreach (var e in _tempList)
                {
                    _descList.Add(e.GetDescription());
                }
                ScrapReasonCollection = new ObservableCollection<string>(_descList);
            }
            _lotList = new List<string>();
            foreach (var c in WipRecord.WipWorkOrder.Picklist?.Where(o => o.IsLotTrace))
            {
                c.WipInfo[0].ScrapList.ListChanged += ScrapList_ListChanged;
            }
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
                    if (string.IsNullOrEmpty(c.BackflushLoc))
                    {
                        _validLoc = c.WipInfo.Count(o => !string.IsNullOrEmpty(o.LotNbr)) == c.WipInfo.Count(o => !string.IsNullOrEmpty(o.RcptLoc));
                    }
                    else
                    {
                        _validLoc = c.WipInfo.Where(o => !o.IsValidLot && !string.IsNullOrEmpty(o.LotNbr)).Count() == 0;
                    }
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
                                    if (!string.IsNullOrEmpty(s.Reason))
                                    {
                                        if (s.Reason == "Quality Scrap" && !string.IsNullOrEmpty(s.Reference))
                                        {
                                            _validScrap = true;
                                        }
                                        else if (s.Reason != "Quality Scrap")
                                        {
                                            _validScrap = true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
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
                WipQuantity = "-1";
            }
            if(e.ListChangedType == ListChangedType.Reset)
            {
                foreach (var c in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
                {
                    foreach(var s in c.WipInfo.Where(o => o.IsValidLot))
                    {
                        s.ScrapList.ListChanged += ScrapList_ListChanged;
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
                    _lotList = _wipProc.First().Value.Contains("*") ? _wipProc.First().Value.Split('*').ToList() : null;
                }
                else
                {
                    WipRecord.WipLot.LotNumber = "NonLotWip";
                    _lotList = null;
                }
                IsSubmitted = true;
                TQty = WipRecord.WipQty + _preOnHand;
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
                    var _locValid = !string.IsNullOrEmpty(WipRecord.ReceiptLocation) && Sku.IsValidLocation(WipRecord.ReceiptLocation);
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
                    }

                    #endregion

                    #region Scrap Validation

                    var _scrapValid = true;
                    if (WipRecord.IsScrap == Model.Enumerations.Complete.Y)
                    {
                        if (WipRecord.ScrapList.Count(o => int.TryParse(o.Quantity, out int i) && i > 0) > 0)
                        {
                            _scrapValid = WipRecord.ScrapList.Count(o => Convert.ToInt32(o.Quantity) > 0) == WipRecord.ScrapList.Count(o => !string.IsNullOrEmpty(o.Reason))
                                && WipRecord.ScrapList.Count(o => o.Reason == "Quality Scrap") == WipRecord.ScrapList.Count(o => !string.IsNullOrEmpty(o.Reference));
                        }
                        else
                        {
                            _scrapValid = false;
                        }
                    }

                    #endregion

                    var _laborValid = WipRecord.CrewList.Where(o => DateTime.TryParse(o.LastClock, out var dt) && !string.IsNullOrEmpty(o.Name) && o.IsDirect).ToList().Count() == WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.Name) && o.IsDirect);
                    var _multiValid = !WipRecord.IsMulti || (WipRecord.IsMulti && WipRecord.RollQty > 0);
                    var _reclaimValid = WipRecord.IsReclaim == Model.Enumerations.Complete.N || (WipRecord.IsReclaim == Model.Enumerations.Complete.Y && WipRecord.ReclaimQty > 0 && !string.IsNullOrEmpty(WipRecord.ReclaimReference));
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
            var _lotNbr = WipRecord.IsLotTracable ? WipRecord.WipLot.LotNumber : "";
            var _diamond = string.Empty;
            if (IsLotTrace)
            {
                //Populating the diamond number based on the Picklist WIP information that was submitted
                foreach (var w in WipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace && o.InventoryType != "HM" && o.InventoryType != "FR"))
                {
                    foreach (var l in w.WipInfo.Where(o => o.IsValidLot))
                    {
                        var _temp = Lot.GetDiamondNumber(l.LotNbr, App.AppSqlCon);
                        _diamond += _diamond == _temp ? "" : $"/{_temp}";
                    }
                }
                _diamond = _diamond.Trim('/');
                //Printing the travel card
                if (_lotList == null || _lotList.Count == 0)
                {
                    TravelCard.Create("", "technology#1",
                        WipRecord.WipWorkOrder.SkuNumber,
                        _lotNbr,
                        WipRecord.WipWorkOrder.SkuDescription,
                        _diamond,
                        _wQty,
                        WipRecord.WipWorkOrder.Uom,
                        Lot.GetAssociatedQIR(_lotNbr, App.AppSqlCon));
                    switch (parameter.ToString())
                    {
                        case "T":
                            TravelCard.PrintPDF(FormType.Portrait);
                            break;
                        case "R":
                            TravelCard.PrintPDF(FormType.Landscape);
                            break;
                    }
                }
                else
                {
                    foreach (var _lot in _lotList)
                    {
                        TravelCard.Create("", "technology#1",
                            WipRecord.WipWorkOrder.SkuNumber,
                            _lot,
                            WipRecord.WipWorkOrder.SkuDescription,
                            _diamond,
                            _wQty,
                            WipRecord.WipWorkOrder.Uom,
                            Lot.GetAssociatedQIR(_lot, App.AppSqlCon));
                        switch (parameter.ToString())
                        {
                            case "T":
                                TravelCard.PrintPDF(FormType.Portrait);
                                break;
                            case "R":
                                TravelCard.PrintPDF(FormType.Landscape);
                                break;
                        }
                    }
                }
            }
            else
            {
                TravelCard.Create("", "technology#1",
                        WipRecord.WipWorkOrder.SkuNumber,
                        "",
                        WipRecord.WipWorkOrder.SkuDescription,
                        "",
                        _wQty,
                        WipRecord.WipWorkOrder.Uom,
                        0);
                switch (parameter.ToString())
                {
                    case "T":
                        TravelCard.PrintPDF(FormType.Portrait);
                        break;
                    case "R":
                        TravelCard.PrintPDF(FormType.Landscape);
                        break;
                }
            }
        }
        private bool MPrintCanExecute(object parameter) => true;

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
            WipRecord.CrewList.Remove(WipRecord.CrewList.FirstOrDefault(c => c.IdNumber.ToString() == parameter.ToString()));
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
            WipQuantity = "-1";
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
            foreach (var c in WipRecord.WipWorkOrder.Picklist)
            {
                foreach (var w in c.WipInfo)
                {
                    if (w.LotNbr == parameter.ToString())
                    {
                        c.WipInfo.Remove(w);
                        WipQuantity = "-1";
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
                    var _temp = Lot.GetDiamondNumber(l.LotNbr, App.AppSqlCon);
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
                _lotList = null;
                RefreshTimer.Start();
                if (!RefreshTimer.IsRefreshing)
                {
                    RefreshTimer.RefreshTimerTick();
                    RefreshTimer.Reset();
                }
            }
        }
    }
}
