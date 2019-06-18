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
            set { lotNbr = value; OnPropertyChanged(nameof(LotNbr)); }
        }

        public bool ValidLot { get; set; }

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

        public bool QtyLock { get; set; }
        public string PartNbr { get; set; }
        public bool IsBackFlush { get; set; }
        public int BaseQty { get; set; }
        public int? ScrapQty { get; set; }

        private string sReason;
        public string ScrapReason
        {
            get { return sReason; }
            set { sReason = value; OnPropertyChanged(nameof(ScrapReason)); }
        }

        private string sRef;
        public string ScrapReference
        {
            get { return sRef; }
            set { sRef = value; OnPropertyChanged(nameof(ScrapReference)); }
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
                    ScrapReason = string.Empty;
                    ScrapQty = null;
                    OnPropertyChanged(nameof(ScrapQty));
                    ScrapReference = string.Empty;
                }
                OnPropertyChanged(nameof(IsScrap));
            }
        }

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
        /// Defualt Constructor
        /// </summary>
        /// <param name="hasBFLoc">Does the component have a default backflush location</param>
        /// <param name="partNbr">Part Number of the component</param>
        /// <param name="uom">Part Unit of Measure of the component</param>
        public CompWipInfo(bool hasBFLoc, string partNbr)
        {
            IsBackFlush = hasBFLoc;
            PartNbr = partNbr;
            QtyLock = false;
            IsScrap = Complete.N;
            ValidLot = false;
        }
    }
}
