using SFW.Commands;
using SFW.Model;
using System.Linq;
using System.Windows.Input;

namespace SFW.Queries
{
    public class PartDetail_ViewModel : ViewModelBase
    {
        #region Properties

        public Sku Part { get; set; }

        private bool lotType;
        public bool LotType
        {
            get { return lotType; }
            set
            {
                lotType = value;
                OnPropertyChanged(nameof(LotType));
            }
        }

        private bool validLot;
        public bool ValidLot
        {
            get { return validLot; }
            set
            {
                validLot = value;
                OnPropertyChanged(nameof(validLot));
            }
        }

        private bool validQir;
        public bool ValidQir
        {
            get { return validQir; }
            set
            {
                validQir = value;
                OnPropertyChanged(nameof(validQir));
            }
        }

        private char prevLot;
        private string lotNbr;
        public string LotNumber
        {
            get { return lotNbr; }
            set
            {
                if (value.Length > 5)
                {
                    Part = new Sku(value, false, App.AppSqlCon);
                    ValidLot = !string.IsNullOrEmpty(Part.SkuNumber);
                }
                if (value.Length == 11)
                {
                    value = lotNbr = $"{prevLot}{value.Last()}";
                }
                if (!string.IsNullOrEmpty(value))
                {
                    prevLot = value.Last();
                }
                lotNbr = value.ToUpper();
                OnPropertyChanged(nameof(lotNbr));
                OnPropertyChanged(nameof(Part));
            }
        }

        private string partNbr;
        public string PartNumber
        {
            get { return partNbr; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 4 && LotType)
                {
                    Part = new Sku(value, App.AppSqlCon);
                    ValidLot = !string.IsNullOrEmpty(Part.SkuNumber);
                }
                partNbr = value;
                OnPropertyChanged(nameof(PartNumber));
            }
        }

        RelayCommand _print;
        RelayCommand _move;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartDetail_ViewModel()
        {

        }

        #region Print ICommand

        /// <summary>
        /// Print Travel Command
        /// </summary>
        public ICommand PrintICommand
        {
            get
            {
                if (_print == null)
                {
                    _print = new RelayCommand(PrintExecute, PrintCanExecute);
                }
                return _print;
            }
        }

        /// <summary>
        /// Print Travel Command Execution
        /// </summary>
        /// <param name="parameter">Travel Card Type as String</param>
        private void PrintExecute(object parameter)
        {
            /*TravelCard.Create("", "technology#1",
                WipRecord.WipWorkOrder.SkuNumber,
                WipRecord.WipLot.LotNumber,
                WipRecord.WipWorkOrder.SkuDescription,
                Sku.GetDiamondNumber(WipRecord.WipLot.LotNumber, App.AppSqlCon),
                _wQty,
                WipRecord.WipWorkOrder.Uom,
                Lot.GetAssociatedQIR(WipRecord.WipLot.LotNumber, App.AppSqlCon),
                CurrentUser.DisplayName);
            switch (parameter.ToString())
            {
                case "T":
                    TravelCard.Display();
                    break;
                case "R":
                    TravelCard.DisplayReference();
                    break;

            }*/
        }
        private bool PrintCanExecute(object parameter) => true;

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
            /*var _suf = M2kClient.M2kCommand.InventoryMove("", "", 0, false, App.ErpCon);

            Skew.MoveQuantity = Quantity;
            Skew.MoveFrom = string.IsNullOrEmpty(FromLoc) ? Skew.OnHand.First().Key : FromLoc;
            Skew.MoveTo = ToLoc.ToUpper();
            Skew.NonConfReason = NonReason;
            MoveHistory.Add(Skew);
            if (_suf == 0)
            {
                MoveHistory[MoveHistory.Count - 1].MoveStatus = "Failed";
                OnPropertyChanged(nameof(MoveHistory));
            }
            else
            {
                Task.Run(() => ProcessingMove(_suf, MoveHistory.Count - 1));
            }
            Quantity = null;
            ToLoc = FromLoc = NonReason = null;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(ToLoc));
            OnPropertyChanged(nameof(FromLoc));*/
        }
        private bool MoveCanExecute(object parameter)
        {
            /*if (ValidLot && Quantity > 0 && !string.IsNullOrEmpty(ToLoc))
            {
                if (ToLoc.ToUpper()[ToLoc.Length - 1] != 'N' || (ToLoc.ToUpper()[ToLoc.Length - 1] == 'N' && !string.IsNullOrEmpty(NonReason)))
                {
                    return Skew.OnHand.Count > 1 || LotType ? !string.IsNullOrEmpty(FromLoc) : true;
                }
            }*/
            return false;
        }

        #endregion

        /// <summary>
        /// View validation that the unplanned move function is working
        /// **Only use as a task delegation**
        /// </summary>
        /// <param name="uID">Unique suffix to track</param>
        /// <param name="arrayNumber">The array location of the skew to process in the MoveHistory</param>
        /// <param name="nonReason">Non-conforming reason</param>
        private void ProcessingMove(int uID, int arrayNumber)
        {
            /*MoveHistory[arrayNumber].MoveStatus = "In Que";
            OnPropertyChanged(nameof(MoveHistory));
            while (System.IO.File.Exists($"{Properties.Settings.Default.MoveFileLocation}LOCXFERC2K.DAT{uID}"))
            {
                MoveHistory[arrayNumber].MoveStatus = "Processing";
                OnPropertyChanged(nameof(MoveHistory));
            }
            MoveHistory[arrayNumber].MoveStatus = "Verifing Record";
            OnPropertyChanged(nameof(MoveHistory));
            System.Threading.Thread.Sleep(3000);
            if (Skew.MoveFrom[Skew.MoveFrom.Length - 1] == 'N' && Skew.MoveTo[Skew.MoveTo.Length - 1] != 'N')
            {
                MoveHistory[arrayNumber].MoveStatus = "Removing N-Loc Reason";
                OnPropertyChanged(nameof(MoveHistory));
                M2k.DeleteRecord("LOT.MASTER", 42, $"{LotNbr}|P");
            }
            else if (Skew.MoveFrom[Skew.MoveFrom.Length - 1] != 'N' && Skew.MoveTo[Skew.MoveTo.Length - 1] == 'N')
            {
                MoveHistory[arrayNumber].MoveStatus = "Adding N-Loc Reason";
                OnPropertyChanged(nameof(MoveHistory));
                M2k.ModifyRecord("LOT.MASTER", 42, Skew.NonConfReason, $"{LotNbr}|P");
            }
            MoveHistory[arrayNumber].MoveStatus = "Writing Record";
            OnPropertyChanged(nameof(MoveHistory));
            if (MoveHistory[arrayNumber].LotNumber == LotNbr)
            {
                System.Threading.Thread.Sleep(5000);
                if (MoveHistory[arrayNumber].LotNumber == LotNbr)
                {
                    LotNbr = MoveHistory[arrayNumber].LotNumber;
                }
            }
            MoveHistory[arrayNumber].MoveStatus = "Complete";
            OnPropertyChanged(nameof(MoveHistory));*/
        }
    }
}
