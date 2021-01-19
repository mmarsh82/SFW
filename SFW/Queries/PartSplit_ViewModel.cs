using SFW.Helpers;
using SFW.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SFW.Queries
{
    public class PartSplit_ViewModel : ViewModelBase
    {
        #region Properties

        private string _lot;
        public string LotNumber
        {
            get
            { return _lot; }
            set
            {
                value = value.ToUpper();
                var _valid = Lot.IsValid(value, App.AppSqlCon);
                if (ValidLot && !_valid)
                {
                    SplitLotList.Clear();
                    RollQuantity = string.Empty;
                    LotQuantity = 0;
                    LotLocation = string.Empty;
                    LotScrap = null;
                }
                else if (_valid)
                {
                    if (value.Count() >= 4)
                    {
                        _startingQty = LotQuantity = Lot.GetLotOnHandQuantity(value, App.AppSqlCon);
                        LotLocation = Lot.GetLotLocation(value, App.AppSqlCon);
                    }
                }
                ValidLot = _valid;
                _lot = value;
                OnPropertyChanged(nameof(LotNumber));
            }
        }

        private int _lotQty;
        private int _startingQty;
        public int LotQuantity
        {
            get
            { return _lotQty; }
            set
            {
                _lotQty = value;
                OnPropertyChanged(nameof(LotQuantity));
            }
        }

        private string _lotLoc;
        public string LotLocation
        {
            get
            { return _lotLoc; }
            set
            {
                _lotLoc = value;
                OnPropertyChanged(nameof(LotLocation));
            }
        }

        private bool _validLot;
        public bool ValidLot
        {
            get
            { return _validLot; }
            set
            { _validLot = value; OnPropertyChanged(nameof(ValidLot)); }
        }
        public int OnHand { get; set; }

        private int? _rQty;
        public string RollQuantity
        {
            get
            { return _rQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i) && i > 0 && ValidLot)
                {
                    var _lot = int.TryParse(LotNumber.Last().ToString(), out int _) ? LotNumber : LotNumber.Substring(0, LotNumber.Length - 1);
                    _rQty = i;
                    OnPropertyChanged(nameof(LotQuantity));
                    SplitLotList.Clear();
                    var _tempCount = i - 1;
                    var _alphaCount = 65;
                    while (_tempCount != 0)
                    {
                        while (Lot.IsValid($"{_lot}{Convert.ToChar(_alphaCount)}", App.AppSqlCon))
                        {
                            _alphaCount++;
                        }
                        SplitLotList.Add(new Lot { LotNumber = $"{_lot}{Convert.ToChar(_alphaCount)}", TransactionQty = string.Empty, Location = LotLocation });
                        _tempCount--;
                        _alphaCount++;
                    }
                }
                else
                {
                    _rQty = null;
                    SplitLotList.Clear();
                }
                OnPropertyChanged(nameof(RollQuantity));
            }
        }

        private string _note;
        public string VarienceNote
        {
            get { return _note; }
            set { _note = value; OnPropertyChanged(nameof(VarienceNote)); }
        }
        public BindingList<Lot> SplitLotList { get; set; }

        private int? _lotScrap;
        public string LotScrap
        {
            get
            { return _lotScrap.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _lotScrap = i;
                    LotQuantity = _startingQty - Convert.ToInt32(_lotScrap) - SplitLotList.Where(o => int.TryParse(o.TransactionQty, out int p)).Sum(a => Convert.ToInt32(a.TransactionQty));
                }
                else
                {
                    _lotScrap = null;
                    LotQuantity = _startingQty - SplitLotList.Where(o => int.TryParse(o.TransactionQty, out int p)).Sum(a => Convert.ToInt32(a.TransactionQty));
                }
                OnPropertyChanged(nameof(LotScrap));
            }
        }

        private string _scrapNote;
        public string ScrapNote
        {
            get
            { return _scrapNote; }
            set
            {
                _scrapNote = value;
                OnPropertyChanged(nameof(ScrapNote));
            }
        }

        private string _aNote;
        public string ActionNote
        {
            get
            { return _aNote; }
            set
            {
                _aNote = value;
                OnPropertyChanged(nameof(ActionNote));
            }
        }

        RelayCommand _split;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartSplit_ViewModel()
        {
            SplitLotList = new BindingList<Lot>();
            SplitLotList.ListChanged += SplitLotList_ListChanged;
        }

        /// <summary>
        /// SplitLotList BindingList event change method
        /// </summary>
        /// <param name="sender">Internal Lot object list</param>
        /// <param name="e">List changed event arguments</param>
        private void SplitLotList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.Name == "TransactionQty")
            {
                var _scrap = int.TryParse(LotScrap, out int i) ? Convert.ToInt32(LotScrap) : 0;
                LotQuantity = _startingQty - _scrap - ((BindingList<Lot>)sender).Where(o => int.TryParse(o.TransactionQty, out int p) && p > 0).Sum(p => Convert.ToInt32(p.TransactionQty));
            }
        }

        #region Roll Split Submit ICommand

        public ICommand SplitSubmitICommand
        {
            get
            {
                if (_split == null)
                {
                    _split = new RelayCommand(SplitExecute, SplitCanExecute);
                }
                return _split;
            }
        }

        private void SplitExecute(object parameter)
        {
            ActionNote = "Building...";
            var _part = Lot.GetSkuNumber(LotNumber, App.AppSqlCon);
            var _scrap = int.TryParse(LotScrap, out int lotInt) ? Convert.ToInt32(LotScrap) : 0;
            var _onHandDelta = _startingQty - LotQuantity;
            M2kClient.M2kCommand.InventoryAdjustment(CurrentUser.DisplayName, $"Split into {RollQuantity} rolls", _part, M2kClient.AdjustCode.CC, 'S', _onHandDelta, LotLocation, App.ErpCon, LotNumber);
            foreach (var l in SplitLotList.Where(o => int.TryParse(o.TransactionQty, out int i) && i > 0))
            {
                M2kClient.M2kCommand.InventoryAdjustment(CurrentUser.DisplayName, $"Split from {LotNumber}", _part, M2kClient.AdjustCode.CC, 'A', Convert.ToInt32(l.TransactionQty), l.Location, App.ErpCon, l.LotNumber);
            }
            ActionNote = "Scrapping...";
            if (_scrap > 0)
            {
                M2kClient.M2kCommand.InventoryAdjustment(CurrentUser.DisplayName, $"Scrap Split", _part, M2kClient.AdjustCode.CC, 'A', _scrap, "SCRAP", App.ErpCon, $"{LotNumber}Z");
                var _counter = 0;
                while (!Lot.IsValid($"{LotNumber}Z", App.AppSqlCon) || _counter >= 2000)
                {
                    _counter++;
                }
                if (_counter >= 2000)
                {
                    System.Windows.MessageBox.Show($"Splitting of lot {LotNumber} has been completed.\nThe scrap note has encountered an error, please contact IT for further assitance.", "Transaction Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    var _note = new string[1];
                    _note[0] = ScrapNote;
                    M2kClient.M2kCommand.EditMVRecord("LOT.MASTER", $"{LotNumber}Z|P", 42, _note, App.ErpCon);
                }
            }
            ActionNote = "Complete";
            System.Windows.MessageBox.Show($"Splitting of lot {LotNumber} has been completed.", "Transaction Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            foreach (object w in System.Windows.Application.Current.Windows)
            {
                if (((System.Windows.Window)w).Name == "Split_Window")
                {
                    ((System.Windows.Window)w).Close();
                }
            }
        }
        private bool SplitCanExecute(object parameter) => 
            ValidLot && int.TryParse(RollQuantity, out int i) && LotQuantity > 0 && 
            SplitLotList.Count() == SplitLotList.Count(o => !string.IsNullOrEmpty(o.LotNumber) && int.TryParse(o.TransactionQty, out int l) && l > 0) &&
            string.IsNullOrEmpty(LotScrap) || (int.TryParse(LotScrap, out int p) && p > 0 && !string.IsNullOrEmpty(ScrapNote));

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }

    }
}
