using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SFW.Queries
{
    public class PartDetail_ViewModel : ViewModelBase
    {
        #region Properties

        public Sku Part { get; set; }
        public ObservableCollection<Sku> MoveHistory { get; set; }

        private string _toLoc;
        public string ToLoc
        {
            get { return _toLoc; }
            set
            {
                _toLoc = value.ToUpper();
                OnPropertyChanged(nameof(ToLoc));
                OnPropertyChanged(nameof(IsLocValid));
                OnPropertyChanged(nameof(ShowSize));
            }
        }

        public string FromLoc
        {
            get { return Part?.Location; }
            set
            {
                Part.Location = value;
                OnPropertyChanged(nameof(FromLoc));
            }
        }

        public string MoveRef { get; set; }

        public string NonReason
        {
            get { return Part?.NonCon; }
            set
            {
                Part.NonCon = value;
                OnPropertyChanged(nameof(NonReason));
            }
        }

        private bool lotType;
        public bool LotType
        {
            get { return lotType; }
            set
            {
                lotType = value;
                Part = null;
                ValidSku = false;
                SkuInput = string.Empty;
                OnPropertyChanged(nameof(LotType));
            }
        }

        private bool validSku;
        public bool ValidSku
        {
            get { return validSku; }
            set
            {
                validSku = value;
                OnPropertyChanged(nameof(ValidSku));
            }
        }

        private bool nonLot;
        public bool NonLot
        {
            get { return nonLot; }
            set
            {
                nonLot = value;
                OnPropertyChanged(nameof(NonLot));
            }
        }

        private string skuInput;
        public string SkuInput
        {
            get { return skuInput; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (LotType)
                    {
                        if (!string.IsNullOrEmpty(value) && value.Length > 4 && LotType)
                        {
                            Part = new Sku(value, true, App.AppSqlCon);
                        }
                    }
                    else
                    {
                        if (value.Length > 5)
                        {
                            Part = new Sku(value, App.AppSqlCon);
                            OnPropertyChanged(nameof(FromLoc));
                        }
                    }
                }
                else
                {
                    Part = null;
                    QuantityInput = null;
                    ToLoc = string.Empty;
                    MoveRef = string.Empty;
                    OnPropertyChanged(nameof(MoveRef));
                }
                skuInput = value.ToUpper();
                ValidSku = !string.IsNullOrEmpty(Part?.SkuNumber);
                OnPropertyChanged(nameof(SkuInput));
                OnPropertyChanged(nameof(Part));
                OnPropertyChanged(nameof(SkuShow));
            }
        }
        public bool SkuShow
        {
            get { return !LotType && ValidSku; }
        }

        private int? qtyInput;
        public int? QuantityInput
        {
            get { return qtyInput; }
            set
            {
                qtyInput = value;
                OnPropertyChanged(nameof(QuantityInput));
            }
        }

        public bool IsLocValid { get { return string.IsNullOrEmpty(ToLoc) || (!string.IsNullOrEmpty(ToLoc) && Sku.IsValidLocation(ToLoc, App.AppSqlCon)); } }
        public int ShowSize { get { return IsLocValid ? 1 : 3; } }

        RelayCommand _mPrint;
        RelayCommand _move;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartDetail_ViewModel()
        {
            if (MoveHistory == null)
            {
                MoveHistory = new ObservableCollection<Sku>();
            }
        }

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
            var _lot = !LotType ? SkuInput : "";
            var _part = !LotType ? Part.SkuNumber : SkuInput;
            var _dmd = !LotType ? Sku.GetDiamondNumber(_lot, App.AppSqlCon) : "";
            var _qir = !LotType ? Lot.GetAssociatedQIR(_lot, App.AppSqlCon) : 0;
            TravelCard.Create("", "technology#1",
                _part,
                _lot,
                Part.SkuDescription,
                _dmd,
                Convert.ToInt32(QuantityInput),
                Part.Uom,
                _qir
                );
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
        private bool MPrintCanExecute(object parameter) => QuantityInput > 0;

        #endregion

        #region Move ICommand

        /// <summary>
        /// Unplanned Move Command
        /// </summary>
        public ICommand MoveICommand
        {
            get
            {
                if (_move == null)
                {
                    _move = new RelayCommand(MoveExecute, MoveCanExecute);
                }
                return _move;
            }
        }

        /// <summary>
        /// Unplanned Move Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveExecute(object parameter)
        {
            if (!LotType)
            {
                M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, Part.SkuNumber, SkuInput, Part.Uom, FromLoc, ToLoc, Convert.ToInt32(QuantityInput), MoveRef, App.ErpCon, NonReason);
            }
            else
            {
                M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, SkuInput, "", Part.Uom, FromLoc, ToLoc, Convert.ToInt32(QuantityInput), MoveRef, App.ErpCon, NonReason);
            }
            Part.SkuNumber = SkuInput;
            Part.SkuDescription = ToLoc;
            Part.TotalOnHand = Convert.ToInt32(QuantityInput);
            MoveHistory.Add(Part);
            SkuInput = string.Empty;
        }
        private bool MoveCanExecute(object parameter)
        {
            if (ValidSku && QuantityInput > 0 && IsLocValid)
            {
                if (!string.IsNullOrEmpty(ToLoc) && (ToLoc[ToLoc.Length - 1] != 'N' || (ToLoc[ToLoc.Length - 1] == 'N') && !string.IsNullOrEmpty(NonReason)))
                {
                    return Part.TotalOnHand > 0 || LotType ? !string.IsNullOrEmpty(FromLoc) : true;
                }
            }
            return false;
        }

        #endregion
    }
}
