using SFW.Commands;
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

        RelayCommand _wip;

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
        private bool WipCanExecute(object parameter) => WipRecord.WipQty > 0 && !string.IsNullOrEmpty(WipRecord.ReceiptLocation) && ValidateComponents();

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
            }
        }
    }
}
