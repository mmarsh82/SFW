using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Linq;
using System.Windows.Input;

//Created by Michael Marsh 10-23-18

namespace SFW.WIP
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WipReceipt WipRecord { get; set; }

        public int? WipQuantity
        {
            get { return WipRecord.WipQty; }
            set
            {
                if (WipRecord.WipQty != value)
                {
                    foreach (Component comp in WipRecord.WipWorkOrder.Bom)
                    {
                        comp.UpdateWipInfo(Convert.ToInt32(value));
                    }
                }
                WipRecord.WipQty = value;
                OnPropertyChanged(nameof(WipQuantity));
            }
        }
        public bool HasCrew { get { return WipRecord.CrewList != null; } }

        private int? tQty;
        public int? TQty
        {
            get { return tQty; }
            set { tQty = value; OnPropertyChanged(nameof(TQty)); }
        }

        RelayCommand _wip;
        RelayCommand _mPrint;

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            WipRecord = new WipReceipt(CurrentUser.DomainUserName, woObject, App.AppSqlCon);
        }

        /// <summary>
        /// Validates that the LotQty for the components equal the main part Wip quantity
        /// </summary>
        /// <returns>Validation response as bool</returns>
        private bool ValidateComponents()
        {
            var _valid = true;
            foreach (Component comp in WipRecord.WipWorkOrder.Bom)
            {
                if (comp.IsLotTrace)
                {
                    _valid = Math.Round(Convert.ToDouble(WipRecord.WipQty) * comp.AssemblyQty, 0) == comp.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty);
                }
                if(string.IsNullOrEmpty(comp.BackflushLoc) && _valid)
                {
                    _valid = comp.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Count() == comp.WipInfo.Where(o => !string.IsNullOrEmpty(o.RcptLoc)).Count();
                }
                if (!_valid)
                {
                    return _valid;
                }
            }
            return _valid;
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
            //TODO: add in the location and lot validation
            var _wipProc = M2kClient.M2kCommand.ProductionWip(WipRecord, App.ErpCon);
            if (_wipProc != null && _wipProc.First().Key > 0)
            {
                WipRecord.WipLot.LotNumber = _wipProc.First().Value;
                OnPropertyChanged(nameof(WipRecord));
            }
        }
        private bool WipCanExecute(object parameter) => WipRecord?.WipQty > 0 && !string.IsNullOrEmpty(WipRecord?.ReceiptLocation) && ValidateComponents();

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
            TravelCard.Create("", "technology#1",
                WipRecord.WipWorkOrder.SkuNumber,
                WipRecord.WipLot.LotNumber,
                WipRecord.WipWorkOrder.SkuDescription,
                Sku.GetDiamondNumber(WipRecord.WipWorkOrder.Bom, App.AppSqlCon),
                _wQty,
                WipRecord.WipWorkOrder.Uom,
                Lot.GetAssociatedQIR(WipRecord.WipLot.LotNumber, App.AppSqlCon),
                CurrentUser.DisplayName);
            switch(parameter.ToString())
            {
                case "T":
                    TravelCard.Display();
                    break;
                case "R":
                    TravelCard.DisplayReference();
                    break;
                    
            }
        }
        private bool MPrintCanExecute(object parameter) => true;

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                WipRecord.WipWorkOrder = null;
                WipRecord = null;
                _wip = null;
            }
        }
    }
}
