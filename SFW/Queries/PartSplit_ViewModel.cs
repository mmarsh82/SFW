using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                }
                _lot = value;
                OnPropertyChanged(nameof(LotNumber));
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
                        SplitLotList.Add(new Lot { LotNumber = $"{LotNumber}{Convert.ToChar(_alphaCount)}", TransactionQty = Convert.ToInt32(value )/ i });
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

        private int? _lQty;
        public string LotQuantity
        {
            get
            { return _lQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _lQty = i;
                }
                else
                {
                    _lQty = null;
                }
                OnPropertyChanged(nameof(LotQuantity));
            }
        }

        private string _note;
        public string VarienceNote
        {
            get { return _note; }
            set { _note = value; OnPropertyChanged(nameof(VarienceNote)); }
        }
        public ObservableCollection<Lot> SplitLotList { get; set; }

        RelayCommand _split;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartSplit_ViewModel()
        {
            SplitLotList = new ObservableCollection<Lot>(new List<Lot>());
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
            M2kClient.M2kCommand.InventoryAdjustment("SFW SPLIT", $"Split into {RollQuantity} rolls", _part, M2kClient.AdjustCode.CC, 'R', Convert.ToInt32(LotQuantity), _loc, App.ErpCon, LotNumber);
            foreach (var l in SplitLotList)
            {
                M2kClient.M2kCommand.InventoryAdjustment("SFW SPLIT", $"Split from {LotNumber}", _part, M2kClient.AdjustCode.CC, 'A', l.TransactionQty, _loc, App.ErpCon, l.LotNumber);
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
            ValidLot && int.TryParse(RollQuantity, out int i) && int.TryParse(LotQuantity, out int j) && j > 0 && 
            SplitLotList.Count() == SplitLotList.Count(o => !string.IsNullOrEmpty(o.LotNumber) && o.TransactionQty > 0);

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
