using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                        foreach (var c in WipRecord.WipWorkOrder.Bom)
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
                        foreach (var c in WipRecord.WipWorkOrder.Bom)
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

        private bool vLoc;
        public bool ValidLocation
        {
            get { return vLoc; }
            set { vLoc = value; OnPropertyChanged(nameof(ValidLocation)); }
        }

        public bool IsLotTrace
        {
            get { return WipRecord.IsLotTracable || WipRecord.WipWorkOrder.Bom.Count(o => o.IsLotTrace) > 0; }
        }

        public ObservableCollection<string> ScrapReasonCollection { get; set; }

        private List<string> _lotList;

        RelayCommand _wip;
        RelayCommand _mPrint;
        RelayCommand _removeCrew;
        RelayCommand _removeComp;
        RelayCommand _removeScrap;
        RelayCommand _removeCompScrap;
        RelayCommand _addScrap;
        RelayCommand _addCompScrap;

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            WipRecord = new WipReceipt(CurrentUser.FirstName, CurrentUser.LastName, woObject, App.AppSqlCon);
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
            foreach (var c in WipRecord.WipWorkOrder.Bom.Where(o => o.IsLotTrace))
            {
                c.WipInfo[0].ScrapList.ListChanged += ScrapList_ListChanged;
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
                var _validLoc = true;
                var _validQty = true;
                var _validScrap = false;
                foreach (var c in WipRecord.WipWorkOrder.Bom.Where(o => o.IsLotTrace))
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
                            if (w.ScrapCollection.Count() != w.ScrapCollection.Count(o => int.TryParse(o.Quantity, out int i)))
                            {
                                return false;
                            }
                            foreach (var s in w.ScrapCollection.Where(o => int.TryParse(o.Quantity, out int i)))
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
                        _validQty = Math.Round(Convert.ToDouble(WipRecord.WipQty + WipRecord.ScrapList.Sum(o => Convert.ToInt32(o.Quantity))) * c.AssemblyQty, 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty);
                    }
                    else
                    {
                        _validQty = Multi
                            ? Math.Round(Convert.ToDouble(WipRecord.WipQty) * c.AssemblyQty * Convert.ToDouble(WipRecord.RollQty), 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty)
                            : Math.Round(Convert.ToDouble(WipRecord.WipQty) * c.AssemblyQty, 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty);
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
            if (WipReceipt.ValidLocation(WipRecord.ReceiptLocation, App.AppSqlCon))
            {
                var _machID = WipRecord.CrewList?.Count > 0 ? WorkOrder.GetAssignedMachineID(WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Seq, App.AppSqlCon) : "";
                var _wipProc = M2kClient.M2kCommand.ProductionWip(WipRecord, WipRecord.CrewList?.Count > 0, App.ErpCon, WipRecord.IsLotTracable, _machID);
                if (_wipProc != null && _wipProc.First().Key > 0)
                {
                    if (_wipProc.First().Value != null)
                    {
                        WipRecord.WipLot.LotNumber = _wipProc.First().Value.Contains("*") || !WipRecord.IsLotTracable ? "Mulitple" : _wipProc.First().Value;
                        _lotList = _wipProc.First().Value.Contains("*") ? _wipProc.First().Value.Split('*').ToList() : null;
                    }
                    else
                    {
                        WipRecord.WipLot.LotNumber = "NonLotWip";
                        _lotList = null;
                    }
                    WipRecord.IsScrap = Model.Enumerations.Complete.N;
                    WipRecord.IsReclaim = Model.Enumerations.Complete.N;
                    OnPropertyChanged(nameof(WipRecord));
                    TQty = Lot.IsValid(WipRecord.WipLot.LotNumber, App.AppSqlCon)
                        ? Lot.GetLotOnHandQuantity(WipRecord.WipLot.LotNumber, WipRecord.ReceiptLocation, App.AppSqlCon) 
                        : Convert.ToInt32(WipQuantity);
                    WipQuantity = null;
                    Multi = false;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("You have entered an invalid receipt location.\nPlease double check your entery and try again."
                    ,"Invalid Location"
                    ,System.Windows.MessageBoxButton.OK
                    ,System.Windows.MessageBoxImage.Warning);
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
                    var _locValid = !string.IsNullOrEmpty(WipRecord.ReceiptLocation) && WipReceipt.ValidLocation(WipRecord.ReceiptLocation, App.AppSqlCon);
                    var _lotValid = WipRecord.IsLotTracable ? !string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) && Lot.LotValidation(WipRecord.WipLot.LotNumber, WipRecord.WipWorkOrder.SkuNumber, App.AppSqlCon) : true;
                    if (WipRecord.WipQty > 0)
                    {
                        _baseValid = _locValid && (string.IsNullOrEmpty(WipRecord.WipLot.LotNumber) || _lotValid) && ValidateComponents();
                    }
                    else if (WipRecord.WipQty == 0)
                    {
                        if (WipRecord.IsReclaim == Model.Enumerations.Complete.Y)
                        {
                            _baseValid = _locValid;
                        }
                        else if (WipRecord.IsScrap == Model.Enumerations.Complete.Y)
                        {
                            _baseValid = _locValid && _lotValid;
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
            if (_lotList == null || _lotList.Count == 0)
            {
                TravelCard.Create("", "technology#1",
                WipRecord.WipWorkOrder.SkuNumber,
                WipRecord.WipLot.LotNumber == "NonLotWip" ? "" : WipRecord.WipLot.LotNumber,
                WipRecord.WipWorkOrder.SkuDescription,
                Sku.GetDiamondNumber(WipRecord.WipLot.LotNumber, App.AppSqlCon),
                _wQty,
                WipRecord.WipWorkOrder.Uom,
                Lot.GetAssociatedQIR(WipRecord.WipLot.LotNumber, App.AppSqlCon));
                switch (parameter.ToString())
                {
                    case "T":
                        TravelCard.Display(FormType.Portrait);
                        break;
                    case "R":
                        TravelCard.Display(FormType.Landscape);
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
                    Sku.GetDiamondNumber(_lot, App.AppSqlCon),
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
            WipRecord.WipWorkOrder.Bom.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault()
                .WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapCollection.Remove(
                WipRecord.WipWorkOrder.Bom.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault()
                .WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapCollection.FirstOrDefault(o => o.ID == ((WipReceipt.Scrap)parameter).ID));
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
            var _newID = WipRecord.WipWorkOrder.Bom.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault().WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapCollection.Count;
            WipRecord.WipWorkOrder.Bom.Where(o => o.CompNumber == _scrArray[1]).FirstOrDefault()
                .WipInfo.Where(o => o.LotNbr == _scrArray[2]).FirstOrDefault().ScrapCollection.Add(new WipReceipt.Scrap { ID = $"{_newID}*{_scrArray[1]}*{_scrArray[2]}" });
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
            foreach (var c in WipRecord.WipWorkOrder.Bom)
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
                ((Schedule.ViewModel)Controls.WorkSpaceDock.WccoDock.GetChildOfType<Schedule.View>().DataContext).RefreshSchedule();
            }
        }
    }
}
