namespace SFW.Queries
{
    public class PartLabel_ViewModel : ViewModelBase
    {
        #region Properties

        private string _dmdNbr;
        public string DiamondNumber 
        {
            get
            { return _dmdNbr; }
            set
            {
                _dmdNbr = value;
                OnPropertyChanged(nameof(DiamondNumber));
                OnPropertyChanged(nameof(PrintParam));
            }
        }

        private int _copy;
        public int Copies
        {
            get
            { return _copy == 0 ? 1 : _copy; }
            set
            {
                _copy = value;
                OnPropertyChanged(nameof(Copies));
                OnPropertyChanged(nameof(PrintParam));
            }
        }

        public string PrintParam
        {
            get
            { return $"{DiamondNumber}*{Copies}"; }
        }

        #endregion

        /// <summary>
        /// Part Label ViewModel default constructor
        /// </summary>
        public PartLabel_ViewModel()
        { }

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
