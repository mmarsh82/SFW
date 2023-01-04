using SFW.Helpers;
using SFW.Model;
using System;
using System.Windows.Input;

namespace SFW.Queries
{
    public class UnplanScrap_ViewModel : ViewModelBase
    {
        #region Properties

        private string lot;
        public string LotNbr
        {
            get
            { return lot; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    validLot = Lot.IsValid(value);
                    if (validLot)
                    {
                        //TODO: add in the sku and work order pull
                    }
                }
                else
                {
                    validLot = false;
                }
                lot = value;
                OnPropertyChanged(nameof(LotNbr));
            }
        }
        private bool validLot;

        private int? scrapQty;
        public string ScrapQty
        {
            get
            { return scrapQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    scrapQty = i;
                }
                else
                {
                    scrapQty = null;
                }
                OnPropertyChanged(nameof(ScrapQty));
            }
        }

        private string part;
        public string PartNbr
        {
            get
            { return part; }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    validPart = Sku.IsValidSkuQuantity(value, Location, Convert.ToInt32(scrapQty));
                }
                else
                {
                    validPart = false;
                }
                part = value;
                OnPropertyChanged(nameof(PartNbr));
            }
        }
        private bool validPart;
        public string QirNbr { get; set; }

        private string loc;
        public string Location
        {
            get
            { return loc; }
            set
            { loc = value.ToUpper(); OnPropertyChanged(nameof(Location)); }
        }
        public string WoNbr { get; set; }

        private bool nonLot;
        public bool NonLotPart
        {
            get
            { return nonLot; }
            set
            {
                if (value)
                {
                    LotNbr = QirNbr = PartNbr = ScrapQty = null;
                }
                else
                {

                }
                nonLot = value;
                OnPropertyChanged(nameof(NonLotPart));
            }
        }

        RelayCommand _scrapSubmit;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UnplanScrap_ViewModel()
        {
            validLot = false;
            validPart = false;
            NonLotPart = false;
        }

        #region Unplanned Scrap Submit ICommand

        public ICommand ScrapSubmitICommand
        {
            get
            {
                if (_scrapSubmit == null)
                {
                    _scrapSubmit = new RelayCommand(ScrapSubmitExecute, ScrapSubmitCanExecute);
                }
                return _scrapSubmit;
            }
        }

        private void ScrapSubmitExecute(object parameter)
        {
            if (string.IsNullOrEmpty(LotNbr))
            {
                M2kClient.M2kCommand.InventoryAdjustment(CurrentUser.DomainUserName, $"{WoNbr}*{QirNbr}", PartNbr, M2kClient.AdjustCode.QSC, 'S', Convert.ToInt32(scrapQty), Location, $"0{App.SiteNumber}", App.ErpCon);
            }
            else
            {
                M2kClient.M2kCommand.InventoryAdjustment(CurrentUser.DomainUserName, $"{WoNbr}*{QirNbr}", PartNbr, M2kClient.AdjustCode.QSC, 'S', Convert.ToInt32(scrapQty), Location, $"0{App.SiteNumber}", App.ErpCon, LotNbr);
            }
        }
        private bool ScrapSubmitCanExecute(object parameter)
        {
            return NonLotPart ? validPart && scrapQty > 0 && !string.IsNullOrEmpty(QirNbr) : validLot && scrapQty > 0 && !string.IsNullOrEmpty(QirNbr);
        }

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _scrapSubmit = null;
            }
        }
    }
}
