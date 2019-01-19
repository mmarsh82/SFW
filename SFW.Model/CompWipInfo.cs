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

        private int? lotQty;
        public int? LotQty
        {
            get { return lotQty; }
            set { lotQty = value; OnPropertyChanged(nameof(LotQty)); }
        }

        private bool rollStatus;
        public bool RollStatus
        {
            get { return rollStatus; }
            set { rollStatus = value; OnPropertyChanged(nameof(RollStatus)); }
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
        public CompWipInfo()
        {

        }
    }
}
