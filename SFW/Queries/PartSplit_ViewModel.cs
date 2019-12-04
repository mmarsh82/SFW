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
                if (value.Count() >= 7)
                {
                    ValidLot = Lot.IsValid(value, App.AppSqlCon);
                    _startingQty = LotQuantity = ValidLot ? Lot.GetLotOnHandQuantity(value, App.AppSqlCon) : 0;
                }
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

        public bool ValidLot { get; set; }
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
                    _rQty = i;
                    OnPropertyChanged(nameof(LotQuantity));
                    SplitLotList.Clear();
                    var _tempCount = i - 1;
                    var _alphaCount = 65;
                    while (_tempCount != 0)
                    {
                        SplitLotList.Add(new Lot { LotNumber = $"{LotNumber}{Convert.ToChar(_alphaCount)}", TransactionQty = string.Empty });
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
                LotQuantity = _startingQty - ((BindingList<Lot>)sender).Where(o => int.TryParse(o.TransactionQty, out int i) && i > 0).Sum(p => Convert.ToInt32(p.TransactionQty));
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
            var _part = Lot.GetSkuNumber(LotNumber, App.AppSqlCon);
            var _loc = Lot.GetLotLocation(LotNumber, App.AppSqlCon);
            var _onHandDelta = _startingQty - LotQuantity;
            M2kClient.M2kCommand.InventoryAdjustment("SFW SPLIT", $"Split into {RollQuantity} rolls", _part, M2kClient.AdjustCode.CC, 'S', _onHandDelta, _loc, App.ErpCon, LotNumber);
            foreach (var l in SplitLotList.Where(o => int.TryParse(o.TransactionQty, out int i) && i > 0))
            {
                M2kClient.M2kCommand.InventoryAdjustment("SFW SPLIT", $"Split from {LotNumber}", _part, M2kClient.AdjustCode.CC, 'A', Convert.ToInt32(l.TransactionQty), _loc, App.ErpCon, l.LotNumber);
            }
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
            SplitLotList.Count() == SplitLotList.Count(o => !string.IsNullOrEmpty(o.LotNumber) && int.TryParse(o.TransactionQty, out int l) && l > 0);

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
