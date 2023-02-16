using SFW.Model.Enumerations;
using System.ComponentModel;

namespace SFW.Model
{
    public class CompWipInfo : INotifyPropertyChanged
    {
        #region Properties

        private string lotNbr;
        public string LotNbr
        {
            get { return lotNbr; }
            set { lotNbr = value; OnPropertyChanged(nameof(LotNbr)); OnPropertyChanged(nameof(IsValidLot)); }
        }

        public bool IsValidLot { get; set; }

        private int? lotQty;
        public int? LotQty
        {
            get { return lotQty; }
            set { lotQty = value; OnPropertyChanged(nameof(LotQty)); }
        }

        private string rcptLoc;
        public string RcptLoc
        {
            get { return rcptLoc; }
            set { rcptLoc = value; OnPropertyChanged(nameof(RcptLoc)); }
        }

        private bool rollStatus;
        public bool RollStatus
        {
            get { return rollStatus; }
            set { rollStatus = value; OnPropertyChanged(nameof(RollStatus)); }
        }

        public bool IsQtyLocked { get; set; }
        public string PartNbr { get; set; }
        public bool IsBackFlush { get; set; }
        public int BaseQty { get; set; }
        public string Uom { get; set; }
        public int OnHandQty { get; set; }

        private int ohCalc;
        public int OnHandCalc
        {
            get { return ohCalc; }
            set { ohCalc = value; OnPropertyChanged(nameof(OnHandCalc)); }
        }

        private Complete isScrap;
        public Complete IsScrap
        {
            get { return isScrap; }
            set
            {
                isScrap = value;
                if (value == Complete.N)
                {
                    ScrapList.Clear();
                }
                else
                {
                    ScrapList.Add(new WipReceipt.Scrap { ID = $"0*{PartNbr}*{LotNbr}" });
                }
                OnPropertyChanged(nameof(IsScrap));
            }
        }

        private string baseLot;
        public string BaseLot
        {
            get { return baseLot; }
            set { baseLot = value; OnPropertyChanged(nameof(BaseLot)); }
        }

        public BindingList<WipReceipt.Scrap> ScrapList { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CompWipInfo()
        {
            IsValidLot = false;
            RcptLoc = string.Empty;
            LotQty = null;
            OnHandQty = 0;
            OnHandCalc = 0;
            BaseLot = string.Empty;
            ScrapList = new BindingList<WipReceipt.Scrap>();
        }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="hasBFLoc">Does the component have a default backflush location</param>
        /// <param name="partNbr">Part Number of the component</param>
        /// <param name="uom">Part Unit of Measure of the component</param>
        public CompWipInfo(bool hasBFLoc, string partNbr, string uom)
        {
            IsBackFlush = hasBFLoc;
            PartNbr = partNbr;
            Uom = uom;
            IsQtyLocked = false;
            ScrapList = new BindingList<WipReceipt.Scrap>();
            IsScrap = Complete.N;
            IsValidLot = false;
        }
    }
}
