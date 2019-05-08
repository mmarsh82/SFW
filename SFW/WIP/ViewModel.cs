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
                if (value == 0 || value == null)
                {
                    foreach (var c in WipRecord.WipWorkOrder.Bom)
                    {
                        c.WipInfo.Clear();
                        c.WipInfo.Add(new CompWipInfo(!string.IsNullOrEmpty(c.BackflushLoc), c.CompNumber));
                    }
                }
                else if (WipRecord.WipQty != value)
                {
                    foreach (var c in WipRecord.WipWorkOrder.Bom)
                    {
                        c.UpdateWipInfo(Convert.ToInt32(value));
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
        RelayCommand _removeCrew;
        RelayCommand _removeComp;

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            WipRecord = new WipReceipt(CurrentUser.FirstName, CurrentUser.LastName, woObject, App.AppSqlCon);
        }

        /// <summary>
        /// Validates that the LotQty for the components equal the main part Wip quantity
        /// </summary>
        /// <returns>Validation response as bool</returns>
        private bool ValidateComponents()
        {
            var _valid = true;
            foreach (var c in WipRecord.WipWorkOrder.Bom)
            {
                if (c.IsLotTrace)
                {
                    _valid = Math.Round(Convert.ToDouble(WipRecord.WipQty) * c.AssemblyQty, 0) == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Sum(o => o.LotQty);
                }
                if(string.IsNullOrEmpty(c.BackflushLoc) && _valid)
                {
                    _valid = c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)).Count() == c.WipInfo.Where(o => !string.IsNullOrEmpty(o.RcptLoc)).Count();
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
            var _machID = WipRecord.CrewList?.Count > 0 ? WorkOrder.GetAssignedMachineID(WipRecord.WipWorkOrder.OrderNumber, WipRecord.WipWorkOrder.Seq, App.AppSqlCon) : "";
            var _wipProc = M2kClient.M2kCommand.ProductionWip(WipRecord, WipRecord.CrewList?.Count > 0, App.ErpCon, WipRecord.IsLotTracable, _machID);
            if (_wipProc != null && _wipProc.First().Key > 0)
            {
                WipRecord.WipLot.LotNumber = _wipProc.First().Value;
                OnPropertyChanged(nameof(WipRecord));
                TQty = WipQuantity;
                WipQuantity = null;
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
                Sku.GetDiamondNumber(WipRecord.WipLot.LotNumber, App.AppSqlCon),
                _wQty,
                WipRecord.WipWorkOrder.Uom,
                Lot.GetAssociatedQIR(WipRecord.WipLot.LotNumber, App.AppSqlCon));
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
                ((Schedule.ViewModel)Controls.WorkSpaceDock.WccoDock.GetChildOfType<Schedule.View>().DataContext).RefreshSchedule();
            }
        }
    }
}
